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

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

// Original multiplayer card: HUDDLE_UP (Uncommon Skill, Exhaust) -- see .claude/loadmap.md
// "作戦会議" for current numbers. ALL players draw cards immediately.
// Singleplayer rework (see .claude/loadmap.md "作戦会議"):
// - DRO on (案1): draws cards now AND grants more at the start of the owner's next turn, via
//   vanilla's own DrawCardsNextTurnPower rather than a bespoke power -- a separate custom power
//   would just duplicate an effect vanilla already implements, and using the shared vanilla power
//   means it correctly stacks with any other source of "next turn" draw instead of tracking its own
//   separate total. Both draw counts are a flat 2 with no upgrade scaling and Exhaust dropped,
//   exactly matching loadmap.md's own 案1 text (no "(N)" upgrade notation there).
// - DRO off (xDRO): matches the original -- draws immediately, scales with upgrade, keeps Exhaust.
[Pool(typeof(ColorlessCardPool))]
public sealed class WarCouncilSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "HUDDLE_UP";

    protected override string OriginalVanillaCardPool => "colorless";

    // Only consumed by the xDRO branch -- see CoordinateSolo's CanonicalVars comment for why this is
    // still declared unconditionally.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(2) };

    public WarCouncilSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        if (!DroActiveForDisplay)
        {
            AddKeyword(CardKeyword.Exhaust);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            await CardPileCmd.Draw(choiceContext, 2, Owner);
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
        }
        else
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        if (!DroActiveForDisplay)
        {
            DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
