using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: GLIMPSE_BEYOND (Rare, 1 cost, Skill, Exhaust). ALL players add 3(4)
// Souls to their Draw Pile immediately.
// Singleplayer rework (see .claude/loadmap.md "彼方への一瞥" 案1): dumping 3-4 Souls into the draw
// pile at once is nearly identical to vanilla's own Soul Extraction; instead this spreads delivery
// out, adding 1 Soul directly to hand at the start of each of the next 2(3) turns (see
// GlimpseBeyondPowerSolo), guaranteeing draws instead of risking deck-clog all at once.
[Pool(typeof(NecrobinderCardPool))]
public sealed class GlimpseBeyondSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "GLIMPSE_BEYOND";

    protected override string OriginalVanillaCardPool => "necrobinder";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Soul>() };

    public GlimpseBeyondSolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await PowerCmd.Apply<GlimpseBeyondPowerSolo>(choiceContext, Owner.Creature, IsUpgraded ? 3m : 2m, Owner.Creature, this);
    }
}
