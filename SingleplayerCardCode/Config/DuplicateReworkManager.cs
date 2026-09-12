using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Single resolution point for a card's Duplicate Rework Option state (DRO, see .claude/loadmap.md).
// Every ported card reads its state through this -- via SingleplayerCardCard.DroActiveForDisplay,
// passing its own OriginalVanillaCardId -- instead of touching SingleplayerCardConfig directly, so no
// card's own code needs to change if the resolution logic changes (e.g. once multiplayer support
// exists and this needs to defer to a host's setting instead of the local client's).
//
// The mapping from vanilla card id to config property is built once via reflection, keyed by each
// property's [CardVariantFor] attribute, so adding a new card's dropdown to SingleplayerCardConfig is
// the only step needed -- nothing here has to be touched per card.
public static class DuplicateReworkManager
{
    private static readonly Dictionary<string, PropertyInfo> PropertiesByVanillaId = BuildLookup();

    private static Dictionary<string, PropertyInfo> BuildLookup()
    {
        Dictionary<string, PropertyInfo> map = new();
        foreach (PropertyInfo property in typeof(SingleplayerCardConfig).GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            CardVariantForAttribute? attribute = property.GetCustomAttribute<CardVariantForAttribute>();
            if (attribute != null)
            {
                map[attribute.VanillaCardId] = property;
            }
        }
        return map;
    }

    private static CardVariant Resolve(string? vanillaCardId, Player? contextPlayer = null)
    {
        if (vanillaCardId == null || !PropertiesByVanillaId.TryGetValue(vanillaCardId, out PropertyInfo? property))
        {
            return CardVariant.Rework;
        }
        object? value = property.GetValue(null);
        return value switch
        {
            CardVariant variant => variant,
            CardVariantNoOriginal noOriginal => noOriginal == CardVariantNoOriginal.Disabled ? CardVariant.Disabled : CardVariant.Rework,
            _ => CardVariant.Rework
        };
    }

    // Whether the Rework effect (as opposed to the Original/xDRO effect) should apply. A card whose
    // selection somehow ends up Disabled (e.g. an existing owned copy from before the player disabled
    // it) still needs SOME effect to run, so Disabled degrades to Rework here rather than being a
    // separate case every card would need to handle.
    public static bool IsReworkActive(string? vanillaCardId, Player? contextPlayer = null)
    {
        return Resolve(vanillaCardId, contextPlayer) != CardVariant.Original;
    }

    // Whether this card should be excluded from its pool entirely (see CardPoolGetUnlockedCardsPatch).
    public static bool IsDisabled(string? vanillaCardId, Player? contextPlayer = null)
    {
        return Resolve(vanillaCardId, contextPlayer) == CardVariant.Disabled;
    }
}
