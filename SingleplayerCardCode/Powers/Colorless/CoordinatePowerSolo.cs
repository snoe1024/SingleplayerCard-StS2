using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

public sealed class CoordinatePowerSolo : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<CoordinateSolo>();
}
