using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Enchantments;
using SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

[Pool(typeof(ColorlessCardPool))]
public sealed class CoordinateSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "COORDINATE";

    protected override string OriginalVanillaCardPool => "colorless";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<TrainEnchantmentSolo>();

    private const string EnchantmentVarKey = "TrainEnchantmentAmount";

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(EnchantmentVarKey, 5m),
        new PowerVar<StrengthPower>(5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    public CoordinateSolo() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            if (CardPile.GetCards(Owner, PileType.Hand).Any(c => c.Enchantment is null))
            {
                var cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), context: choiceContext, player: base.Owner, filter: c => c.Enchantment is null, source: this)).FirstOrDefault();
                if (cardModel != null)
                {
                    CardCmd.Enchant<TrainEnchantmentSolo>(cardModel, DynamicVars[EnchantmentVarKey].BaseValue);
                }
            }
        }
        else
        {
            await PowerCmd.Apply<CoordinatePowerSolo>(choiceContext, Owner.Creature, DynamicVars.Strength.BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroWasOnAtCreation)
        {
            DynamicVars[EnchantmentVarKey].UpgradeValueBy(3m);
        }
        else
        {
            DynamicVars.Strength.UpgradeValueBy(3m);
        }
    }
}
