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
