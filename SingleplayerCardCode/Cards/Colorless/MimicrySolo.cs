using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public sealed class MimicrySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "MIMIC";

    protected override string OriginalVanillaCardPool => "colorless";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("BestHandBlock").WithMultiplier((card, _) => CardPile.GetCards(card.Owner, PileType.Hand).Where(c => c != card && c.DynamicVars.ContainsKey("Block")).Select(c => c.DynamicVars.Block.BaseValue).DefaultIfEmpty(0m).Max())
    };

    public MimicrySolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal best = ((CalculatedVar)DynamicVars["BestHandBlock"]).Calculate(null);
        if (best > 0m)
        {
            await CreatureCmd.GainBlock(Owner.Creature, best, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
