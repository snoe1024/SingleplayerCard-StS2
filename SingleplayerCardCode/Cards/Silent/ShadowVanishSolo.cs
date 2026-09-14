using System.Collections.Generic;
using System.Linq;
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

// Original multiplayer card: FADE (Uncommon Skill, Retain) -- see .claude/loadmap.md "影隠し" for
// current numbers. Another player gains Dexterity this turn.
// Singleplayer rework (see .claude/loadmap.md "影隠し"):
// - DRO on (案1): no other player, so instead grants Dexterity this turn equal to the number of
//   non-Skill cards currently in hand.
// - DRO off (xDRO): a flat Dexterity gain this turn instead, matching the original amount.
// Retain kept and Exhaust removed on upgrade in both branches, matching the original.
[Pool(typeof(SilentCardPool))]
public sealed class ShadowVanishSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "FADE";

    protected override string OriginalVanillaCardPool => "silent";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

    // Only consumed by the xDRO branch -- see CoordinateSolo's CanonicalVars comment for why this is
    // still declared unconditionally.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("FlatDexterity", 6m) };

    public ShadowVanishSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            int nonSkillCards = CardPile.GetCards(Owner, PileType.Hand).Count(c => c.Type != CardType.Skill);
            if (nonSkillCards > 0)
            {
                await PowerCmd.Apply<ShadowVanishPowerSolo>(choiceContext, Owner.Creature, nonSkillCards, Owner.Creature, this);
            }
        }
        else
        {
            await PowerCmd.Apply<ShadowVanishPowerSolo>(choiceContext, Owner.Creature, DynamicVars["FlatDexterity"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
        if (!DroActiveForDisplay)
        {
            DynamicVars["FlatDexterity"].UpgradeValueBy(3m);
        }
    }
}
