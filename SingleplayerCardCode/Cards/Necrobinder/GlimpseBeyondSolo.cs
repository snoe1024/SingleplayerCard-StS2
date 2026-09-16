using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using System.Linq;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Necrobinder;

// Original multiplayer card: GLIMPSE_BEYOND (Rare Skill, Exhaust) -- see .claude/loadmap.md
// "彼方への一瞥" for current numbers. ALL players add Souls to their Draw Pile immediately.
// Singleplayer rework (see .claude/loadmap.md "彼方への一瞥"):
// - DRO on (案1): dumping that many Souls into the draw pile at once is nearly identical to
//   vanilla's own Soul Extraction; instead this spreads delivery out, adding a Soul directly to
//   hand at the start of each of the next several turns (see GlimpseBeyondPowerSolo), guaranteeing
//   draws instead of risking deck-clog all at once.
// - DRO off (xDRO): matches the original exactly -- Soul.Create + AddGeneratedCardsToCombat into
//   the Draw Pile at random positions, same helper vanilla's own OnPlay uses.
[Pool(typeof(NecrobinderCardPool))]
public sealed class GlimpseBeyondSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "GLIMPSE_BEYOND";

    protected override string OriginalVanillaCardPool => "necrobinder";

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromCard<Soul>() };

    // Only consumed by the xDRO branch -- see CoordinateSolo's CanonicalVars comment for why this is
    // still declared unconditionally.
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new CardsVar("SoulsRemake", 2),
        new CardsVar("SoulsXdro", 3)
    ];

    public GlimpseBeyondSolo() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        if (DroActiveForDisplay)
        {
            var soul = Soul.Create(Owner, 1, CombatState);
            await CardPileCmd.AddGeneratedCardsToCombat(soul, PileType.Hand, Owner, CardPilePosition.Top);
            await PowerCmd.Apply<GlimpseBeyondPowerSolo>(choiceContext, Owner.Creature, DynamicVars["SoulsRemake"].IntValue, Owner.Creature, this);
        }
        else
        {
            var soul = Soul.Create(Owner, DynamicVars["SoulsXdro"].IntValue, CombatState);
            var results = await CardPileCmd.AddGeneratedCardsToCombat(soul, PileType.Draw, Owner, CardPilePosition.Random);
            CardCmd.PreviewCardPileAdd(results);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            DynamicVars["SoulsRemake"].UpgradeValueBy(1m);
        }
        else
        {
            DynamicVars["SoulsXdro"].UpgradeValueBy(1m);
        }
    }
}
