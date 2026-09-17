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

public sealed class HibernatePowerSolo : SingleplayerCardPower
{
    protected override string? OriginalVanillaPowerId => "HIBERNATE_POWER";

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
