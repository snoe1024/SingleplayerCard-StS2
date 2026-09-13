using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs KnockdownSolo (see .claude/loadmap.md "ノックダウン"). Vanilla KNOCKDOWN_POWER doubles
// damage the target takes from OTHER players THIS turn. Damage and the multiplier amount are
// identical between branches -- only the TIMING differs:
// - DRO on (案1): waits until the owner's next turn starts, doubles/triples damage taken for that
//   whole turn, then removes itself at that turn's end -- "next turn" instead of "this turn from
//   others".
// - DRO off (xDRO): matches the original -- active immediately, for the REST of the current turn.
// Like StealthPowerSolo, this has no DroActiveForDisplay of its own -- it's captured from the
// applying card via AfterApplied into a [SavedProperty] field.
public sealed class KnockdownPowerSolo : SingleplayerCardPower
{
    [SavedProperty]
    public bool DroActiveForDisplay { get; private set; } = true;

    private bool _active;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is SingleplayerCardCard soloCard)
        {
            DroActiveForDisplay = soloCard.DroActiveForDisplay;
        }
        if (!DroActiveForDisplay)
        {
            _active = true;
        }
        return base.AfterApplied(applier, cardSource);
    }

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
