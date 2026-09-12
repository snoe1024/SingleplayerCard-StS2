using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs TagTeamSolo (see .claude/loadmap.md "タッグチーム" 案1, identical to its own xDRO). Vanilla
// TAG_TEAM_POWER excludes the applier's own Attacks (since the whole point was "another player's
// next Attack"); this version has no such exclusion since it's meant to apply to the owner's own
// next Attack against the target.
public sealed class TagTeamPowerSolo : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override int DisplayAmount => 1;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (card.Type != CardType.Attack || target != Owner)
        {
            return playCount;
        }

        return playCount + Amount;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        await PowerCmd.Remove(this);
    }
}
