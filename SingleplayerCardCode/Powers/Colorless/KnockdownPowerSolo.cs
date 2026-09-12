using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs KnockdownSolo (see .claude/loadmap.md "ノックダウン" 案1). Vanilla KNOCKDOWN_POWER doubles
// damage the target takes from OTHER players this turn. Singleplayer rework instead waits until
// the owner's next turn starts, doubles/triples damage taken for that whole turn, then removes
// itself at that turn's end -- "next turn" instead of "this turn from others".
public sealed class KnockdownPowerSolo : SingleplayerCardPower
{
    private bool _active;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!_active || target != Owner || !props.IsPoweredAttack())
        {
            return 1m;
        }

        return Amount;
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == CombatSide.Player)
        {
            _active = true;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && _active)
        {
            await PowerCmd.Remove(this);
        }
    }
}
