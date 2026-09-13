using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.Core.Platform;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Per-card DRO/availability selection (see .claude/loadmap.md "効果の重複する一部のカードの効果を調整する"
// and "特定のマルチプレイヤーカードを出現しなくする"). Every ported card gets its own dropdown instead of
// one blanket on/off switch, replacing the old single DuplicateReworkOption toggle.
//
// [ConfigDropdownOverrideLocalization("CARD_VARIANT")] on every property means all of them -- both the
// 3-option CardVariant and the 2-option CardVariantNoOriginal ones -- share the same three underlying
// option-label keys (.../CARD_VARIANT.Rework, .Original, .Disabled) rather than needing per-card labels;
// see localization/*/settings_ui.json. [CardVariantFor("VANILLA_ID")] is this mod's own attribute (not
// BaseLib's) recording which vanilla card each property controls, consumed by DuplicateReworkManager's
// reflection-based lookup so no card's own code ever needs to reference its config property directly.
//
// [ConfigSection] only needs to be declared on the first property of a run of properties that share a
// section name -- BaseLib's SectionTracker keeps reusing the current section for any property with no
// [ConfigSection] attribute at all (see decompiled SimpleModConfig.GenerateOptionsForAllProperties), it
// only starts a new one when it sees a DIFFERENT non-null section name.
//
// [ConfigHoverTipsByDefault] turns on a hover tip for every property here without needing
// [ConfigHoverTip] repeated 36 times -- each one reads its text from
// "settings_ui"."{ModPrefix}{PROPERTY_NAME}.hover.desc". That key's actual VALUE, and each property's
// ".title" row label, are never hand-authored: SettingsUiCardTextSync.Sync() (called from
// SetupConfigUI below) computes them at runtime from the vanilla card's own title and this mod's own
// card description text, so neither can drift from a retranslation or a balance change. Only the
// three CONFIG_HOVER_TEXT_* templates and the CARD_VARIANT option labels live in settings_ui.json.
// [XdroUnavailableReason] on the CardVariantNoOriginal properties selects which template explains the
// missing Original choice, per two distinct reasons loadmap.md calls out: the xDRO effect either
// breaks outright (Breaks) or is word-for-word identical to the Rework effect anyway (Identical).
[ConfigHoverTipsByDefault]
public sealed class SingleplayerCardConfig : SimpleModConfig
{
    private static bool IsPublicBetaAvailable()
    {
        PlatformBranch branch = PlatformUtil.GetPlatformBranch();
        return branch is PlatformBranch.PublicBeta or PlatformBranch.PrivateBeta or PlatformBranch.DevTest;
    }

    // No [ConfigSection] here on purpose: this property comes before any section attribute, so it
    // renders as a standalone row above every character group instead of inside a collapsible section
    // (see the [ConfigSection] note below on how BaseLib decides section boundaries).
    //
    // Default OFF: this mod's effects should only ever change multiplayer for a player who explicitly
    // opts in, since every OTHER connected player is bound by this same value once it's turned on (see
    // MultiplayerConfigAuthority) -- an unexpected multiplayer pool change for someone who never agreed
    // to it would be a much worse surprise than an unexpected singleplayer one.
    public static bool ApplyInMultiplayer { get; set; } = false;

    // --- アイアンクラッドのカード ---

    [ConfigSection("IroncladCards", CollapsedByDefault = true)]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("DEMONIC_SHIELD")]
    public static CardVariant DemonicShield { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("TANK")]
    [XdroUnavailableReason(XdroUnavailableReason.Breaks)]
    public static CardVariantNoOriginal Tank { get; set; } = CardVariantNoOriginal.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("MIDNIGHT")]
    public static CardVariant Midnight { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("BLAZE")]
    public static CardVariant Blaze { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("OUTRAGE")]
    public static CardVariant Outrage { get; set; } = CardVariant.Rework;

    // --- サイレントのカード ---

    [ConfigSection("SilentCards", CollapsedByDefault = true)]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("FLANKING")]
    public static CardVariant PincerAttack { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("SNEAKY")]
    public static CardVariant Stealth { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("BLADE_SYMPHONY")]
    public static CardVariant BladeSymphony { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("CONCOCT")]
    public static CardVariant Concoct { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("FADE")]
    public static CardVariant ShadowVanish { get; set; } = CardVariant.Rework;

    // --- リージェントのカード ---

    [ConfigSection("RegentCards", CollapsedByDefault = true)]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("LARGESSE")]
    public static CardVariant GenerousGift { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("HAMMER_TIME")]
    [XdroUnavailableReason(XdroUnavailableReason.Breaks)]
    public static CardVariantNoOriginal HammerTime { get; set; } = CardVariantNoOriginal.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("PLOT")]
    public static CardVariant Scheme { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("CONSTELLATION")]
    public static CardVariant Constellation { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("TUTOR")]
    [XdroUnavailableReason(XdroUnavailableReason.Identical)]
    public static CardVariantOriginalOnly Tutor { get; set; } = CardVariantOriginalOnly.Original;

    // --- ネクロバインダーのカード ---

    [ConfigSection("NecrobinderCards", CollapsedByDefault = true)]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("LEGION_OF_BONE")]
    public static CardVariant BoneLegion { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("GLIMPSE_BEYOND")]
    public static CardVariant GlimpseBeyond { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("UNDERWORLD")]
    public static CardVariant Underworld { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("SOULBOUND")]
    public static CardVariant Soulbound { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("CACOPHONY")]
    public static CardVariant Cacophony { get; set; } = CardVariant.Rework;

    // --- ディフェクトのカード ---

    [ConfigSection("DefectCards", CollapsedByDefault = true)]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("ENERGY_SURGE")]
    public static CardVariant EnergySurge { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("IGNITION")]
    public static CardVariant Ignition { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("HIBERNATE")]
    public static CardVariant Hibernate { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("ONE_FOR_ALL")]
    [XdroUnavailableReason(XdroUnavailableReason.Identical)]
    public static CardVariantOriginalOnly OneForAll { get; set; } = CardVariantOriginalOnly.Original;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("IMITATION_LEARNING")]
    public static CardVariant ImitationLearning { get; set; } = CardVariant.Rework;

    // --- 無色カード ---

    [ConfigSection("ColorlessCards", CollapsedByDefault = true)]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("GANG_UP")]
    public static CardVariant GangUp { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("TAG_TEAM")]
    [XdroUnavailableReason(XdroUnavailableReason.Identical)]
    public static CardVariantOriginalOnly TagTeam { get; set; } = CardVariantOriginalOnly.Original;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("KNOCKDOWN")]
    public static CardVariant Knockdown { get; set; } = CardVariant.Rework;

    [ConfigVisibleIf(nameof(IsPublicBetaAvailable))]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("THE_BALL")]
    public static CardVariant TheBall { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("BELIEVE_IN_YOU")]
    public static CardVariant BelieveInYou { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("INTERCEPT")]
    public static CardVariant Intercept { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("HUDDLE_UP")]
    public static CardVariant WarCouncil { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("LIFT")]
    public static CardVariant HelpingHand { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("COORDINATE")]
    public static CardVariant Coordinate { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("MIMIC")]
    [XdroUnavailableReason(XdroUnavailableReason.Breaks)]
    public static CardVariantNoOriginal Mimicry { get; set; } = CardVariantNoOriginal.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("RALLY")]
    public static CardVariant Gather { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("BEACON_OF_HOPE")]
    [XdroUnavailableReason(XdroUnavailableReason.Breaks)]
    public static CardVariantNoOriginal BeaconOfHope { get; set; } = CardVariantNoOriginal.Rework;

    public override void SetupConfigUI(Control optionContainer)
    {
        SettingsUiCardTextSync.Sync();
        GenerateOptionsForAllProperties(optionContainer);
    }
}
