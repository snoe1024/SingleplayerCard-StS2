using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Populates each SingleplayerCardConfig dropdown's row label and hover text by reading the actual
// vanilla card title and this mod's own card description text -- never a hand-authored copy -- so
// there is nothing to keep in sync if a vanilla title is retranslated or a card's effect/numbers
// change. BaseLib's settings UI (GetLabelText/AddHoverTip, see decompiled ModConfig.cs/
// NConfigOptionRow.cs) only ever reads from the "settings_ui" loc table by a fixed
// {ModPrefix}{PROPERTY_NAME}.title/.hover.desc key, with no hook to point it at a different table, so
// this writes the resolved text directly into "settings_ui" via LocTable.MergeWith (a plain public
// dictionary merge, see decompiled LocTable.cs) before the settings screen ever reads it.
internal static class SettingsUiCardTextSync
{
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

            string settingsPrefix = "SINGLEPLAYERCARD-" + StringHelper.Slugify(property.Name);

            string vanillaTitleKey = variantFor.VanillaCardId + ".title";
            if (cardsTable.HasEntry(vanillaTitleKey))
            {
                overrides[settingsPrefix + ".title"] = cardsTable.GetRawText(vanillaTitleKey);
            }

            overrides[settingsPrefix + ".hover.desc"] = BuildHoverText(property, cardsTable);
        }

        settingsTable.MergeWith(overrides);
    }

    private static string BuildHoverText(PropertyInfo property, LocTable cardsTable)
    {
        // Property names were deliberately chosen to match their card class name minus "Solo" (see
        // SingleplayerCardConfig's own per-card comments), so the card's own Id.Entry -- and its
        // description loc keys -- can be derived the same way BaseLib derives Id.Entry itself
        // (StringHelper.Slugify(typeName), see decompiled ModelDb.GetEntry) rather than needing yet
        // another attribute just to store it.
        string soloCardId = StringHelper.Slugify(property.Name) + "_SOLO";
        CardModel? card = FindCanonicalCard(soloCardId);

        string reworkKey = soloCardId + ".descriptionRework";
        string plainKey = soloCardId + ".description";
        string reworkText = ResolveCardText(card, cardsTable, cardsTable.HasEntry(reworkKey) ? reworkKey : plainKey);

        XdroUnavailableReasonAttribute? reasonAttribute = property.GetCustomAttribute<XdroUnavailableReasonAttribute>();
        if (reasonAttribute != null)
        {
            return reasonAttribute.Reason == XdroUnavailableReason.Breaks
                ? FormatHoverTemplate("CONFIG_HOVER_TEXT_ORIGINAL_COLLAPSE", reworkText, null)
                : FormatHoverTemplate("CONFIG_HOVER_TEXT_REWORK_MATCH", reworkText, null);
        }

        string xdroKey = soloCardId + ".descriptionXdro";
        if (cardsTable.HasEntry(xdroKey))
        {
            string xdroText = ResolveCardText(card, cardsTable, xdroKey);
            return FormatHoverTemplate("CONFIG_HOVER_TEXT_NORMAL", reworkText, xdroText);
        }

        // The card's xDRO branch hasn't been split out of a single ".description" yet (ongoing
        // per-card work) -- show whatever single effect currently exists rather than erroring.
        return FormatHoverTemplate("CONFIG_HOVER_TEXT_REWORK_MATCH", reworkText, null);
    }

    private static CardModel? FindCanonicalCard(string soloCardId)
    {
        System.Type? cardType = typeof(SingleplayerCardConfig).Assembly.GetTypes()
            .FirstOrDefault(t => ModelDb.GetEntry(t) == soloCardId && typeof(CardModel).IsAssignableFrom(t));
        return cardType == null ? null : ModelDb.GetByIdOrNull<CardModel>(ModelDb.GetId(cardType));
    }

    // Resolves a card description's raw text (which may contain {Var:diff()}-style placeholders)
    // against the canonical (never-upgraded) instance's own DynamicVars, matching what the card
    // actually shows in play at base value -- the "未UG版" text the user asked for.
    private static string ResolveCardText(CardModel? card, LocTable cardsTable, string key)
    {
        if (!cardsTable.HasEntry(key))
        {
            return "";
        }
        if (card == null)
        {
            return cardsTable.GetRawText(key);
        }
        LocString description = new("cards", key);
        card.DynamicVars.AddTo(description);
        return description.GetFormattedText();
    }

    private static string FormatHoverTemplate(string templateKey, string reworkText, string? originalText)
    {
        LocString template = new("settings_ui", templateKey);
        template.Add("rework", new LocString("settings_ui", "CARD_VARIANT.Rework").GetFormattedText());
        template.Add("original", new LocString("settings_ui", "CARD_VARIANT.Original").GetFormattedText());
        template.Add("reworked_card_text", reworkText);
        template.Add("original_card_text", originalText ?? "");
        return template.GetFormattedText();
    }
}
