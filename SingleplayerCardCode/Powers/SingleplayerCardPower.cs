using BaseLib.Abstracts;
using BaseLib.Extensions;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Helpers;

namespace SingleplayerCard.SingleplayerCardCode.Powers;

public abstract class SingleplayerCardPower : CustomPowerModel
{
    // Override in powers whose effect corresponds to a real vanilla Power of the same name (e.g.
    // KnockdownPowerSolo -> vanilla KNOCKDOWN_POWER, Id.Entry of the vanilla PowerModel subclass) so
    // the vanilla icon is reused instead of falling back to this mod's own placeholder art, which
    // doesn't exist for most of these. Mirrors OriginalVanillaCardId/OriginalVanillaCardPool on
    // SingleplayerCardCard, which does the same for card portraits. Leave null for powers with no
    // vanilla equivalent (e.g. ones backing a card whose vanilla version grants no dedicated Power).
    protected virtual string? OriginalVanillaPowerId => null;

    // Vanilla ships one packed atlas sprite per power (small icon) and a separate flat PNG (big
    // icon/hover), unlike cards which reuse a single portrait resource for both -- see
    // MegaCrit.Sts2.Core.Models.PowerModel's own (non-virtual, so unusable directly by a
    // CustomPowerModel) PackedIconPath/BigIconPath for the paths this mirrors.
    private string? VanillaPackedIconPath => OriginalVanillaPowerId != null
        ? ImageHelper.GetImagePath($"atlases/power_atlas.sprites/{OriginalVanillaPowerId.ToLowerInvariant()}.tres")
        : null;

    private string? VanillaBigIconPath => OriginalVanillaPowerId != null
        ? ImageHelper.GetImagePath($"powers/{OriginalVanillaPowerId.ToLowerInvariant()}.png")
        : null;

    //Loads from SingleplayerCard/images/powers/your_power.png
    public override string CustomPackedIconPath => VanillaPackedIconPath ?? $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => VanillaBigIconPath ?? $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}