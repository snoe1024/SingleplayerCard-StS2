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
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

// Backs UnderworldSolo (see .claude/loadmap.md "冥界"). Vanilla UNDERWORLD_POWER (damage -> Doom)
// converts other players' Attack damage into Doom applied to the target -- meaningless targeted at
// yourself, since it explicitly excludes the power owner's own damage.
// - DRO on (案1): reverses the causality entirely: whenever the owner applies Doom this turn, deal
//   damage equal to that amount to the doomed target. Uses ValueProp.Unpowered (not a powered
//   Attack) so it can't chain into a Deathify-style attack-to-Doom conversion loop.
// - DRO off (xDRO): keeps vanilla's own damage -> Doom causality, just retargeted at the owner's own
//   Attack damage instead of "other players'" (there being no one else to trigger off of).
//
// Like StealthPowerSolo, this has no DroActiveForDisplay of its own -- it's captured from the
// applying card via AfterApplied into a [SavedProperty] field.
public sealed class UnderworldPowerSolo : SingleplayerCardPower
{
    [SavedProperty]
    public bool DroActiveForDisplay { get; private set; } = true;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<DoomPower>() };

    // Causality direction is fixed once, at apply time, by the owning card's frozen DRO state -- a
    // DRO-axis split (like Cards' own .descriptionRework/.descriptionXdro), not a cond()-branch on a
    // per-instance runtime role. See StealthPowerSolo for the same pattern and its rationale.
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
        // Must check power.Owner.Side != Owner.Side explicitly: loadmap.md corrected this to say
        // "whenever you apply Doom to an ENEMY this turn" -- without the side check, a future card
        // that applies Doom to the OWNER (e.g. a Borrowed Time-style "Doom yourself for Energy" skill,
        // which existed at an earlier point in this project) would deal that same damage back to the
        // owner instead of an enemy.
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
