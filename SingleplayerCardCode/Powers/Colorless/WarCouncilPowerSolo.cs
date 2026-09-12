using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs WarCouncilSolo (see .claude/loadmap.md "作戦会議" 案1). Fires once, at the start of the
// owner's next turn, then removes itself.
public sealed class WarCouncilPowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, (int)Amount, player);
        await PowerCmd.Remove(this);
    }
}
