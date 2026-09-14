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

// Original multiplayer card: LIFT (Uncommon Skill) -- see .claude/loadmap.md "手助け" for current
// numbers. Give another player Block.
// Singleplayer rework (see .claude/loadmap.md "手助け"):
// - DRO on (案1): no other player, so instead doubles the Block gained from your very next card
//   (HelpingHandPowerSolo). Cost lowered on upgrade.
// - DRO off (xDRO): matches the original -- a flat immediate Block gain. Cost stays fixed.
[Pool(typeof(ColorlessCardPool))]
public sealed class HelpingHandSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "LIFT";

    protected override string OriginalVanillaCardPool => "colorless";

    // Only consumed by the xDRO branch -- see CoordinateSolo's CanonicalVars comment for why this is
    // still declared unconditionally.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new BlockVar(11m, ValueProp.Move) };

    public HelpingHandSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<HelpingHandPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
        else
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            EnergyCost.UpgradeBy(-1);
        }
        else
        {
            DynamicVars.Block.UpgradeValueBy(5m);
        }
    }
}
