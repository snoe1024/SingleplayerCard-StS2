using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

// Backs SoulboundSolo (see .claude/loadmap.md "ソウルバウンド"). Vanilla SOULBOUND_POWER (Soul
// generated -> add a Soul to a chosen ally's deck) is meaningless targeted at yourself -- there's no
// ally to pick.
// - DRO on (案1): targets a chosen ENEMY instead of an ally. The first time that enemy takes
//   unblocked Attack damage each of the owner's turns, add Amount Soul(s) to the OWNER's draw pile
//   (the applier, not this power's own Owner -- Owner here is the targeted enemy).
// - DRO off (xDRO): keeps vanilla's own Soul-generation trigger, just retargeted at the owner (self)
//   instead of "an ally" -- there being no one else to grant it to. Guarded against re-entrancy
//   (_isAddingSoul) exactly like vanilla's own SoulboundPower, since adding a Soul via
//   AddGeneratedCardsToCombat is itself a Soul generation that would otherwise re-trigger this hook.
//
// Like UnderworldPowerSolo, this has no DroActiveForDisplay of its own -- it's captured from the
// applying card via AfterApplied into a [SavedProperty] field.
public sealed class SoulboundPowerSolo : SingleplayerCardPower
{
    [SavedProperty]
    public bool DroActiveForDisplay { get; private set; } = true;

    private bool _hasTriggeredThisTurn;
    private bool _isAddingSoul;

    protected override string? OriginalVanillaPowerId => "SOULBOUND_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Soul>() };

    public override Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (cardSource is SingleplayerCardCard soloCard)
        {
            DroActiveForDisplay = soloCard.DroActiveForDisplay;
        }
        return base.AfterApplied(applier, cardSource);
    }

    public override Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (DroActiveForDisplay && side == CombatSide.Player)
        {
            _hasTriggeredThisTurn = false;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (!DroActiveForDisplay || _hasTriggeredThisTurn || target != Owner || Applier == null || !props.IsPoweredAttack() || result.UnblockedDamage <= 0m)
        {
            return;
        }

        _hasTriggeredThisTurn = true;
        Flash();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(Applier.Player, Amount, CombatState), PileType.Draw, Applier.Player, CardPilePosition.Random));
    }

    public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
    {
        if (DroActiveForDisplay || _isAddingSoul || creator == null || creator.Creature != Applier || card is not Soul)
        {
            return;
        }

        _isAddingSoul = true;
        Flash();
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardsToCombat(Soul.Create(Owner.Player, Amount, CombatState), PileType.Draw, Owner.Player, CardPilePosition.Random));
        _isAddingSoul = false;
    }
}
