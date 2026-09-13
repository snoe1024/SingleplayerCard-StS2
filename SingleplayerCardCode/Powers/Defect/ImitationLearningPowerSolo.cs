using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Defect;

// Backs ImitationLearningSolo's Rework branch (see .claude/loadmap.md "模倣学習" 案1, redesigned
// 2026-09-14 -- the old 案2 replay-your-own-last-Power design was scrapped for being underwhelming).
// Applied to a chosen enemy. The first Amount times that enemy gains ANY Buff-type power (its own
// Strength/Ritual/etc, not something the player did to it), add a random Power card from the
// player's own character pool to the player's hand at 0 Energy cost, then decrement Amount --
// removing this power once it hits 0. Fires during the enemy's turn, so with 10 or fewer cards in
// hand the generated Power(s) sit there ready to play at the start of the owner's next turn.
public sealed class ImitationLearningPowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (Amount <= 0 || power == this || power.Owner != Owner || power.Type != PowerType.Buff || amount <= 0m || Applier?.Player == null)
        {
            return;
        }

        Player player = Applier.Player;
        CardModel? card = CardFactory.GetDistinctForCombat(player, player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).Where(c => c.Type == CardType.Power), 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (card != null)
        {
            card.EnergyCost.SetCustomBaseCost(0);
            Flash();
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }

        await PowerCmd.Decrement(this);
    }
}
