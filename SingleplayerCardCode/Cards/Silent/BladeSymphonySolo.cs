using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SingleplayerCard.SingleplayerCardCode.Powers.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

[Pool(typeof(SilentCardPool))]
public sealed class BladeSymphonySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "BLADE_SYMPHONY";

    protected override string OriginalVanillaCardPool => "silent";
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromCard<Shiv>()];
    
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];
    
    public BladeSymphonySolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        
        if (CombatState == null)
        {
            return;
        }
        
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<BladeSymphonyPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
        else
        {
            for (var i = 0; i < DynamicVars.Cards.IntValue; i++)
            {
                await Shiv.CreateInHand(Owner, CombatState);
                await Cmd.Wait(0.1f);
            }
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
