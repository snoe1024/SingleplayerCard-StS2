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
