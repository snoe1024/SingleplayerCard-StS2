using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Powers.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

[Pool(typeof(SilentCardPool))]
public sealed class ShadowVanishSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "FADE";

    protected override string OriginalVanillaCardPool => "silent";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<DexterityPower>()];

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DynamicVar("FlatDexterity", 6m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("DexterityRemake").WithMultiplier((card, _) => CardPile.GetCards(card.Owner, PileType.Hand).Count(c => c.Type != CardType.Skill))
    ];

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
