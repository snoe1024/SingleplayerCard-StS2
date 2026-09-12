using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

// Backs UnderworldSolo (see .claude/loadmap.md "冥界" 案1). Vanilla UNDERWORLD_POWER converts other
// players' Attack damage into Doom applied to the target -- meaningless alone. Singleplayer rework
// reverses the causality: whenever the owner applies Doom this turn, deal damage equal to that
// amount to the doomed target. Uses ValueProp.Unpowered (not a powered Attack) so it can't chain
// into a Deathify-style attack-to-Doom conversion loop.
public sealed class UnderworldPowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<DoomPower>() };

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is DoomPower && applier == Owner && amount > 0m)
        {
            await CreatureCmd.Damage(choiceContext, power.Owner, amount, ValueProp.Unpowered, Owner);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Remove(this);
        }
    }
}
