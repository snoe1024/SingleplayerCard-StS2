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

[Pool(typeof(NecrobinderCardPool))]
public sealed class BoneLegionSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "LEGION_OF_BONE";

    protected override string OriginalVanillaCardPool => "necrobinder";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.SummonDynamic, DynamicVars.Summon) };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new SummonVar(5m) };

    public BoneLegionSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

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
