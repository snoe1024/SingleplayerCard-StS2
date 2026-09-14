using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: SOULBOUND (Uncommon Power, Innate on upgrade) -- see .claude/loadmap.md
// "ソウルバウンド" for current numbers. Choose an ally; whenever you generate a Soul, add a Soul to
// that ally's deck.
// Singleplayer rework (see .claude/loadmap.md "ソウルバウンド"): reworked into SoulboundPowerSolo
// (see its own doc comment for both branches' causality). TargetType switches per branch since
// Rework needs to choose an enemy while xDRO is self-only (there's no ally either way).
[Pool(typeof(NecrobinderCardPool))]
public sealed class SoulboundSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "SOULBOUND";

    protected override string OriginalVanillaCardPool => "necrobinder";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Soul>() };

    public override TargetType TargetType => DroActiveForDisplay ? TargetType.AnyEnemy : TargetType.Self;

    public SoulboundSolo() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        if (DroActiveForDisplay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
            await PowerCmd.Apply<SoulboundPowerSolo>(choiceContext, cardPlay.Target, 1m, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<SoulboundPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
