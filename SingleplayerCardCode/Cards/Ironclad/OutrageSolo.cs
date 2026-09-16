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

    // "Damage" is kept as a plain DamageVar purely as the gameplay-effective/cross-card-visible key
    // (OnPlay's DamageCmd.Attack reads it, and vanilla's Thrash picks a random Attack card from hand
    // and reads ITS "Damage" key directly -- see Thrash.OnPlay); RefreshDroBranchState below syncs it
    // to whichever of DamageRemake/DamageXdro is active. Both branches get their own registered var
    // (rather than only Remake being "a real DamageVar") so .descriptionRework/.descriptionXdro can
    // each reference their own number directly -- correct on canonical/Card Library instances too,
    // since CanonicalVars entries need no mutation to read, unlike Damage itself.
    // DamageRemake/DamageXdro MUST themselves be DamageVar (not a plain DynamicVar) -- see MidnightSolo's
    // matching comment: CardModel's UpdateDynamicVarPreview loop calls UpdateCardPreview on every var
    // regardless of key name, and only DamageVar's override actually recomputes PreviewValue from
    // current Strength/Vulnerable/Weak/enchantments; a plain DynamicVar leaves :diff() always showing
    // the raw, un-buffed number (confirmed as a real regression in actual play).
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(8m, ValueProp.Move),
        new DamageVar("DamageRemake", 8m, ValueProp.Move),
        new DamageVar("DamageXdro", 9m, ValueProp.Move)
    };

    public OutrageSolo() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
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
        if (DroActiveForDisplay)
        {
            DynamicVars["DamageRemake"].UpgradeValueBy(3m);
        }
        else
        {
            DynamicVars["DamageXdro"].UpgradeValueBy(4m);
        }
        RefreshDroBranchState();
    }
}
