using BaseLib.Abstracts;
using BaseLib.Extensions;
using SingleplayerCard.SingleplayerCardCode.Config;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SingleplayerCard.SingleplayerCardCode.Cards;

public abstract class SingleplayerCardCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    // Snapshot of the Duplicate Rework Option (see .claude/loadmap.md and DuplicateReworkManager) at
    // the moment this specific card instance actually became an owned/mutable card. [SavedProperty]
    // persists it per-instance (survives save/load and run history reconstruction, per
    // Guilty/MadScience's use of the same attribute in the vanilla source), so a card's actual effect
    // and title never retroactively change just because the player later flips the option -- old
    // runs/decks keep showing what was really true when the card was made.
    //
    // This CANNOT be a property initializer: every CardModel type has exactly one canonical instance,
    // built once at boot via ModelDb.Init()'s Activator.CreateInstance (see decompiled ModelDb.cs),
    // and every owned copy is derived from it via AbstractModel.MutableClone(), which uses
    // MemberwiseClone() -- a shallow field copy that never re-runs the constructor or field/property
    // initializers. A property initializer here would only ever run once, for that one boot-time
    // canonical instance, and every subsequent clone would just inherit that same frozen value
    // forever regardless of when it was actually obtained. AfterCloned() is the hook CardModel
    // documents for exactly this ("clean up shallow-copied references" after MemberwiseClone), and it
    // runs before FromSerializable applies saved [SavedProperty] values on top, so loading a real
    // historical value still correctly overrides this.
    [SavedProperty]
    public bool DroWasOnAtCreation { get; private set; }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        DroWasOnAtCreation = DuplicateReworkManager.IsReworkActive(OriginalVanillaCardId);
    }

    // Whether the Rework (DRO-on) effect should be shown/used right now. The canonical instance
    // (Card Library/compendium browsing, not an owned card -- see IsCanonical) never goes through
    // AfterCloned, so it reads the live option instead of a frozen value; an owned instance uses its
    // own frozen DroWasOnAtCreation. Cards with a DRO-dependent effect should gate their OnPlay,
    // OnUpgrade, and description text on this rather than reading DroWasOnAtCreation or
    // DuplicateReworkManager directly (see CoordinateSolo for a worked example).
    protected bool DroActiveForDisplay => IsCanonical ? DuplicateReworkManager.IsReworkActive(OriginalVanillaCardId) : DroWasOnAtCreation;

    // The vanilla card's own title text where we have one (OriginalVanillaCardId), falling back to
    // this mod's own ".title" key otherwise (e.g. for any wholly original card this mod might add
    // later). Every ported card's name should track vanilla's exactly -- including if vanilla ever
    // retranslates or renames it -- rather than us maintaining a second, potentially-drifting copy.
    private string BaseTitleText => OriginalVanillaCardId != null
        ? new LocString("cards", OriginalVanillaCardId + ".title").GetFormattedText()
        : TitleLocString.GetFormattedText();

    // Single-letter DRO indicator as a SUFFIX rather than a prefix, so it doesn't shift the Card
    // Library's alphabetical sort order: "R" for the Rework/DRO-on effect, "S" for the Solo/xDRO-off
    // fallback. No leading space in Japanese (a single trailing letter reads naturally there); one
    // leading space in every other supported language.
    private string DroSuffix => (LocManager.Instance.Language == "jpn" ? "" : " ") + (DroActiveForDisplay ? "R" : "S");

    // Lets the title alone distinguish which effect a card instance has, without opening its
    // description. Reimplements CardModel.Title's upgrade-suffix logic rather than calling base.Title,
    // since the base implementation is hardwired to this card's own TitleLocString.
    //
    // DroSuffix goes BEFORE the upgrade "+N" marker, not after: a friend of the user's testing the mod
    // reported that "+"-then-"R"/"S" reads as though the "+" and the DRO letter are one combined token,
    // making the upgrade marker easy to miss. Vanilla's own "+" is the more important/frequent signal
    // (players scan for it constantly), so it stays rightmost, closest to the plain eye-scan position.
    public override string Title
    {
        get
        {
            string title = BaseTitleText + DroSuffix;
            if (IsUpgraded)
            {
                title += MaxUpgradeLevel > 1 ? $"+{CurrentUpgradeLevel}" : "+";
            }
            return title;
        }
    }

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

    // Same-assembly accessor for code outside this class hierarchy (e.g. CardPoolGetUnlockedCardsPatch)
    // that needs a card's vanilla id without needing OriginalVanillaCardId itself to be more than
    // protected.
    internal string? VanillaCardIdForConfig => OriginalVanillaCardId;

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