using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Ironclad;

// Backs TankSolo (see .claude/loadmap.md "タンク" 案1). Vanilla TANK_POWER increases self damage
// taken and halves damage taken by allies; singleplayer has no allies, so this rework keeps the
// "take more damage" downside but replaces the ally-protection upside with reflecting blocked
// attack damage back at the attacker (same pattern as vanilla REFLECT_POWER, but permanent instead
// of decrementing each turn, since this is meant to be the payoff for the extra damage taken).
public sealed class TankPowerSolo : SingleplayerCardPower
{
    private const string _damageIncreaseKey = "DamageIncrease";

    public const decimal damageIncrease = 1.5m;
    
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(_damageIncreaseKey, damageIncrease)
    ];
    
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != Owner || !props.IsPoweredAttack())
        {
            return 1m;
        }

        return base.DynamicVars["DamageIncrease"].BaseValue;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && result.BlockedDamage > 0 && props.IsPoweredAttack() && dealer != null)
        {
            await CreatureCmd.Damage(choiceContext, dealer, result.BlockedDamage, ValueProp.Unpowered, Owner);
        }
    }
}
