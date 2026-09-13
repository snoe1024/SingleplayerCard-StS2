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
// Singleplayer rework (see .claude/loadmap.md "策謀"):
// - DRO on (案1): "lose the extra draw you'd get at the start of next turn, and draw it now
//   instead" -- i.e. just draw the cards immediately. Upgrade path changes from adding an extra
//   card to reducing cost instead, since the card count no longer needs scaling.
// - DRO off (xDRO): matches the original exactly -- applies vanilla's own DrawCardsNextTurnPower to
//   the owner. Cost stays fixed; card count scales on upgrade like the original.
[Pool(typeof(RegentCardPool))]
public sealed class SchemeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "PLOT";

    protected override string OriginalVanillaCardPool => "regent";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(2) };

    public SchemeSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
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

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
