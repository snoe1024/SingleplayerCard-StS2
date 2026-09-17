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

[Pool(typeof(RegentCardPool))]
public sealed class SchemeSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

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
