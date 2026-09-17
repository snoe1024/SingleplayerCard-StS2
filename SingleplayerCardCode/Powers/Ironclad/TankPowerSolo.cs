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

public sealed class TankPowerSolo : SingleplayerCardPower
{
    private const string _damageIncreaseKey = "DamageIncrease";

    public const decimal damageIncrease = 1.5m;

    protected override string? OriginalVanillaPowerId => "TANK_POWER";

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
