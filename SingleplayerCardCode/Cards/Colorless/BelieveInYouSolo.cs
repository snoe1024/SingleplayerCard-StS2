using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public sealed class BelieveInYouSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;
    
    protected override string OriginalVanillaCardId => "BELIEVE_IN_YOU";

    protected override string OriginalVanillaCardPool => "colorless";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new EnergyVar(2) };

    public BelieveInYouSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<NoEnergyGainPowerSolo>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
            await PowerCmd.Apply<NoDrawPowerSolo>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
    }
}
