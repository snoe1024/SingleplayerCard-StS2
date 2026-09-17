using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public sealed class GatherSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "RALLY";

    protected override string OriginalVanillaCardPool => "colorless";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(12m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("HandBlockTotal").WithMultiplier((card, _) => CardPile.GetCards(card.Owner, PileType.Hand).Where(c => c != card).SelectMany(c => c.DynamicVars.Values.OfType<BlockVar>()).Sum(v => v.BaseValue))
    };

    public GatherSolo() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            decimal total = ((CalculatedVar)DynamicVars["HandBlockTotal"]).Calculate(null);
            if (total > 0m)
            {
                await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Move, cardPlay);
            }
        }
        else
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
        else
        {
            DynamicVars.Block.UpgradeValueBy(5m);
        }
    }
}
