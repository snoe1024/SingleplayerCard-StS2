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

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(36m, ValueProp.Move),
        new DamageVar("DamageRemake", 36m, ValueProp.Move),
        new DamageVar("DamageXdro", ModelDb.Card<Midnight>().DynamicVars.Damage.BaseValue, ValueProp.Move)
    };

    public MidnightSolo() : base(12, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

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
            EnergyCost.AddThisCombat(DecreasedCostInTurn * 2);
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
            var amount = CombatManager.Instance.History.Entries.OfType<CardExhaustedEntry>().Select(e => e.HappenedThisTurn(CombatState) ? 3 : 1).Sum();
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
            ReduceCostBy(3);
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
