using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

// Original multiplayer card: SNEAKY (Rare Power, Sly) -- see .claude/loadmap.md "隠密" for current
// numbers. Gain Block whenever another player attacks an enemy.
// Singleplayer rework (see .claude/loadmap.md "隠密"). See StealthPowerSolo for the two branches'
// actual trigger conditions -- the power itself decides which one fires, based on the DRO state it
// captured from this card when applied.
[Pool(typeof(SilentCardPool))]
public sealed class StealthSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "SNEAKY";

    protected override string OriginalVanillaCardPool => "silent";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Sly };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StealthPowerSolo>("StealthRemake", 2m),
        new PowerVar<StealthPowerSolo>("StealthXdro", 1m)
    ];

    public StealthSolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<StealthPowerSolo>(choiceContext, Owner.Creature, DynamicVars["StealthRemake"].BaseValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<StealthPowerSolo>(choiceContext, Owner.Creature, DynamicVars["StealthXdro"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars["StealthRemake"].UpgradeValueBy(1m);
        }
        else
        {
            DynamicVars["StealthXdro"].UpgradeValueBy(1m);
        }
    }
}
