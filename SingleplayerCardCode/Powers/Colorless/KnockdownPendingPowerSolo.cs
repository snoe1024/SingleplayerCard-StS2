using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs KnockdownSolo's Rework (案1) branch. A pure marker/timer power, applied instead of
// KnockdownPowerSolo directly so the power tooltip can unambiguously say "NEXT turn" rather than
// reusing KnockdownPowerSolo's old design, where the SAME power instance's description always read
// the same regardless of an internal "am I active yet?" flag the tooltip system couldn't see (and
// playing two Knockdowns on different turns could visibly show the wrong pending/active state).
// At the end of the enemy's turn (i.e. right as the owner's next turn is about to begin), this
// converts into an application of KnockdownPowerSolo with the same Amount, then removes itself.
public sealed class KnockdownPendingPowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy)
        {
            return;
        }

        await PowerCmd.Apply<KnockdownPowerSolo>(choiceContext, Owner, Amount, Applier, null);
        await PowerCmd.Remove(this);
    }
}
