using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

// Original multiplayer card: OUTRAGE (Uncommon, 0 cost, Attack, 9(13) damage). Adds a copy of
// itself to every OTHER player's discard pile (not the caster's own) -- so a lone player gets no
// extra copies at all, only groups do.
// Singleplayer rework (see .claude/loadmap.md "アウトレイジ" 案1): lowered to 8(12) damage, and
// always adds a copy to its own discard pile (like vanilla Rage/Anger), PLUS one more copy for
// every OTHER copy of this card currently in hand -- so accumulated copies compound the way extra
// players would in multiplayer, instead of vanishing entirely when played solo.
[Pool(typeof(IroncladCardPool))]
public sealed class OutrageSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(8m, ValueProp.Move) };

    public OutrageSolo() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
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

        int otherCopiesInHand = CardPile.GetCards(Owner, PileType.Hand).Count(c => c is OutrageSolo && c != this);
        int copiesToAdd = 1 + otherCopiesInHand;
        for (int i = 0; i < copiesToAdd; i++)
        {
            CardModel card = CreateCloneForPlayer(Owner);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, Owner), 2.2f);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
