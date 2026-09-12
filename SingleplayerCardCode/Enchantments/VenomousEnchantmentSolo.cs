using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Enchantments;

// New "Venomous N" enchantment introduced by ConcoctSolo (see .claude/loadmap.md "調合" 案1):
// whenever the enchanted card deals unblocked Attack damage, apply N Poison to the target.
// Only Attack cards can carry it (EnchantmentModel.CanEnchant already restricts to Attack/Skill/
// Power-excluded types; this narrows further to Attack only, matching the card text).
public sealed class VenomousEnchantmentSolo : EnchantmentModel
{
    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card) && card.Type == CardType.Attack;
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (cardSource == Card && props.IsPoweredAttack() && result.UnblockedDamage > 0)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, target, Amount, dealer, Card);
        }
    }
}
