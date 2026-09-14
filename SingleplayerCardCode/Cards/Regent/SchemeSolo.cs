using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

// Original multiplayer card: PLOT (Uncommon Skill) -- see .claude/loadmap.md "策謀" for current
// numbers. Next turn, ALL players draw extra cards (via vanilla's own DrawCardsNextTurnPower).
// Singleplayer rework (see .claude/loadmap.md "策謀", 2026-09-14 balance pass -- picked 案1 over the
// earlier 案2 "consume your current next-turn-draw stack and draw it now" design):
// - DRO on (案1): at the start of each of the owner's next 3 turns, draw 1 additional card. Uses
//   vanilla's own ClarityPower (Core/Models/Powers/ClarityPower.cs) rather than a bespoke power --
//   it already does exactly this (Amount = turns remaining, fixed +1 draw per turn, decrementing at
//   each of the owner's turn starts), so no custom power is needed. This card has no card-count of
//   its own in this branch; CanonicalVars' CardsVar is only consumed by the xDRO branch below.
//   Upgrade path reduces cost instead of scaling the turn count or draw amount, matching loadmap.md's
//   "1(0)コスト" notation (no "(N)" on the turn/draw counts themselves).
// - DRO off (xDRO): matches the original exactly -- applies vanilla's own DrawCardsNextTurnPower to
//   the owner. Cost stays fixed; card count scales on upgrade like the original.
[Pool(typeof(RegentCardPool))]
public sealed class SchemeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "PLOT";

    protected override string OriginalVanillaCardPool => "regent";

    // Only consumed by the xDRO branch -- see CoordinateSolo's CanonicalVars comment for why this is
    // still declared unconditionally.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(2) };

    public SchemeSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            await PowerCmd.Apply<ClarityPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        }
        else
        {
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, DynamicVars.Cards.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            EnergyCost.UpgradeBy(-1);
        }
        else
        {
            DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }
}
