using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public sealed class BeaconOfHopeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "BEACON_OF_HOPE";

    protected override string OriginalVanillaCardPool => "colorless";

    public BeaconOfHopeSolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<BeaconOfHopePowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
