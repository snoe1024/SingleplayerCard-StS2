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

[Pool(typeof(ColorlessCardPool))]
public sealed class HelpingHandSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "LIFT";

    protected override string OriginalVanillaCardPool => "colorless";

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
