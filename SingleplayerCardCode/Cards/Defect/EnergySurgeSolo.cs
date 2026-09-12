using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Defect;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Defect;

// Original multiplayer card: ENERGY_SURGE (Uncommon, 1 cost, Skill, Exhaust). ALL players gain
// 2(3) Energy immediately.
// Singleplayer rework (see .claude/loadmap.md "エナジーサージ" 案1): a lump 2-3 Energy in one turn
// is too much for a single Defect, so this instead grants 1 Energy at the start of each of the next
// 2(3) turns (EnergySurgePowerSolo). Cost lowered to 0 to match.
[Pool(typeof(DefectCardPool))]
public sealed class EnergySurgeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { EnergyHoverTip };

    public EnergySurgeSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<EnergySurgePowerSolo>(choiceContext, Owner.Creature, IsUpgraded ? 3m : 2m, Owner.Creature, this);
    }
}
