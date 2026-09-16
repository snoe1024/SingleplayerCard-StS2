using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

[Pool(typeof(IroncladCardPool))]
public sealed class BlazeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "BLAZE";

    protected override string OriginalVanillaCardPool => "ironclad";
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<StrengthPower>(2m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(2m),
        new CalculatedVar("StrengthRemake").WithMultiplier((card, _) => CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Count(e => e.HappenedThisTurn(card.CombatState)))
    ];

    public BlazeSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        DynamicVars.Strength.BaseValue = DroActiveForDisplay ? 2m : 5m;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            decimal total = ((CalculatedVar)DynamicVars["StrengthRemake"]).Calculate(null);
            if (total > 0m)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, total, Owner.Creature, this);
            }
        }
        else
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars.CalculationExtra.UpgradeValueBy(1m);
        }
        else
        {
            DynamicVars.Strength.UpgradeValueBy(2m);
        }
    }
}
