using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Powers.Defect;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

// Original multiplayer card: ENERGY_SURGE (Uncommon Skill, Exhaust) -- see .claude/loadmap.md
// "エナジーサージ" for current numbers. ALL players gain Energy immediately.
// Singleplayer rework (see .claude/loadmap.md "エナジーサージ"):
// - DRO on (案1): a lump sum of Energy in one turn is too much for a single Defect, so this instead
//   grants Energy at the start of each of the next several turns, via vanilla's own RadiancePower
//   rather than a bespoke power (RadiancePower already does exactly this). Cost lowered to match.
// - DRO off (xDRO): matches the original -- grants Energy immediately, at the original's cost.
// Exhaust kept in both branches, matching the original.
[Pool(typeof(DefectCardPool))]
public sealed class EnergySurgeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "ENERGY_SURGE";

    protected override string OriginalVanillaCardPool => "defect";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { EnergyHoverTip };

    // Only consumed by the xDRO branch -- see CoordinateSolo's CanonicalVars comment for why this is
    // still declared unconditionally.
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new EnergyVar(2),
        new PowerVar<RadiancePower>(2)
    ];

    public EnergySurgeSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // Pure function of DroActiveForDisplay -- see base class doc comment. Cost never changes with
    // upgrade in either branch, so no IsUpgraded term is needed here.
    protected override void RefreshDroBranchState()
    {
        EnergyCost.SetCustomBaseCost(DroActiveForDisplay ? 0 : 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<RadiancePower>(choiceContext, Owner.Creature, DynamicVars.Power<RadiancePower>().IntValue, Owner.Creature, this);
        }
        else
        {
            await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars.Power<RadiancePower>().UpgradeValueBy(1);
        }
        else
        {
            DynamicVars.Energy.UpgradeValueBy(1m);
        }
    }
}
