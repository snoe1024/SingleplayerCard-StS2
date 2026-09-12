using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Silent;

// Backs StealthSolo (see .claude/loadmap.md "隠密" 案1). Vanilla SNEAKY_POWER grants Block whenever
// ANOTHER player attacks. Singleplayer has no other player, so this instead grants Block whenever
// the owner deals damage by means OTHER than an Attack card (poison, Choke, thorns-style relics,
// orbs, etc. -- see loadmap.md's own list of non-Attack damage sources).
public sealed class StealthPowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.Static(StaticHoverTip.Block) };

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (dealer == Owner && (cardSource == null || cardSource.Type != CardType.Attack))
        {
            await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null, fast: true);
        }
    }
}
