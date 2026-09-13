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

// Original multiplayer card: CONCOCT (Uncommon Skill) -- see .claude/loadmap.md "調合" for current
// numbers. Choose another player; whenever their Attacks deal unblocked damage this turn, they
// apply Poison.
// Singleplayer rework (see .claude/loadmap.md "調合"):
// - DRO on (案1): no other player to buff, so instead this attaches a new "Venomous" enchantment
//   (VenomousEnchantmentSolo) to every un-enchanted Attack card currently in hand -- permanent (not
//   "this turn only"), since enchantments stick to the card rather than expiring.
// - DRO off (xDRO): matches the original -- applies vanilla's OWN ConcoctPower (Core/Models/Powers/
//   ConcoctPower.cs) to the owner directly. That power's trigger already checks `dealer == Owner`
//   with no multiplayer-specific logic, so it works correctly targeted at ourselves with no Solo
//   reimplementation needed. Costs 0 like the original (Rework keeps the higher cost).
[Pool(typeof(SilentCardPool))]
public sealed class ConcoctSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "CONCOCT";

    protected override string OriginalVanillaCardPool => "silent";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<PoisonPower>() };

    // 1m (the Rework/案1 base) since Rework is this mod's default variant; AfterCloned overwrites
    // this (and the cost) to the xDRO values when that branch is active instead.
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Venomous", 1m) };

    public ConcoctSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        if (DroActiveForDisplay)
        {
            DynamicVars["Venomous"].BaseValue = 1m;
        }
        else
        {
            DynamicVars["Venomous"].BaseValue = 3m;
            EnergyCost.SetCustomBaseCost(0);
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
            await PowerCmd.Apply<ConcoctPower>(choiceContext, Owner.Creature, DynamicVars["Venomous"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Venomous"].UpgradeValueBy(1m);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        LocString branch = new LocString("cards", Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
        DynamicVars.AddTo(branch);
        description.Add("DroEffectText", branch.GetFormattedText());
    }
}
