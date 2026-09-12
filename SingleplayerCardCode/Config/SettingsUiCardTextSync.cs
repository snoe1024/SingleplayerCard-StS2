using System.Collections.Generic;
using System.Reflection;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Populates each SingleplayerCardConfig dropdown's row label by reading the vanilla card's own title
// -- never a hand-authored copy -- so a future vanilla retranslation or rename is picked up
// automatically. BaseLib's GetLabelText only ever reads "settings_ui"."{ModPrefix}{PROPERTY_NAME}.title"
// with no hook to point it at a different table (confirmed via decompile), so this writes the resolved
// title directly into that table via LocTable.MergeWith (a plain public dictionary merge) before the
// settings screen ever reads it.
//
// Hover text is NOT derived this way: an earlier version tried resolving it live from each card's own
// ".descriptionRework"/".descriptionXdro" against the canonical instance's DynamicVars, but that has
// two real problems raised during testing -- {Var:diff()} only renders a single live-colored number,
// not the base(upgraded) notation loadmap.md actually uses, and any resolution hiccup (a missing key,
// an unexpected var) would leak literal "{Var:diff()}" text into a tooltip with no visual indication
// anything went wrong. So hover text stays hand-authored per card
// ("{PROPERTY}.hover.desc.rework"/".hover.desc.xdro" in settings_ui.json, including base(upgraded)
// values as literal text), only assembled through the three shared CONFIG_HOVER_TEXT_* templates.
internal static class SettingsUiCardTextSync
{
    // Matches TypePrefix.GetPrefix()'s derivation (uppercased root namespace + '-') for this mod's
    // own namespace -- hardcoded rather than computed since every loc key in this codebase already
    // assumes this exact literal (see e.g. localization/*/cards.json).
    private const string ModPrefix = "SINGLEPLAYERCARD-";

    public static void Sync()
    {
        LocTable cardsTable = LocManager.Instance.GetTable("cards");
        LocTable settingsTable = LocManager.Instance.GetTable("settings_ui");
        Dictionary<string, string> overrides = new();

        foreach (PropertyInfo property in typeof(SingleplayerCardConfig).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            CardVariantForAttribute? variantFor = property.GetCustomAttribute<CardVariantForAttribute>();
            if (variantFor == null)
            {
                continue;
            }

            string settingsPrefix = ModPrefix + StringHelper.Slugify(property.Name);

            string vanillaTitleKey = variantFor.VanillaCardId + ".title";
            if (cardsTable.HasEntry(vanillaTitleKey))
            {
                overrides[settingsPrefix + ".title"] = cardsTable.GetRawText(vanillaTitleKey);
            }

            // BuildHoverText reads the ".hover.desc.rework"/".hover.desc.xdro" entries already present
            // in settingsTable (hand-authored, see settings_ui.json) and combines them through the
            // shared templates; the RESULT gets written back under the plain ".hover.desc" key that
            // BaseLib's own AddHoverTip() (invoked later, inside GenerateOptionsForAllProperties) reads.
            overrides[settingsPrefix + ".hover.desc"] = BuildHoverText(property, settingsTable);
        }

        settingsTable.MergeWith(overrides);
    }

    private static string BuildHoverText(PropertyInfo property, LocTable settingsTable)
    {
        string settingsPrefix = ModPrefix + StringHelper.Slugify(property.Name);
        string reworkText = settingsTable.HasEntry(settingsPrefix + ".hover.desc.rework")
            ? settingsTable.GetRawText(settingsPrefix + ".hover.desc.rework")
            : "";

        XdroUnavailableReasonAttribute? reasonAttribute = property.GetCustomAttribute<XdroUnavailableReasonAttribute>();
        if (reasonAttribute != null)
        {
            return reasonAttribute.Reason == XdroUnavailableReason.Breaks
                ? FormatHoverTemplate("CONFIG_HOVER_TEXT_ORIGINAL_COLLAPSE", reworkText, null)
                : FormatHoverTemplate("CONFIG_HOVER_TEXT_REWORK_MATCH", reworkText, null);
        }

        string xdroKey = settingsPrefix + ".hover.desc.xdro";
        if (settingsTable.HasEntry(xdroKey))
        {
            return FormatHoverTemplate("CONFIG_HOVER_TEXT_NORMAL", reworkText, settingsTable.GetRawText(xdroKey));
        }

        return FormatHoverTemplate("CONFIG_HOVER_TEXT_REWORK_MATCH", reworkText, null);
    }

    private static string FormatHoverTemplate(string templateKey, string reworkText, string? originalText)
    {
        LocString template = new("settings_ui", ModPrefix + templateKey);
        template.Add("rework", new LocString("settings_ui", ModPrefix + "CARD_VARIANT.Rework").GetFormattedText());
        template.Add("original", new LocString("settings_ui", ModPrefix + "CARD_VARIANT.Original").GetFormattedText());
        template.Add("reworked_card_text", reworkText);
        template.Add("original_card_text", originalText ?? "");
        return template.GetFormattedText();
    }
}
