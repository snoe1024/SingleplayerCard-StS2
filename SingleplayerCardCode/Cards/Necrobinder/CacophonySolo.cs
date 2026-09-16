using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: CACOPHONY (Rare Power) -- see .claude/loadmap.md "不協和音" for
// current numbers. Every so many cards drawn by ALL players, deal damage to a random enemy.
// Singleplayer rework (see .claude/loadmap.md "不協和音"). See CacophonyPowerSolo for the two
// branches' actual threshold/damage numbers -- the power itself decides which to use, based on
// values this card sets right after applying it.
[Pool(typeof(NecrobinderCardPool))]
public sealed class CacophonySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "CACOPHONY";

    protected override string OriginalVanillaCardPool => "necrobinder";

    // Rework/案1 defaults (12 draws / 16 damage) since Rework is this mod's default variant;
    // AfterCloned overwrites both to the xDRO values (33 / 66) when that branch is active instead.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(12),
        new DamageVar(16m, ValueProp.Unpowered)
    };

    public CacophonySolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    // Pure function of (DroActiveForDisplay, IsUpgraded) -- see base class doc comment. Cards never
    // upgrades in either branch (only Damage does, matching OnUpgrade below), so it's a fixed value.
    protected override void RefreshDroBranchState()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars.Cards.BaseValue = 12m;
            DynamicVars.Damage.BaseValue = IsUpgraded ? 24m : 16m;
        }
        else
        {
            DynamicVars.Cards.BaseValue = 33m;
            DynamicVars.Damage.BaseValue = IsUpgraded ? 99m : 66m;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<CacophonyPowerSolo>(choiceContext, Owner.Creature, DynamicVars.Damage.IntValue, Owner.Creature, this);
        if (power != null)
        {
            power.Threshold = DynamicVars.Cards.IntValue;
            power.DynamicVars.Cards.BaseValue = DynamicVars.Cards.BaseValue;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DroActiveForDisplay ? 8m : 33m);
    }
}
