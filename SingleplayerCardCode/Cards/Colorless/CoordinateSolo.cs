using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: COORDINATE (Uncommon, 1 cost, Skill). Give another player 5(8)
// Strength this turn.
// Singleplayer rework (see .claude/loadmap.md "連携" 案1): no other player, so instead grants
// Strength this turn equal to the number of cards currently in hand. Cost lowered to 0 on upgrade.
[Pool(typeof(ColorlessCardPool))]
public sealed class CoordinateSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "COORDINATE";

    protected override string OriginalVanillaCardPool => "colorless";

    public CoordinateSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int cardsInHand = CardPile.GetCards(Owner, PileType.Hand).Count();
        if (cardsInHand > 0)
        {
            await PowerCmd.Apply<CoordinatePowerSolo>(choiceContext, Owner.Creature, cardsInHand, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
