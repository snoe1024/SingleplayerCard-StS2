using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Silent;

// Backs StealthSolo (see .claude/loadmap.md "隠密"). Vanilla SNEAKY_POWER (Core/Models/Powers/
// SneakyPower.cs) grants Block via AfterCardPlayed whenever ANOTHER player plays an Attack card.
// - DRO on (案1): no other player, so instead grants Block whenever the owner deals damage by means
//   OTHER than an Attack card (poison, Choke, thorns-style relics, orbs, etc. -- see loadmap.md's own
//   list of non-Attack damage sources).
// - DRO off (xDRO): keeps vanilla's own AfterCardPlayed/Attack-card hook, just with the "other
//   player" restriction dropped to "the owner" instead, since there's no one else to trigger off of.
//
// Unlike a CardModel, PowerModel has no DroActiveForDisplay of its own -- it's the CARD that knows
// its frozen DRO state, not the power it applies. AfterApplied's cardSource parameter is the
// originating StealthSolo instance, so its DroActiveForDisplay is captured into this power's own
// [SavedProperty] bool once, here, rather than re-deriving it every trigger (which would need a
// stored reference back to that specific card instance anyway).
public sealed class StealthPowerSolo : SingleplayerCardPower
{
    [SavedProperty]
    public bool DroActiveForDisplay { get; private set; } = true;

    protected override string? OriginalVanillaPowerId => "SNEAKY_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.Block) };

    // Which trigger condition is active is fixed once, at apply time, by the owning card's frozen
    // DRO state -- it never changes for the rest of this instance's life. That makes it a DRO-axis
    // split (like Cards' own .descriptionRework/.descriptionXdro), not a cond()-branch on a
    // per-instance runtime role the way vanilla's {OnPlayer}/{ApplierName} are -- see
    // sts2_dev_knowledge/topics/power-text-writing-conventions.md's own triage rule for this.
    // Mirrors vanilla TemporaryStrengthPower's own Description/SmartDescriptionLocKey override
    // pattern (there branching on IsPositive instead of DRO state).
    public override LocString Description => new LocString("powers", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));

    protected override string SmartDescriptionLocKey => Id.Entry + (DroActiveForDisplay ? ".smartDescriptionRework" : ".smartDescriptionXdro");

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is SingleplayerCardCard soloCard)
        {
            DroActiveForDisplay = soloCard.DroActiveForDisplay;
        }
        return base.AfterApplied(applier, cardSource);
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (DroActiveForDisplay && dealer == Owner && (cardSource == null || cardSource.Type != CardType.Attack))
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!DroActiveForDisplay && cardPlay.Card.Owner.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
        {
            Flash();
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
        }
    }
}
