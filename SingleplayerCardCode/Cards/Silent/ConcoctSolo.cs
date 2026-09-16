using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
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

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

[Pool(typeof(SilentCardPool))]
public sealed class ConcoctSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override bool DroVersionExists => true;

    protected override string OriginalVanillaCardId => "CONCOCT";

    protected override string OriginalVanillaCardPool => "silent";

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => HoverTipFactory.FromEnchantment<VenomousEnchantmentSolo>();

    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new DynamicVar("Envenom", 3m),
        new DynamicVar("Venomous", 2m),
    ];

    public ConcoctSolo() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    // Pure function of (DroActiveForDisplay, IsUpgraded) -- see base class doc comment. Rework keeps
    // Exhaust only until upgraded (matches OnUpgrade below, "Upgrade to remove Exhaust"); xDRO never
    // has it. Written as an explicit Add-or-Remove rather than a one-way conditional so a later,
    // corrected call can undo an earlier wrong guess in either direction.
    protected override void RefreshDroBranchState()
    {
        if (DroActiveForDisplay && !IsUpgraded)
        {
            AddKeyword(CardKeyword.Exhaust);
        }
        else
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (DroActiveForDisplay)
        {
            var targets = CardPile.GetCards(Owner, PileType.Hand).Where(c => c.Type == CardType.Attack && c.Enchantment == null).ToList();
            foreach (CardModel card in targets)
            {
                CardCmd.Enchant<VenomousEnchantmentSolo>(card, DynamicVars["Venomous"].BaseValue);
            }
        }
        else
        {
            await PowerCmd.Apply<ConcoctPower>(choiceContext, Owner.Creature, DynamicVars["Envenom"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        if (DroActiveForDisplay)
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
        else
        {
            DynamicVars["Envenom"].UpgradeValueBy(1m);
        }
    }
}
