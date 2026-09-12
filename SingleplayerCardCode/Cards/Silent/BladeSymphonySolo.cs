using System;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

// Original multiplayer card: BLADE_SYMPHONY (Uncommon, (2)1 cost, Skill). Add 2 Shivs to ALL
// players' hands.
// Singleplayer rework (see .claude/loadmap.md "ブレイド・シンフォニー" 案1): no other players to
// hand Shivs to, so instead this replays every Shiv-tagged card currently sitting in the Discard or
// Exhaust pile against a chosen enemy (same AutoPlay pattern as vanilla KNIFE_TRAP, which only reads
// the Exhaust pile -- this also covers ones that overflowed to Discard). Approximates "Shivs
// generated this turn" as "Shivs currently off the hand/draw/play piles", since there's no cheap way
// to timestamp when a specific card instance was generated.
[Pool(typeof(SilentCardPool))]
public sealed class BladeSymphonySolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    public BladeSymphonySolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        var shivs = PileType.Discard.GetPile(Owner).Cards
            .Concat(PileType.Exhaust.GetPile(Owner).Cards)
            .Where(c => c.Tags.Contains(CardTag.Shiv))
            .ToList();

        bool first = true;
        foreach (CardModel shiv in shivs)
        {
            if (IsUpgraded)
            {
                CardCmd.Upgrade(shiv, CardPreviewStyle.None);
            }

            await CardCmd.AutoPlay(choiceContext, shiv, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !first);
            first = false;
        }
    }
}
