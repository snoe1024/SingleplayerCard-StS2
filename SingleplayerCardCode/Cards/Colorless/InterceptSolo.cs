using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public sealed class InterceptSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "INTERCEPT";

    protected override string OriginalVanillaCardPool => "colorless";

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new BlockVar(15m, ValueProp.Move) };

    public InterceptSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override void RefreshDroBranchState()
    {
        DynamicVars.Block.BaseValue = DroActiveForDisplay
            ? (IsUpgraded ? 21m : 15m)
            : (IsUpgraded ? 13m : 9m);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(DroActiveForDisplay ? 6m : 4m);
    }
}
