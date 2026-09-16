using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Defect;

public sealed class ImitationLearningPowerSolo : SingleplayerCardPower
{
    protected override string? OriginalVanillaPowerId => "IMITATION_LEARNING_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (Owner.Player is null)
        {
            return;
        }
        
        if (Amount <= 0 || power == this || power.Owner.Side == Owner.Side || power.Type != PowerType.Buff || amount <= 0m || Applier?.Player == null)
        {
            return;
        }

        var player = Owner.Player;
        var card = CardFactory.GetDistinctForCombat(player, player.Character.CardPool.GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint).Where(c => c.Type == CardType.Power), 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (card != null)
        {
            card.EnergyCost.SetCustomBaseCost(0);
            Flash();
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }

        await PowerCmd.Decrement(this);
    }
}
