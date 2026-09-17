using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

public sealed class HelpingHandPowerSolo : SingleplayerCardPower
{
    private bool _consumed;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!_consumed && target == Owner)
        {
            _consumed = true;
            return 2m;
        }

        return 1m;
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
    {
        if (_consumed && creature == Owner)
        {
            await PowerCmd.Remove(this);
        }
    }
}
