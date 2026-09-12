using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: LEGION_OF_BONE (Uncommon, 2 cost, Skill). ALL players Summon 6(8).
// Singleplayer rework (see .claude/loadmap.md "骨の軍団" 案1): no other players to scale with, so
// this instead Summons 5(7) per enemy currently in the fight, and gains Exhaust since it's no
// longer a party-wide effect (enemy count naturally shrinks over the fight, similar in spirit to
// Defect's "Chill").
[Pool(typeof(NecrobinderCardPool))]
public sealed class BoneLegionSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "LEGION_OF_BONE";

    protected override string OriginalVanillaCardPool => "necrobinder";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.SummonDynamic, DynamicVars.Summon) };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new SummonVar(5m) };

    public BoneLegionSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, MegaCrit.Sts2.Core.Models.Characters.Necrobinder.GetSummonAnimIfApplicable(Owner.Character), MegaCrit.Sts2.Core.Models.Characters.Necrobinder.GetSummonDelayIfApplicable(Owner.Character));
        int enemyCount = CombatState.HittableEnemies.Count;
        if (enemyCount > 0)
        {
            await OstyCmd.Summon(choiceContext, Owner, DynamicVars.Summon.BaseValue * enemyCount, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Summon.UpgradeValueBy(2m);
    }
}
