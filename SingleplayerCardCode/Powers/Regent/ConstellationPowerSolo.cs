using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Regent;

// Backs ConstellationSolo (see .claude/loadmap.md "星座" 案1). Fires once, at the start of the
// owner's next turn, then removes itself.
public sealed class ConstellationPowerSolo : SingleplayerCardPower
{
    public int Cards { get; set; }

    public int Energy { get; set; }

    public decimal Block { get; set; }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, Cards, player);
        await PlayerCmd.GainEnergy(Energy, player);
        await CreatureCmd.GainBlock(player.Creature, Block, ValueProp.Move, null);
        await PowerCmd.Remove(this);
    }
}
