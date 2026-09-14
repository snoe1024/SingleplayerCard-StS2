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
using MegaCrit.Sts2.Core.Models.Orbs;
using SingleplayerCard.SingleplayerCardCode.Powers.Defect;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

// Original multiplayer card: HIBERNATE (Uncommon Skill) -- see .claude/loadmap.md "冬眠" for
// current numbers. This turn, your Frost grants ALL allies Block. Channel Frost.
// Singleplayer rework (see .claude/loadmap.md "冬眠"):
// - DRO on (案1): no allies to share Frost's Block with, so instead Frost's Passive triggers twice
//   this turn (see HibernatePowerSolo) while still Channeling Frost.
// - DRO off (xDRO): just Channels Frost, matching the original aside from not needing allies.
// Same cost and Frost count in both branches.
[Pool(typeof(DefectCardPool))]
public sealed class HibernateSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "HIBERNATE";

    protected override string OriginalVanillaCardPool => "defect";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[]
    {
        HoverTipFactory.Static(StaticHoverTip.Channeling),
        HoverTipFactory.FromOrb<FrostOrb>(),
        HoverTipFactory.Static(StaticHoverTip.Block)
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new RepeatVar(2) };

    public HibernateSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<HibernatePowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, cardPlay.Card);
        }

        for (int i = 0; i < DynamicVars.Repeat.IntValue; i++)
        {
            await OrbCmd.Channel<FrostOrb>(choiceContext, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Repeat.UpgradeValueBy(1m);
    }
}
