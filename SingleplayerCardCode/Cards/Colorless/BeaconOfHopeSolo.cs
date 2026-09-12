using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: BEACON_OF_HOPE (Rare, 1(2) cost -- 1 on the normal branch, 2 on
// public beta per loadmap.md -- Power). Whenever you gain Block on your turn, other players gain
// half that much Block.
// Singleplayer rework (see .claude/loadmap.md "希望の道標" 案1): no other players, so instead
// banks half of the Block gained during your turn and grants it as Block at the start of your next
// turn (see BeaconOfHopePowerSolo). Uses the beta's 2-cost baseline since that's what this machine's
// decompiled source reflects.
[Pool(typeof(ColorlessCardPool))]
public sealed class BeaconOfHopeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "BEACON_OF_HOPE";

    protected override string OriginalVanillaCardPool => "colorless";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate };

    public BeaconOfHopeSolo() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<BeaconOfHopePowerSolo>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
}
