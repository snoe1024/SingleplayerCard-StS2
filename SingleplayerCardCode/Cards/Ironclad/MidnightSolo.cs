using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

// Original multiplayer card: MIDNIGHT (public-beta only; Rare Attack, costs 1 less per card Exhausted
// this combat by ANYONE -- see .claude/loadmap.md "ミッドナイト" for current numbers, which change as
// balance is tuned and are NOT duplicated here to avoid this comment going stale).
// Singleplayer rework (see .claude/loadmap.md "ミッドナイト"): base damage lowered so the card isn't a
// guaranteed-eventually-free finisher (Ironclad has enough cost-reduction tools, especially combined
// with Blaze, that an unlowered value routinely turned this into a 0-cost, oversized hit), and the
// cost-reduction trigger changes from "exhausted by anyone" to two triggers of our own: -1 (permanent,
// this combat) per card discarded, and -1 (this turn only) per card exhausted this turn, approximating
// "1 per card currently in the discard pile" as "1 per discard event this combat" for simplicity
// (matches vanilla's own AddThisCombat-per-event pattern for MIDNIGHT; this only diverges if something
// later removes cards from the discard pile, which is rare).
[Pool(typeof(IroncladCardPool))]
public sealed class MidnightSolo : SingleplayerCardCard
{
    private int _decreasedCostInTurn = 0;
    
    private int DecreasedCostInTurn
    {
        get
        {
            return _decreasedCostInTurn;
        }
        set
        {
            AssertMutable();
            _decreasedCostInTurn = value;
        }
    }
    
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "MIDNIGHT";

    protected override string OriginalVanillaCardPool => "ironclad";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(36m, ValueProp.Move) };

    public MidnightSolo() : base(12, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override void AfterCloned()
    {
        // base.AfterCloned() must run FIRST: SingleplayerCardCard.AfterCloned() is what actually sets
        // DroWasOnAtCreation (from the live DuplicateReworkManager resolution) at the end of its own
        // body. DroActiveForDisplay reads DroWasOnAtCreation for any non-canonical instance (every
        // instance that reaches AfterCloned at all), so reading it BEFORE calling base would only ever
        // see that field's still-default value from the shallow MemberwiseClone -- always false,
        // regardless of the player's actual setting.
        base.AfterCloned();

        if (DroActiveForDisplay)
        {
            DynamicVars.Damage.BaseValue = 36m;
        }
        else
        {
            DynamicVars.Damage.BaseValue = ModelDb.Card<Midnight>().DynamicVars.Damage.BaseValue;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithAttackerAnim(MegaCrit.Sts2.Core.Models.Characters.Ironclad.GetHeavyAnimIfApplicable(Owner.Character), MegaCrit.Sts2.Core.Models.Characters.Ironclad.GetHeavyAttackDelayIfApplicable(Owner.Character))
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .WithHitVfxSpawnedAtBase()
            .Execute(choiceContext);
    }

    public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (DroActiveForDisplay)
        {
            EnergyCost.AddThisCombat(DecreasedCostInTurn);
            DecreasedCostInTurn = 0;
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || IsClone)
        {
            return Task.CompletedTask;
        }

        int amount = CombatManager.Instance.History.Entries.OfType<CardDiscardedEntry>().Count();
        if (amount > 0)
        {
            ReduceCostBy(amount);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (DroActiveForDisplay)
        {
            // Per loadmap.md's 2026-09-13 revision this reduction amount is 1, not 2 (Blaze made a
            // -2-per-exhaust version too easy to break). DecreasedCostInTurn tracks event COUNT (not
            // total amount reduced) so BeforeSideTurnStart's AddThisCombat(DecreasedCostInTurn) reverts
            // exactly this much per event at next turn's start -- keeping the amounts here and there in
            // sync is what makes this reduction genuinely "this turn only" rather than partially
            // persisting forever.
            DecreasedCostInTurn++;
            ReduceCostBy(1);
        }
        else
        {
            ReduceCostBy(1);
        }
        return Task.CompletedTask;
    }

    private void ReduceCostBy(int amount)
    {
        base.EnergyCost.AddThisCombat(-amount);
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars.Damage.UpgradeValueBy(12m);
        }
        else
        {
            // Derive vanilla Midnight's own upgrade delta dynamically rather than hardcoding a second
            // copy of "+12" that could silently drift if vanilla ever rebalances it. ModelDb.Card<T>()
            // always returns the single shared canonical instance (CardModel.cs's own AssertMutable()
            // guards would throw if we tried to upgrade it directly), so ToMutable() -> UpgradeInternal()
            // is the same "clone it, upgrade the throwaway copy, read the result, discard it" pattern
            // vanilla itself uses for upgrade previews (see NInspectCardScreen.UpdateCardDisplay,
            // NGridCardHolder.UpdateCardModel, and CardModel.DowngradeInternal, which all do exactly
            // this). UpgradeInternal() is public and dispatches to Midnight.OnUpgrade() polymorphically,
            // so no subclass relationship to Midnight is needed here.
            //
            // Safe for Midnight specifically because its own OnUpgrade() is a pure DynamicVars
            // arithmetic call with no Owner/CombatState/RunState access -- the throwaway clone's Owner
            // stays null. Re-check this assumption before copying this pattern to a different ported
            // card's xDRO branch.
            CardModel vanillaUpgraded = ModelDb.Card<Midnight>().ToMutable();
            vanillaUpgraded.UpgradeInternal();
            DynamicVars.Damage.UpgradeValueBy(vanillaUpgraded.DynamicVars.Damage.BaseValue - DynamicVars.Damage.BaseValue);
        }
    }
}
