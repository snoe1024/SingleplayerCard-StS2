using BaseLib.Abstracts;
using BaseLib.Extensions;
using GamblerMod.GamblerModCode.Patch;
using MegaCrit.Sts2.Core.Combat;
using SingleplayerCard.SingleplayerCardCode.Config;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace SingleplayerCard.SingleplayerCardCode.Cards;

public abstract class SingleplayerCardCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target), IDynamicCardDescription
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
    // Setter MUST be public (not private): confirmed via a real crash on Continue ("Property set
    // method not found" thrown from CardModel.FromSerializable while restoring this exact property)
    // that BaseLib's [SavedProperty] restore path can't reach a non-public setter -- every vanilla
    // example of this attribute (Guilty.CombatsSeen, MadScience.TinkerTimeType/TinkerTimeRider) uses a
    // fully public setter for the same reason. AssertMutable() isn't added here since the base
    // CardModel setters vanilla itself uses for [SavedProperty] don't gate on it either for a plain
    // bool like this; nothing outside AfterCloned() has a reason to set it anyway.
    [SavedProperty]
    public bool DroWasOnAtCreation { get; set; }

    // Guards AfterCloned's live-config guess below so it only ever fires ONCE per logical card, not
    // on every MutableClone(). Confirmed via diagnostic logging (.claude/todo.md item 2,
    // ImitationLearningSolo repro) that MutableClone() -- and therefore AfterCloned() -- fires again
    // every time an owned card gets a fresh per-combat instance (entering a new combat), not just on
    // a genuinely new draft or a save-file reload. MemberwiseClone() (which MutableClone() uses) is a
    // plain shallow field copy, so it already correctly carries DroWasOnAtCreation's real, previously-
    // frozen value forward into that fresh per-combat instance -- but AfterCloned() then unconditionally
    // OVERWROTE it with whatever the option happens to say RIGHT NOW, silently re-freezing an already-
    // owned card to the live setting every single combat. This field is plain (not [SavedProperty]),
    // so it does NOT survive an actual save-file round trip -- a genuine reload clones fresh from the
    // canonical instance (see CardModel.FromSerializable's SaveUtil...ToMutable() call) same as a first
    // draft, correctly re-arming this guard for one more guess, which FromSerializable's subsequent
    // save.Props.Fill() then immediately overrides with the true historical value same as before.
    private bool _droBranchGuessed;

    protected override void AfterCloned()
    {
        base.AfterCloned();
        if (!_droBranchGuessed)
        {
            DroWasOnAtCreation = DuplicateReworkManager.IsReworkActive(OriginalVanillaCardId);
            _droBranchGuessed = true;
        }
        RefreshDroBranchState();
    }

    // Whether the Rework (DRO-on) effect should be shown/used right now. The canonical instance
    // (Card Library/compendium browsing, not an owned card -- see IsCanonical) never goes through
    // AfterCloned, so it reads the live option instead of a frozen value; an owned instance uses its
    // own frozen DroWasOnAtCreation. Cards with a DRO-dependent effect should gate their OnPlay,
    // OnUpgrade, and description text on this rather than reading DroWasOnAtCreation or
    // DuplicateReworkManager directly (see CoordinateSolo for a worked example).
    //
    // internal (not just protected): some cards grant a custom Power whose own trigger CONDITION
    // (not just its amount) differs by branch, not just the amount applied -- that Power needs to
    // read the applying card's DRO state at apply time to know which behavior to run (see
    // StealthPowerSolo, which captures this into its own [SavedProperty] field via AfterApplied).
    // A Power is a different, unrelated class hierarchy from SingleplayerCardCard, so `protected`
    // alone isn't reachable from it -- same-assembly access is the minimal widening that covers this.
    internal protected bool DroActiveForDisplay => IsCanonical ? DuplicateReworkManager.IsReworkActive(OriginalVanillaCardId) : DroWasOnAtCreation;

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

    protected virtual bool DroVersionExists => false;

    // Cards that bake a DRO-branch-dependent numeric/cost/keyword value (e.g. MidnightSolo's
    // Damage.BaseValue, EnergySurgeSolo's custom cost) override this instead of stamping that state
    // directly in their own AfterCloned(). AfterCloned() runs BEFORE CardModel.FromSerializable
    // restores this card's real DroWasOnAtCreation on a reloaded save (see that property's doc
    // comment) -- so a value baked only in AfterCloned keeps whichever guess the LIVE config
    // happened to produce at that instant, silently wrong forever once the option is changed while a
    // save exists (confirmed root cause of the "text says R but the number is xDRO's" bug reports).
    // Called again from GetDynamicDescription below on every real Description-getter access
    // (DynamicCardDisplayPatch's Harmony postfix), which only ever happens well after any load has
    // fully finished, so it self-heals regardless of AfterCloned/FromSerializable ordering.
    // MUST be written as a pure function of (DroActiveForDisplay, IsUpgraded/CurrentUpgradeLevel),
    // never an incremental "+= delta" -- it can fire many times over a card's life, so anything that
    // only conditionally mutates one direction (e.g. "if xDRO, AddKeyword") needs an explicit opposite
    // branch too, or a second call with the corrected value can't undo a wrong first guess.
    protected virtual void RefreshDroBranchState() { }

    public LocString GetDynamicDescription(LocString original)
    {
        // The canonical/Card Library instance is never mutable (see IsCanonical), and every override
        // of this (SetCustomBaseCost, AddKeyword/RemoveKeyword, DynamicVars.X.BaseValue setters) goes
        // through AssertMutable() somewhere underneath -- calling it here for canonical would throw
        // the moment a player opens the Card Library on any of these cards. This does mean the Library
        // preview keeps showing CanonicalVars' fixed representative number instead of live-branch
        // numbers regardless (a separate, lower-priority, purely cosmetic known limitation).
        if (IsMutable)
        {
            RefreshDroBranchState();
        }
        if (!DroVersionExists) return original;

        return new LocString("cards", this.Id.Entry + (DroActiveForDisplay ? ".descriptionRework" : ".descriptionXdro"));
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

    // CardModel.GetDescriptionForPile adds several "automatic" variables (IfUpgradedVar, energyPrefix,
    // etc. -- see decompiled CardModel.cs) to the outer description LocString AFTER
    // AddExtraArgsToDescription returns. That's too late for the .descriptionRework/.descriptionXdro
    // branch pattern used throughout this mod (a separate LocString built and formatted via
    // GetFormattedText() INSIDE AddExtraArgsToDescription, then embedded as "DroEffectText") --
    // GetFormattedText() resolves eagerly against ONLY its own variable dictionary (LocString.Add(name,
    // LocString) confirms this: it just calls variable.GetFormattedText() immediately, there's no
    // deferred/inherited-scope form). Confirmed via two real crashes ("No source extension could handle
    // the selector named IfUpgraded", then energyPrefix) from templates that used those tokens without
    // this. Call this on `branch` before formatting it, for whichever of these tokens that branch's
    // template actually references.
    protected void AddCommonDescriptionArgs(LocString target)
    {
        target.Add(new IfUpgradedVar(IsUpgraded ? UpgradeDisplay.Upgraded : UpgradeDisplay.Normal));
        target.Add("energyPrefix", EnergyIconHelper.GetPrefix(this));
    }

    // SingleplayerOnly (not the CardModel default of None) so these never appear in an actual
    // multiplayer game's pools by default: CardPoolModel.GetUnlockedCards removes SingleplayerOnly
    // cards whenever the run itself is multiplayer, exactly mirroring how it already removes vanilla's
    // own MultiplayerOnly cards from singleplayer pools. CardPoolGetUnlockedCardsPatch is what adds
    // these back in for multiplayer specifically, and only when SingleplayerCardConfig.ApplyInMultiplayer
    // (or the multiplayer host's equivalent, see MultiplayerConfigAuthority) is on.
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.SingleplayerOnly;

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