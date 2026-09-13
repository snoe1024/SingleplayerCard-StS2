using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// Single resolution point for a card's Duplicate Rework Option state (DRO, see .claude/loadmap.md).
// Every ported card reads its state through this -- via SingleplayerCardCard.DroActiveForDisplay,
// passing its own OriginalVanillaCardId -- instead of touching SingleplayerCardConfig directly, so no
// card's own code needs to change if the resolution logic changes.
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

    // Every vanilla card id this mod has a config property for -- used by CardPoolGetUnlockedCardsPatch
    // to know which vanilla MultiplayerOnly cards should be replaced by this mod's Solo versions when
    // multiplayer support is active, and by BuildLocalSnapshot below.
    public static IEnumerable<string> AllPortedVanillaCardIds => PropertiesByVanillaId.Keys;

    // Snapshot of every ported card's LOCALLY configured variant, ignoring multiplayer host authority
    // entirely -- used to build the message a multiplayer host broadcasts to clients (see
    // MultiplayerConfigAuthority). Resolve() below is what a client (and everyone in singleplayer) uses
    // day-to-day; this is the one place that must always read the local machine's own config regardless
    // of role, since it's what defines what "local" even means for the host.
    public static IReadOnlyDictionary<string, CardVariant> BuildLocalSnapshot()
    {
        return PropertiesByVanillaId.Keys.ToDictionary(id => id, ResolveLocal);
    }

    private static CardVariant ResolveLocal(string vanillaCardId)
    {
        if (!PropertiesByVanillaId.TryGetValue(vanillaCardId, out PropertyInfo? property))
        {
            return CardVariant.Rework;
        }
        object? value = property.GetValue(null);
        return value switch
        {
            CardVariant variant => variant,
            CardVariantNoOriginal noOriginal => noOriginal == CardVariantNoOriginal.Disabled ? CardVariant.Disabled : CardVariant.Rework,
            CardVariantOriginalOnly originalOnly => originalOnly == CardVariantOriginalOnly.Disabled ? CardVariant.Disabled : CardVariant.Original,
            _ => CardVariant.Rework
        };
    }

    // In multiplayer, every participant must agree on each card's effect and availability -- so a
    // client defers to the host's own resolution (MultiplayerConfigAuthority) instead of its own local
    // config. The host and any singleplayer game are always authoritative over themselves, so they
    // just resolve locally like before this existed.
    private static CardVariant Resolve(string? vanillaCardId, Player? contextPlayer = null)
    {
        if (vanillaCardId == null)
        {
            return CardVariant.Rework;
        }
        return MultiplayerConfigAuthority.TryGetHostVariant(vanillaCardId, out CardVariant hostVariant)
            ? hostVariant
            : ResolveLocal(vanillaCardId);
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
