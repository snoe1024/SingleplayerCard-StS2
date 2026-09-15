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
using MegaCrit.Sts2.Core.Models.Powers;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

public sealed class NoEnergyGainPowerSolo : SingleplayerCardPower
{
    protected override string? OriginalVanillaPowerId => "NO_ENERGY_GAIN_POWER";
    
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side && participants.Contains(Owner))
        {
            await PowerCmd.Apply<NoEnergyGainPower>(null, Owner, 1, Owner, null);

            if (base.Amount == 1)
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<NoEnergyGainPower>(null, Owner, 1, applier, cardSource);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.Decrement(this);
        }
    }
}
