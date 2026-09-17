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

[Pool(typeof(ColorlessCardPool))]
public sealed class WarCouncilSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "HUDDLE_UP";

    protected override string OriginalVanillaCardPool => "colorless";

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(2) };

    public WarCouncilSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override void RefreshDroBranchState()
    {
        if (DroActiveForDisplay)
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
        else
        {
            AddKeyword(CardKeyword.Exhaust);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
            await PowerCmd.Apply<DrawCardsNextTurnPower>(choiceContext, Owner.Creature, 2m, Owner.Creature, this);
        }
        else
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
