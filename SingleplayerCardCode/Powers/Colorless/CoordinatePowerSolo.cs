using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Cards.Colorless;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Colorless;

// Backs CoordinateSolo (see .claude/loadmap.md "連携" 案1). Thin subclass of vanilla
// TemporaryStrengthPower (used internally by many official cards) that just points OriginModel
// at our own card so tooltips/titles attribute correctly.
public sealed class CoordinatePowerSolo : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<CoordinateSolo>();
}
