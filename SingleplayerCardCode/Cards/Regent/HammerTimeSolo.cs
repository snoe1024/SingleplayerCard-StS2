using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using SingleplayerCard.SingleplayerCardCode.Powers.Regent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

// Original multiplayer card: HAMMER_TIME (Rare Power) -- see .claude/loadmap.md "ハンマータイム"
// for current numbers. Whenever you Forge, all allies Forge as well -- does nothing alone.
// Singleplayer rework (see .claude/loadmap.md "ハンマータイム" 案1): instead, whenever Sovereign
// Blade is played, replay every card that Forged this turn. See HammerTimePowerSolo.
// 2026-09-14 balance pass: fixed cost of 1 from the start (was 2, reduced to 1 on upgrade); upgrade
// now grants Innate instead of reducing cost, matching loadmap.md's "1コスト" / "(UG天賦)" notation.
[Pool(typeof(RegentCardPool))]
public sealed class HammerTimeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "HAMMER_TIME";

    protected override string OriginalVanillaCardPool => "regent";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ HoverTipFactory.FromCard<SovereignBlade>() ];

    public HammerTimeSolo() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<HammerTimePowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Innate);
    }
}
