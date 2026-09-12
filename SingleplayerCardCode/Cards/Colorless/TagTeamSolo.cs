using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: TAG_TEAM (Uncommon, 2 cost, Attack). Deal 11(15) damage; the next
// Attack ANOTHER player plays on the enemy is played an extra time.
// Singleplayer rework (see .claude/loadmap.md "タッグチーム" 案1, identical to its own xDRO): just
// targets your own next Attack instead of another player's (see TagTeamPowerSolo).
[Pool(typeof(ColorlessCardPool))]
public sealed class TagTeamSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DamageVar(11m, ValueProp.Move) };

    public TagTeamSolo() : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<TagTeamPowerSolo>(choiceContext, cardPlay.Target, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
