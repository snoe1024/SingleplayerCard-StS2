using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Defect;

// Backs ImitationLearningSolo (see .claude/loadmap.md "模倣学習" 案1). Vanilla
// IMITATION_LEARNING_POWER copies another player's Powers as they play them -- meaningless alone.
// Singleplayer rework: remembers the last Power card the owner played, and at the end of each of
// the next 2(3) turns, plays a copy of it (whichever one was most recently played counts, so this
// can cascade if the owner keeps playing new Powers).
public sealed class ImitationLearningPowerSolo : SingleplayerCardPower
{
    private CardModel? _lastPower;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner == Owner.Player && cardPlay.Card.Type == CardType.Power)
        {
            _lastPower = cardPlay.Card;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner) || Amount <= 0)
        {
            return;
        }

        if (_lastPower != null)
        {
            Flash();
            CardModel clone = _lastPower.CreateCloneForPlayer(Owner.Player);
            await CardCmd.AutoPlay(choiceContext, clone, null);
        }

        await PowerCmd.Decrement(this);
    }
}
