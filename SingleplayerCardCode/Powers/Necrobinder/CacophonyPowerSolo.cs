using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Necrobinder;

public sealed class CacophonyPowerSolo : SingleplayerCardPower
{
    public int Threshold { get; set; } = 12;

    protected override string? OriginalVanillaPowerId => "CACOPHONY_POWER";

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => DynamicVars.Cards.IntValue;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[] { new CardsVar(12) };

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        DynamicVars.Cards.BaseValue--;
        InvokeDisplayAmountChanged();
        if (DynamicVars.Cards.IntValue <= 0)
        {
            Creature? enemy = Owner.Player.RunState.Rng.CombatTargets.NextItem(CombatState.HittableEnemies);
            DynamicVars.Cards.BaseValue = Threshold;
            InvokeDisplayAmountChanged();
            await Cmd.Wait(0.5f);
            if (enemy != null)
            {
                await CreatureCmd.Damage(choiceContext, enemy, Amount, ValueProp.Unpowered, Owner);
            }
        }
    }
}
