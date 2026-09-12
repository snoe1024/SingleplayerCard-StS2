using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: RALLY (Rare, 2 cost, Skill). ALL players gain 12(17) Block
// immediately.
// Singleplayer rework (see .claude/loadmap.md "結集" 案1): no other players, so instead gain Block
// equal to the sum of every other hand card's printed Block value (BlockVar only, ignoring
// calculated/special block).
[Pool(typeof(ColorlessCardPool))]
public sealed class GatherSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public override bool GainsBlock => true;

    public GatherSolo() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal total = CardPile.GetCards(Owner, PileType.Hand)
            .Where(c => c != this)
            .SelectMany(c => c.DynamicVars.Values.OfType<BlockVar>())
            .Sum(v => v.BaseValue);

        if (total > 0m)
        {
            await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Move, cardPlay);
        }
    }
}
