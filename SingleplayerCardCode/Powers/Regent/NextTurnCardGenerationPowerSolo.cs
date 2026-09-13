using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Regent;

// Backs GenerousGiftSolo's unupgraded case (see .claude/loadmap.md "寛大なる施し" 案1). Generates
// non-Upgraded Colorless cards. A separate power (NextTurnCardGenerationPlusPowerSolo) exists for
// the Upgraded case rather than sharing one power with a mutable "generate upgraded?" flag -- with
// a shared flag, playing both an unupgraded and an Upgraded copy of this card in the same turn would
// stack into a single power instance whose flag gets clobbered by whichever Apply call ran last, so
// both plays would resolve using the same (wrong, for one of them) upgrade state.
public sealed class NextTurnCardGenerationPowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        List<CardModel> cards = CardFactory.GetDistinctForCombat(player, ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint), Amount, player.RunState.Rng.CombatCardGeneration).ToList();
        foreach (CardModel card in cards)
        {
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        }

        await PowerCmd.Remove(this);
    }
}
