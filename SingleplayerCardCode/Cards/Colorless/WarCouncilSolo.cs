using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: HUDDLE_UP (Uncommon, 1 cost, Skill, Exhaust). ALL players draw 2(3)
// cards immediately.
// Singleplayer rework (see .claude/loadmap.md "作戦会議" 案1): draws 2 now AND grants another 2 at
// the start of the owner's next turn (WarCouncilPowerSolo) -- Exhaust dropped in this rework.
[Pool(typeof(ColorlessCardPool))]
public sealed class WarCouncilSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "HUDDLE_UP";

    protected override string OriginalVanillaCardPool => "colorless";

    public WarCouncilSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 2, Owner);
        await PowerCmd.Apply<WarCouncilPowerSolo>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
    }
}
