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
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

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

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "MIDNIGHT";

    protected override string OriginalVanillaCardPool => "ironclad";

    // "Damage" itself is kept as a plain DamageVar purely as the gameplay-effective/cross-card-visible
    // key (OnPlay's DamageCmd.Attack reads it, and vanilla's Thrash picks a random Attack card from
    // hand and reads ITS "Damage" key directly -- see Thrash.OnPlay) -- RefreshDroBranchState below
    // keeps it synced to whichever of DamageRemake/DamageXdro is the active branch. Registering BOTH
    // branches as their own named vars (rather than only Remake being "a real DamageVar" and xDRO
    // existing solely as a literal inside RefreshDroBranchState) is what lets .descriptionRework and
    // .descriptionXdro each reference their own number directly -- correct on canonical/Card Library
    // instances too, since CanonicalVars entries need no mutation to read, unlike Damage itself.
    // DamageRemake/DamageXdro MUST themselves be DamageVar (not a plain DynamicVar) -- CardModel's
    // UpdateDynamicVarPreview loops over every var in DynamicVars and calls UpdateCardPreview on each
    // regardless of key name; DamageVar is what makes that call actually recompute PreviewValue from
    // current Strength/Vulnerable/Weak/enchantments (a plain DynamicVar's UpdateCardPreview is a
    // no-op, so :diff() -- which reads PreviewValue -- would always show the raw, un-buffed number
    // regardless of the target's current debuffs; confirmed as a real regression in actual play).
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(36m, ValueProp.Move),
        new DamageVar("DamageRemake", 36m, ValueProp.Move),
        new DamageVar("DamageXdro", ModelDb.Card<Midnight>().DynamicVars.Damage.BaseValue, ValueProp.Move)
    };

    public MidnightSolo() : base(12, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    // Pure function of DroActiveForDisplay -- see base class doc comment. DamageRemake/DamageXdro
    // each carry their own upgrade state already (see OnUpgrade), so no separate IsUpgraded branching
    // is needed here.
    protected override void RefreshDroBranchState()
    {
        DynamicVars.Damage.BaseValue = DroActiveForDisplay
            ? DynamicVars["DamageRemake"].BaseValue
            : DynamicVars["DamageXdro"].BaseValue;
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

        if (DroActiveForDisplay)
        {
            var amount = CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Select(e => e.HappenedThisTurn(CombatState) ? 2 : 1).Sum();
            if (amount > 0)
            {
                ReduceCostBy(amount);
            }
        }
        else
        {
            var amount = CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Count();
            if (amount > 0)
            {
                ReduceCostBy(amount);
            }
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        if (DroActiveForDisplay)
        {
            DecreasedCostInTurn++;
            ReduceCostBy(2);
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
            DynamicVars["DamageRemake"].UpgradeValueBy(12m);
        }
        else
        {
            CardModel vanillaUpgraded = ModelDb.Card<Midnight>().ToMutable();
            vanillaUpgraded.UpgradeInternal();
            DynamicVars["DamageXdro"].UpgradeValueBy(vanillaUpgraded.DynamicVars.Damage.BaseValue - DynamicVars["DamageXdro"].BaseValue);
        }
        RefreshDroBranchState();
    }
}
