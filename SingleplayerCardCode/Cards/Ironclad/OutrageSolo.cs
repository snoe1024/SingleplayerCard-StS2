using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

// Original multiplayer card: OUTRAGE (Uncommon Attack) -- see .claude/loadmap.md "アウトレイジ"
// for current numbers. Adds a copy of itself to every OTHER player's discard pile (not the
// caster's own) -- so a lone player gets no extra copies at all, only groups do.
// Singleplayer rework (see .claude/loadmap.md "アウトレイジ"):
// - DRO on (案1): damage lowered, and always adds a copy of itself to its own discard pile (like
//   vanilla Rage/Anger), PLUS a separate copy of every OTHER copy of this card currently in hand --
//   so accumulated copies compound the way extra players would in multiplayer, instead of vanishing
//   entirely when played solo. Each hand copy is cloned from itself (not from the played card), so a
//   copy of an upgraded or Sharp-enchanted Outrage in hand produces a matching upgraded/enchanted
//   clone rather than a plain one.
// - DRO off (xDRO): matches the original amount, and always adds exactly one copy to its own
//   discard pile -- no compounding, since there's no "other players" concept to approximate.
[Pool(typeof(IroncladCardPool))]
public sealed class OutrageSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "OUTRAGE";

    protected override string OriginalVanillaCardPool => "ironclad";

    // 8m (the Rework/案1 base) since Rework is this mod's default variant; AfterCloned overwrites
    // this to the xDRO base when that branch is active instead.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(8m, ValueProp.Move) };

    public OutrageSolo() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        DynamicVars.Damage.BaseValue = DroActiveForDisplay ? 8m : 9m;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (CombatState == null)
        {
            return;
        }

        List<CardModel> sourcesToClone = new List<CardModel> { this };
        if (DroActiveForDisplay)
        {
            sourcesToClone.AddRange(CardPile.GetCards(Owner, PileType.Hand).Where(c => c is OutrageSolo && c != this));
        }
        foreach (CardModel source in sourcesToClone)
        {
            CardModel card = source.CreateCloneForPlayer(Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, Owner), 2.2f);
        }
    }

    protected override void OnUpgrade()
    {
        // Rework's upgrade delta was lowered from +4 to +2 per loadmap.md's 2026-09-13 revision
        // ("増加し続けるカードがこの強化幅はダメ" -- a compounding card growing this fast on upgrade
        // was too strong); xDRO keeps the original's +4 since it never compounds.
        DynamicVars.Damage.UpgradeValueBy(DroActiveForDisplay ? 2m : 4m);
    }
}
