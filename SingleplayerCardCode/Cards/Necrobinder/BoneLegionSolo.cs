using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: LEGION_OF_BONE (Uncommon Skill) -- see .claude/loadmap.md "骨の軍団"
// for current numbers. ALL players Summon.
// Singleplayer rework (see .claude/loadmap.md "骨の軍団"):
// - DRO on (案1): no other players to scale with, so this instead Summons per enemy currently in
//   the fight (enemy count naturally shrinks over the fight, similar in spirit to Defect's "Chill").
// - DRO off (xDRO): a flat Summon amount instead, matching the original.
// Exhaust kept in both branches, matching the original.
[Pool(typeof(NecrobinderCardPool))]
public sealed class BoneLegionSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "LEGION_OF_BONE";

    protected override string OriginalVanillaCardPool => "necrobinder";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.SummonDynamic, DynamicVars.Summon) };

    // 5m (the Rework/案1 base) since Rework is this mod's default variant; AfterCloned overwrites
    // this to the xDRO flat amount when that branch is active instead. Both branches share the same
    // +2 upgrade delta (5->7, 6->8), so OnUpgrade doesn't need to branch.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new SummonVar(5m) };

    public BoneLegionSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // Pure function of (DroActiveForDisplay, IsUpgraded) -- see base class doc comment. Both
    // branches share the same +2 upgrade delta (5->7, 6->8), matching OnUpgrade below.
    protected override void RefreshDroBranchState()
    {
        decimal baseline = DroActiveForDisplay ? 5m : 6m;
        DynamicVars.Summon.BaseValue = IsUpgraded ? baseline + 2m : baseline;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, MegaCrit.Sts2.Core.Models.Characters.Necrobinder.GetSummonAnimIfApplicable(Owner.Character), MegaCrit.Sts2.Core.Models.Characters.Necrobinder.GetSummonDelayIfApplicable(Owner.Character));
        if (DroActiveForDisplay)
        {
            int enemyCount = CombatState.HittableEnemies.Count;
            if (enemyCount > 0)
            {
                await OstyCmd.Summon(choiceContext, Owner, DynamicVars.Summon.BaseValue * enemyCount, this);
            }
        }
        else
        {
            await OstyCmd.Summon(choiceContext, Owner, DynamicVars.Summon.BaseValue, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Summon.UpgradeValueBy(2m);
    }
}
