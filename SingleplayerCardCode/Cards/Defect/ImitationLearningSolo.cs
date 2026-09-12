using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Defect;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

// Original multiplayer card: IMITATION_LEARNING (Rare, 1 cost, Skill, Exhaust). Choose another
// player; the next 2(3) times they play a Power, you play a copy of it.
// Singleplayer rework (see .claude/loadmap.md "模倣学習" 案1): no other player's Powers to copy,
// so instead this remembers the owner's own last-played Power and replays a copy of it at the end
// of each of the next 2(3) turns (see ImitationLearningPowerSolo).
[Pool(typeof(DefectCardPool))]
public sealed class ImitationLearningSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "IMITATION_LEARNING";

    protected override string OriginalVanillaCardPool => "defect";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public ImitationLearningSolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<ImitationLearningPowerSolo>(choiceContext, Owner.Creature, IsUpgraded ? 3m : 2m, Owner.Creature, this);
    }
}
