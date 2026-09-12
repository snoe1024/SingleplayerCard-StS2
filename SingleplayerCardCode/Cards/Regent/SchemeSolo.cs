using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Regent;

// Original multiplayer card: PLOT (Uncommon, 1 cost, Skill). Next turn, ALL players draw 2(3) extra
// cards.
// Singleplayer rework (see .claude/loadmap.md "策謀" 案1): "lose the extra draw you'd get at the
// start of next turn, and draw it now instead" -- i.e. just draw the cards immediately. Upgrade
// path changes from "+1 card" to "-1 cost" since the card count no longer needs scaling.
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
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
