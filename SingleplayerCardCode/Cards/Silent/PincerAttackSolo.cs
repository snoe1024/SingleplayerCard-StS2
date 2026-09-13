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

// Original multiplayer card: FLANKING (Rare Skill) -- see .claude/loadmap.md "挟撃" for current
// numbers. This turn, the target enemy takes double Attack damage from other players (not the
// caster).
// Singleplayer rework (see .claude/loadmap.md "挟撃"):
// - DRO on (案1): no other players, so instead the target takes double Attack damage this turn from
//   any Attack card that was NOT already in hand when this was played (i.e. cards drawn or generated
//   afterward). See PincerAttackPowerSolo.
// - DRO off (xDRO): plain double Attack damage this turn from anything, no exclusion.
[Pool(typeof(SilentCardPool))]
public sealed class PincerAttackSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

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

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
