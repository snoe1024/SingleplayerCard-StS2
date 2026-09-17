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

    protected override string OriginalVanillaCardId => "CACOPHONY";

    protected override string OriginalVanillaCardPool => "necrobinder";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(12),
        new DamageVar(16m, ValueProp.Unpowered)
    };

    public CacophonySolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override void RefreshDroBranchState()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars.Cards.BaseValue = 12m;
            DynamicVars.Damage.BaseValue = IsUpgraded ? 24m : 16m;
        }
        else
        {
            DynamicVars.Cards.BaseValue = 33m;
            DynamicVars.Damage.BaseValue = IsUpgraded ? 99m : 66m;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var power = await PowerCmd.Apply<CacophonyPowerSolo>(choiceContext, Owner.Creature, DynamicVars.Damage.IntValue, Owner.Creature, this);
        if (power != null)
        {
            power.Threshold = DynamicVars.Cards.IntValue;
            power.DynamicVars.Cards.BaseValue = DynamicVars.Cards.BaseValue;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(DroActiveForDisplay ? 8m : 33m);
    }
}
