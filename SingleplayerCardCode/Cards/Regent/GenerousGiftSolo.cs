using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Regent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

// Original multiplayer card: LARGESSE (Uncommon, 0 cost, Skill). Another player adds 1 random
// (Upgraded if this is Upgraded) Colorless card to their Hand.
// Singleplayer rework (see .claude/loadmap.md "寛大なる施し" 案1): no other player, so instead of
// giving it immediately, it's granted at the start of the owner's NEXT turn (see
// GenerousGiftPowerSolo) -- fits Regent's pattern of deferring energy/draw-style effects to next
// turn.
[Pool(typeof(RegentCardPool))]
public sealed class GenerousGiftSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public GenerousGiftSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        var power = await PowerCmd.Apply<GenerousGiftPowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        if (power != null)
        {
            power.GenerateUpgraded = IsUpgraded;
        }
    }
}
