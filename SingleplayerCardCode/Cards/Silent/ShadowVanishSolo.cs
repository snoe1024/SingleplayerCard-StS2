using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;
using SingleplayerCard.SingleplayerCardCode.Powers.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

// Original multiplayer card: FADE (Uncommon, 0 cost, Skill, Retain). Another player gains 6(9)
// Dexterity this turn.
// Singleplayer rework (see .claude/loadmap.md "影隠し" 案1): no other player, so instead grants
// Dexterity this turn equal to the number of non-Skill cards currently in hand (Retain kept,
// Exhaust removed on upgrade).
[Pool(typeof(SilentCardPool))]
public sealed class ShadowVanishSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "FADE";

    protected override string OriginalVanillaCardPool => "silent";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

    public ShadowVanishSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int nonSkillCards = CardPile.GetCards(Owner, PileType.Hand).Count(c => c.Type != CardType.Skill);
        if (nonSkillCards > 0)
        {
            await PowerCmd.Apply<ShadowVanishPowerSolo>(choiceContext, Owner.Creature, nonSkillCards, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
