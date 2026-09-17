using System;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

[Pool(typeof(SilentCardPool))]
public sealed class PincerAttackSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "FLANKING";

    protected override string OriginalVanillaCardPool => "silent";

    public PincerAttackSolo() : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var power = await PowerCmd.Apply<PincerAttackPowerSolo>(choiceContext, cardPlay.Target, 2m, Owner.Creature, this);
        if (DroActiveForDisplay)
        {
            power?.SetExcludedCards(CardPile.GetCards(Owner, PileType.Hand));
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
