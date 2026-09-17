using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

public sealed class UnderworldPowerSolo : SingleplayerCardPower
{
    protected override string? OriginalVanillaPowerId => "UNDERWORLD_POWER";

    public bool DroActiveForDisplay { get; private set; } = true;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<DoomPower>() };

    public override LocString Description => new LocString("powers", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is SingleplayerCardCard soloCard)
        {
            DroActiveForDisplay = soloCard.DroActiveForDisplay;
        }
        return base.AfterApplied(applier, cardSource);
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        // Must check Owner.Side not to apply damage because of self-Doom applying card like old version of Borrowed Time
        if (DroActiveForDisplay && power is DoomPower && applier == Owner && power.Owner.Side != Owner.Side && amount > 0m)
        {
            await CreatureCmd.Damage(choiceContext, power.Owner, amount, ValueProp.Unpowered, Owner);
        }
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (!DroActiveForDisplay && dealer == Owner && props.IsPoweredAttack() && result.TotalDamage > 0)
        {
            await PowerCmd.Apply<DoomPower>(choiceContext, target, result.TotalDamage, Owner, null);
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
