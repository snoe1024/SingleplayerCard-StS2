using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Regent;

// Backs GenerousGiftSolo (see .claude/loadmap.md "寛大なる施し" 案1). Fires once, at the start of
// the owner's next turn, then removes itself.
public sealed class GenerousGiftPowerSolo : SingleplayerCardPower
{
    public bool GenerateUpgraded { get; set; }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        CardModel? card = CardFactory.GetDistinctForCombat(player, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint), 1, player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (card != null)
        {
            if (GenerateUpgraded)
            {
                CardCmd.Upgrade(card);
            }

            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }

        await PowerCmd.Remove(this);
    }
}
