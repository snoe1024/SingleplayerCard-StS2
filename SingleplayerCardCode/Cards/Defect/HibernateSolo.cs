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
