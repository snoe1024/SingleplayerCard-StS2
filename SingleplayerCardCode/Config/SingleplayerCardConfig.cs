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
public sealed class SingleplayerCardConfig : SimpleModConfig
{
    private static bool IsPublicBetaAvailable()
    {
        PlatformBranch branch = PlatformUtil.GetPlatformBranch();
        return branch is PlatformBranch.PublicBeta or PlatformBranch.PrivateBeta or PlatformBranch.DevTest;
    }

    // --- アイアンクラッドのカード ---

    [ConfigSection("IroncladCards", CollapsedByDefault = true)]
    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("DEMONIC_SHIELD")]
    public static CardVariant DemonicShield { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("TANK")]
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
    public static CardVariantNoOriginal Tutor { get; set; } = CardVariantNoOriginal.Rework;

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
    public static CardVariantNoOriginal OneForAll { get; set; } = CardVariantNoOriginal.Rework;

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
    public static CardVariantNoOriginal TagTeam { get; set; } = CardVariantNoOriginal.Rework;

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
    public static CardVariantNoOriginal Intercept { get; set; } = CardVariantNoOriginal.Rework;

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
    public static CardVariantNoOriginal Mimicry { get; set; } = CardVariantNoOriginal.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("RALLY")]
    public static CardVariant Gather { get; set; } = CardVariant.Rework;

    [ConfigDropdownOverrideLocalization("CARD_VARIANT")]
    [CardVariantFor("BEACON_OF_HOPE")]
    public static CardVariantNoOriginal BeaconOfHope { get; set; } = CardVariantNoOriginal.Rework;

    public override void SetupConfigUI(Control optionContainer)
    {
        GenerateOptionsForAllProperties(optionContainer);
    }
}
