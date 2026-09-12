using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

// Original multiplayer card: SNEAKY (Rare, 2 cost, Power, Sly). Gain Block whenever another player
// attacks an enemy.
// Singleplayer rework (see .claude/loadmap.md "隠密" 案1): no other player, so instead grants
// Block whenever the owner deals damage by means other than an Attack card. See StealthPowerSolo.
[Pool(typeof(SilentCardPool))]
public sealed class StealthSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Sly };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new PowerVar<StealthPowerSolo>(1m) };

    public StealthSolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<StealthPowerSolo>(choiceContext, Owner.Creature, DynamicVars["StealthPowerSolo"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StealthPowerSolo"].UpgradeValueBy(1m);
    }
}
