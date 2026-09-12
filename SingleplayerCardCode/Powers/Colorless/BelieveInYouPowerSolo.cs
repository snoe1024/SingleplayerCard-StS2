using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs BelieveInYouSolo (see .claude/loadmap.md "お前を信じる" 案1). Active for exactly one
// (the owner's next) turn: once an Attack, a Skill, AND a Power have all been played that turn,
// grants Energy once and removes itself. If the turn ends before all three types are covered, it
// just expires without effect.
public sealed class BelieveInYouPowerSolo : SingleplayerCardPower
{
    private readonly HashSet<CardType> _typesPlayed = new();
    private bool _active;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            _active = true;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_active || cardPlay.Card.Owner != Owner.Player)
        {
            return;
        }

        if (cardPlay.Card.Type is CardType.Attack or CardType.Skill or CardType.Power)
        {
            _typesPlayed.Add(cardPlay.Card.Type);
        }

        if (_typesPlayed.Count >= 3)
        {
            await PlayerCmd.GainEnergy(Amount, Owner.Player);
            await PowerCmd.Remove(this);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
