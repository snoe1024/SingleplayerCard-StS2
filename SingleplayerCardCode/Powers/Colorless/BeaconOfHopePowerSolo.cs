using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs BeaconOfHopeSolo (see .claude/loadmap.md "希望の道標" 案1). Vanilla BEACON_OF_HOPE_POWER
// gives other players half of any Block the owner gains -- meaningless alone. Singleplayer rework:
// tracks Block gained during the owner's own turn, and at that turn's end banks half of it to be
// granted as Block at the start of the owner's NEXT turn.
public sealed class BeaconOfHopePowerSolo : SingleplayerCardPower
{
    protected override string? OriginalVanillaPowerId => "BEACON_OF_HOPE_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature == Owner && props.IsCardOrMonsterMove() && cardSource != null && amount > 0m)
        {
            int num = CombatManager.Instance.History.Entries.OfType<BlockGainedEntry>().Count(e => e.HappenedThisTurn(CombatState) && e.CardPlay != null && e.CardPlay.Player.Creature == base.Owner && e.Props.IsCardOrMonsterMove() && e.CardPlay.Card != cardSource);
            if (num < Amount)
            {
                await PowerCmd.Apply<BlockNextTurnPower>(null, Owner, amount, Owner, null);
                Flash();
            }
        }
    }
}
