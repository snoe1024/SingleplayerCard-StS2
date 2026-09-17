using System.Linq;
using System.Reflection;
using BaseLib.Config;
using Godot;
using MegaCrit.Sts2.Core.Platform;

namespace SingleplayerCard.SingleplayerCardCode.Config;

[ConfigHoverTipsByDefault]
public sealed class SingleplayerCardConfig : SimpleModConfig
{
    private static bool IsPublicBetaAvailable()
    {
        PlatformBranch branch = PlatformUtil.GetPlatformBranch();
        return branch is PlatformBranch.PublicBeta or PlatformBranch.PrivateBeta or PlatformBranch.DevTest;
    }

    public static bool ApplyInMultiplayer { get; set; } = false;

    private static readonly PropertyInfo[] CardVariantProperties = typeof(SingleplayerCardConfig)
        .GetProperties(BindingFlags.Public | BindingFlags.Static)
        .Where(p => p.GetCustomAttribute<CardVariantForAttribute>() != null)
        .ToArray();

    [ConfigButton("Apply")]
    private void SetAllCardsToRework()
    {
        foreach (PropertyInfo property in CardVariantProperties)
        {
            if (property.PropertyType == typeof(CardVariant))
            {
                property.SetValue(null, CardVariant.Rework);
            }
            else if (property.PropertyType == typeof(CardVariantNoOriginal))
            {
                property.SetValue(null, CardVariantNoOriginal.Rework);
            }
        }
        Changed();
    }

    [ConfigButton("Apply")]
    private void SetAllCardsToOriginal()
    {
        foreach (PropertyInfo property in CardVariantProperties)
        {
            if (property.PropertyType == typeof(CardVariant))
            {
                property.SetValue(null, CardVariant.Original);
            }
            else if (property.PropertyType == typeof(CardVariantOriginalOnly))
            {
                property.SetValue(null, CardVariantOriginalOnly.Original);
            }
        }
        Changed();
    }

    [ConfigButton("Apply")]
    private void SetAllCardsToDisabled()
    {
        foreach (PropertyInfo property in CardVariantProperties)
        {
            if (property.PropertyType == typeof(CardVariant))
            {
                property.SetValue(null, CardVariant.Disabled);
            }
            else if (property.PropertyType == typeof(CardVariantNoOriginal))
            {
                property.SetValue(null, CardVariantNoOriginal.Disabled);
            }
            else if (property.PropertyType == typeof(CardVariantOriginalOnly))
            {
                property.SetValue(null, CardVariantOriginalOnly.Disabled);
            }
        }
        Changed();
    }

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
    [XdroUnavailableReason(XdroUnavailableReason.Identical)]
    public static CardVariantOriginalOnly GangUp { get; set; } = CardVariantOriginalOnly.Original;

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
