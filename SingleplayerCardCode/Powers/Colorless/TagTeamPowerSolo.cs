using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

public sealed class TagTeamPowerSolo : SingleplayerCardPower
{
    protected override string? OriginalVanillaPowerId => "TAG_TEAM_POWER";
    
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
