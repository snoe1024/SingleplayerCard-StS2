using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: KNOCKDOWN (Rare Attack) -- see .claude/loadmap.md "ノックダウン" for
// current numbers. Deal damage; the enemy takes double/triple damage from OTHER players this turn.
// Singleplayer rework (see .claude/loadmap.md "ノックダウン"). Damage and multiplier amount are
// identical between branches -- only the TIMING differs:
// - DRO on (案1): applies KnockdownPendingPowerSolo ("next turn" marker), which converts into
//   KnockdownPowerSolo at the end of the enemy's turn -- see those two power classes' own doc
//   comments for why this is two separate power types instead of one power with an internal
//   "pending vs active" flag.
// - DRO off (xDRO): applies KnockdownPowerSolo directly -- active immediately, for the REST of the
//   current turn, matching the original.
[Pool(typeof(ColorlessCardPool))]
public sealed class KnockdownSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "KNOCKDOWN";

    protected override string OriginalVanillaCardPool => "colorless";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("KnockdownPower", 2m)
    };

    public KnockdownSolo() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<KnockdownPendingPowerSolo>(choiceContext, cardPlay.Target, DynamicVars["KnockdownPower"].BaseValue, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<KnockdownPowerSolo>(choiceContext, cardPlay.Target, DynamicVars["KnockdownPower"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
        DynamicVars["KnockdownPower"].UpgradeValueBy(1m);
    }
}
