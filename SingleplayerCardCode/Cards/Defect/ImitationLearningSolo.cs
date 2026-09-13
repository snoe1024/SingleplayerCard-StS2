using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Powers.Defect;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

// Original multiplayer card: IMITATION_LEARNING (Rare Skill, Exhaust) -- see .claude/loadmap.md
// "模倣学習" for current numbers. Choose another player; the next several times they play a Power,
// you play a copy of it.
//
// NOTE (flagged for the mod author, not silently changed): loadmap.md's own closing paragraph
// argues for 案1 (enemy-buff-triggered 0-cost power draw) as the actual intended design, but this
// class instead implements 案2 (replay your own last-played Power at the end of each of the next
// several turns) via ImitationLearningPowerSolo. That mismatch predates this xDRO addition and
// wasn't changed here -- only the missing xDRO branch below is new.
//
// Singleplayer rework (see .claude/loadmap.md "模倣学習"):
// - DRO on (currently implements 案2, see NOTE above): no other player's Powers to copy, so instead
//   this remembers the owner's own last-played Power and replays a copy of it at the end of each of
//   the next several turns (see ImitationLearningPowerSolo).
// - DRO off (xDRO): matches the original -- applies vanilla's own ImitationLearningPower
//   (Core/Models/Powers/ImitationLearningPower.cs) targeted at the OWNER instead of another player,
//   so playing your own Powers triggers it. That power's logic only checks `cardPlay.Card.Owner ==
//   PlayerTarget`, with no other multiplayer-specific behavior, so it works correctly self-targeted.
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
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<ImitationLearningPowerSolo>(choiceContext, Owner.Creature, IsUpgraded ? 3m : 2m, Owner.Creature, this);
        }
        else
        {
            var power = await PowerCmd.Apply<ImitationLearningPower>(choiceContext, Owner.Creature, IsUpgraded ? 3m : 2m, Owner.Creature, this);
            if (power != null)
            {
                power.PlayerTarget = Owner;
            }
        }
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
