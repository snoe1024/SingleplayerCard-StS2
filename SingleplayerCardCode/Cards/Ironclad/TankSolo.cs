using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Ironclad;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Ironclad;

// Original multiplayer card: TANK (Rare, 1(0) cost, Power). Self takes 50% more damage from
// enemies; allies take 50% less damage from enemies. Doesn't stack with itself.
// Singleplayer rework (see .claude/loadmap.md "タンク" 案1): no allies to protect, so instead of
// reducing ally damage, blocked attack damage is reflected back at the attacker. See
// TankPowerSolo for the actual effect.
[Pool(typeof(IroncladCardPool))]
public sealed class TankSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public TankSolo() : base(1, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<TankPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
