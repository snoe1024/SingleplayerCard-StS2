using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs BeaconOfHopeSolo (see .claude/loadmap.md "希望の道標" 案1). Vanilla BEACON_OF_HOPE_POWER
// gives other players half of any Block the owner gains -- meaningless alone. Singleplayer rework:
// tracks Block gained during the owner's own turn, and at that turn's end banks half of it to be
// granted as Block at the start of the owner's NEXT turn.
public sealed class BeaconOfHopePowerSolo : SingleplayerCardPower
{
    private decimal _blockGainedThisTurn;
    private decimal _pendingBlock;

    protected override string? OriginalVanillaPowerId => "BEACON_OF_HOPE_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (creature == Owner && amount > 0m)
        {
            _blockGainedThisTurn += amount;
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            _pendingBlock = _blockGainedThisTurn / 2m;
            _blockGainedThisTurn = 0m;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || _pendingBlock <= 0m)
        {
            return;
        }

        decimal amount = _pendingBlock;
        _pendingBlock = 0m;
        await CreatureCmd.GainBlock(Owner, amount, ValueProp.Unpowered, null);
    }
}
