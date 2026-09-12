using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Orbs;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Defect;

// Backs HibernateSolo (see .claude/loadmap.md "冬眠" 案1). Frost orbs already trigger their
// Passive once automatically at end of turn (engine-driven). This power adds ONE extra manual
// trigger right before that, so the net effect is "Frost triggers twice this turn", then removes
// itself.
public sealed class HibernatePowerSolo : SingleplayerCardPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => new[] { HoverTipFactory.FromOrb<FrostOrb>() };

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        foreach (FrostOrb orb in Owner.Player.PlayerCombatState!.OrbQueue.Orbs.OfType<FrostOrb>().ToList())
        {
            await OrbCmd.Passive(choiceContext, orb, null);
        }

        await PowerCmd.Remove(this);
    }
}
