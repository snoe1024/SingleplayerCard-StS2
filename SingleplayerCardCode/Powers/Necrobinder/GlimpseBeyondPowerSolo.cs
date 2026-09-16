using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

// Backs GlimpseBeyondSolo (see .claude/loadmap.md "彼方への一瞥" 案1). Ticks down once per owner
// turn start, adding 1 Soul directly to hand each time, until Amount reaches 0 (PowerCmd.Decrement
// removes the power automatically at that point, same as vanilla's per-turn counter powers).
public sealed class GlimpseBeyondPowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Soul>() };

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || Amount <= 0)
        {
            return;
        }

        var soul = Soul.Create(Owner.Player, 1, CombatState).First();
        await CardPileCmd.AddGeneratedCardsToCombat([soul], PileType.Hand, Owner.Player, CardPilePosition.Random);
        await PowerCmd.Decrement(this);
    }
}
