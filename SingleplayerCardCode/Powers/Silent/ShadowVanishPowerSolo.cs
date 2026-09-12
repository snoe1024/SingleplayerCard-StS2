using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Cards.Silent;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Silent;

// Backs ShadowVanishSolo (see .claude/loadmap.md "影隠し" 案1). Thin subclass of vanilla
// TemporaryDexterityPower (used internally by many official cards) that just points OriginModel
// at our own card so tooltips/titles attribute correctly.
public sealed class ShadowVanishPowerSolo : TemporaryDexterityPower
{
    public override AbstractModel OriginModel => ModelDb.Card<ShadowVanishSolo>();
}
