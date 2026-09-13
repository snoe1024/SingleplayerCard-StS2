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

// Backs CacophonySolo (see .claude/loadmap.md "不協和音"). Identical mechanism to vanilla
// CACOPHONY_POWER (count down a per-draw counter, deal damage and reset when it hits 0) for BOTH
// branches -- only the draw threshold and damage amount (Amount) differ:
// - DRO on (案1): the original draw-count window (33) is far too large for a single player to
//   realistically hit, so this lowers both the draw threshold and the damage.
// - DRO off (xDRO): matches the original exactly (33 draws / 66(99) damage).
// Threshold is set by CacophonySolo right after PowerCmd.Apply returns, same pattern as
// GenerousGiftPowerSolo.GenerateUpgraded/ConstellationPowerSolo's extra properties -- the card also
// pokes DynamicVars.Cards.BaseValue directly at that point so even the FIRST countdown cycle starts
// from the right branch's threshold, not CanonicalVars' hardcoded default.
public sealed class CacophonyPowerSolo : SingleplayerCardPower
{
    public int Threshold { get; set; } = 12;

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
