using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

// Original multiplayer card: MIDNIGHT (public-beta only per loadmap.md; Rare, 12 cost, Attack, 60(72)
// damage, costs 1 less per card Exhausted this combat by ANYONE).
// Singleplayer rework (see .claude/loadmap.md "ミッドナイト" 案1): base damage lowered to 42(56) so
// the card isn't a guaranteed-eventually-free finisher, and the cost-reduction trigger changes from
// "exhausted by anyone" to two triggers of our own: -1 (permanent, this combat) per card discarded,
// and -2 (this turn only) per card exhausted this turn, approximating "1 per card currently in the
// discard pile" as "1 per discard event this combat" for simplicity (matches vanilla's own
// AddThisCombat-per-event pattern for MIDNIGHT; this only diverges if something later removes cards
// from the discard pile, which is rare).
[Pool(typeof(IroncladCardPool))]
public sealed class MidnightSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "MIDNIGHT";

    protected override string OriginalVanillaCardPool => "ironclad";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(42m, ValueProp.Move) };

    public MidnightSolo() : base(12, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
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

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this || IsClone)
        {
            return Task.CompletedTask;
        }

        int discardedSoFar = CombatManager.Instance.History.Entries.OfType<CardDiscardedEntry>().Count();
        if (discardedSoFar > 0)
        {
            EnergyCost.AddThisCombat(-discardedSoFar);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
    {
        EnergyCost.AddThisCombat(-1);
        return Task.CompletedTask;
    }

    public override Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
    {
        EnergyCost.AddThisTurn(-2);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(14m);
    }
}
