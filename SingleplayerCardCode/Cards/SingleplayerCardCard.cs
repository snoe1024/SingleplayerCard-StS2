using BaseLib.Abstracts;
using BaseLib.Extensions;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;

namespace SingleplayerCard.SingleplayerCardCode.Cards;

public abstract class SingleplayerCardCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    // Override in ported cards to reuse the original multiplayer card's vanilla portrait instead of
    // a mod-specific placeholder image. Values are the original card's Id.Entry (e.g. "DEMONIC_SHIELD")
    // and its vanilla CardPoolModel folder name (e.g. "ironclad", matching IroncladCardPool.Title
    // etc. in the decompiled source). Leave both null for cards that need their own new artwork.
    // Vanilla ships only one portrait resource per card (no separate small/big art the way BaseLib's
    // own convention expects for original mod art), so both the small (PortraitPath/BetaPortraitPath)
    // and big (CustomPortraitPath) hooks below point at the same atlas sprite -- matching how the
    // base game itself just scales that same texture up for its own hover/zoom view.
    protected virtual string? OriginalVanillaCardId => null;
    protected virtual string? OriginalVanillaCardPool => null;

    private string? VanillaPortraitPath => OriginalVanillaCardId != null && OriginalVanillaCardPool != null
        ? ImageHelper.GetImagePath($"atlases/card_atlas.sprites/{OriginalVanillaCardPool}/{OriginalVanillaCardId.ToLowerInvariant()}.tres")
        : null;

    //Image size:
    //Normal art: 1000x760 (Using 500x380 should also work, it will simply be scaled.)
    //Full art: 606x852
    public override string CustomPortraitPath => VanillaPortraitPath ?? $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Smaller variants of card images for efficiency:
    //Smaller variant of fullart: 250x350
    //Smaller variant of normalart: 250x190

    //Uses card_portraits/card_name.png as image path. These should be smaller images.
    public override string PortraitPath => VanillaPortraitPath ?? $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => VanillaPortraitPath ?? $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
}