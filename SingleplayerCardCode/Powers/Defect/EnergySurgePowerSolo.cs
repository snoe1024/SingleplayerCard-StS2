using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Defect;

// Backs EnergySurgeSolo (see .claude/loadmap.md "エナジーサージ" 案1). Ticks down once per owner
// turn start, granting 1 Energy each time, until Amount reaches 0.
public sealed class EnergySurgePowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || Amount <= 0)
        {
            return;
        }

        await PlayerCmd.GainEnergy(1, Owner.Player);
        await PowerCmd.Decrement(this);
    }
}
