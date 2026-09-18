using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

[Pool(typeof(NecrobinderCardPool))]
public sealed class CacophonySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "CACOPHONY";

    protected override string OriginalVanillaCardPool => "necrobinder";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar("CardsRework", 12),
        new DamageVar("DamageRework", 20m, ValueProp.Unpowered),
        new CardsVar("CardsXdro", 33),
        new DamageVar("DamageXdro", 66m, ValueProp.Unpowered)
    };

    public CacophonySolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            var power = await PowerCmd.Apply<CacophonyPowerSolo>(choiceContext, Owner.Creature, DynamicVars["DamageRework"].IntValue, Owner.Creature, this);
            if (power != null)
            {
                power.Threshold = DynamicVars["CardsRework"].IntValue;
                power.DynamicVars.Cards.BaseValue = DynamicVars["CardsRework"].BaseValue;
            }
        }
        else
        {
            var power = await PowerCmd.Apply<CacophonyPowerSolo>(choiceContext, Owner.Creature, DynamicVars["DamageXdro"].IntValue, Owner.Creature, this);
            if (power != null)
            {
                power.Threshold = DynamicVars["CardsXdro"].IntValue;
                power.DynamicVars.Cards.BaseValue = DynamicVars["CardsXdro"].BaseValue;
            }
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars["DamageRework"].UpgradeValueBy(8m);
        }
        else
        {
            DynamicVars["DamageXdro"].UpgradeValueBy(33m);
        }
    }
}
