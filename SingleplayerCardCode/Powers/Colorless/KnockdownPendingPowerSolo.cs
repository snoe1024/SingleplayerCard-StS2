using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

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
