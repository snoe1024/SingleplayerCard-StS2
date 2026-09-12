using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Enchantments;

namespace SingleplayerCard.SingleplayerCardCode.Cards.Silent;

// Original multiplayer card: CONCOCT (Uncommon, 0 cost, Skill). Choose another player; whenever
// their Attacks deal unblocked damage this turn, they apply 3(4) Poison.
// Singleplayer rework (see .claude/loadmap.md "調合" 案1): no other player to buff, so instead this
// attaches a new "Venomous 1(2)" enchantment (VenomousEnchantmentSolo) to every un-enchanted Attack
// card currently in hand -- permanent (not "this turn only"), since enchantments stick to the card
// rather than expiring.
[Pool(typeof(SilentCardPool))]
public sealed class ConcoctSolo : SingleplayerCardCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

    protected override string OriginalVanillaCardId => "CONCOCT";

    protected override string OriginalVanillaCardPool => "silent";

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromPower<PoisonPower>() };

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new DynamicVar("Venomous", 1m) };

    public ConcoctSolo() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var targets = CardPile.GetCards(Owner, PileType.Hand).Where(c => c.Type == CardType.Attack && c.Enchantment == null).ToList();
        foreach (CardModel card in targets)
        {
            CardCmd.Enchant<VenomousEnchantmentSolo>(card, DynamicVars["Venomous"].BaseValue);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Venomous"].UpgradeValueBy(1m);
    }
}
