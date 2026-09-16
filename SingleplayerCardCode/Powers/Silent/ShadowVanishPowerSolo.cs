using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using SingleplayerCard.SingleplayerCardCode.Cards.Silent;
using SingleplayerCard.SingleplayerCardCode.Extensions;

namespace SingleplayerCard.SingleplayerCardCode.Powers.Silent;

public sealed class ShadowVanishPowerSolo : CustomTemporaryPowerModelWrapper<ShadowVanishSolo, DexterityPower>
{
    public override string CustomPackedIconPath => ImageHelper.GetImagePath($"atlases/power_atlas.sprites/fade_power.tres");
    
    public override string CustomBigIconPath => ImageHelper.GetImagePath($"powers/fade_power.png");
    
    public override AbstractModel OriginModel => ModelDb.Card<ShadowVanishSolo>();
    
    public override LocString Title => new LocString("cards", "FADE.title");
}
