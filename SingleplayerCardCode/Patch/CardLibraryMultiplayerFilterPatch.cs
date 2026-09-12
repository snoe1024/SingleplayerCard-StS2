using System;
using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using SingleplayerCard.SingleplayerCardCode.Cards;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// Implements the ".claude/loadmap.md" "カードライブラリで表示を工夫する" note: the vanilla Card
// Library has a "View Multiplayer Cards" checkbox (NCardLibrary._viewMultiplayerCards) that, when
// ticked, additionally shows cards with MultiplayerConstraint == MultiplayerOnly (see
// NCardLibrary.UpdateFilter in the decompiled source). Our ported cards are the opposite: they
// should show when that checkbox is UNticked (mirroring "you're looking at singleplayer content
// only") and hide when it's ticked (mirroring "you're looking at the multiplayer card pool").
//
// Two patches are needed because the checkbox's ticked state and the actual filtering happen in
// different places, at different times, both inside NCardLibrary/NCardLibraryGrid:
// 1. NCardLibrary.UpdateFilter is called on every filter-affecting UI event (including the
//    checkbox toggle and the screen opening) and reads the live tickbox synchronously, in the same
//    call stack that shortly afterward reaches NCardLibraryGrid.FilterCards. A Prefix here caches
//    the ticked state into a static field before that happens.
// 2. NCardLibraryGrid.FilterCards(filter, sortingPriority) is the actual method that applies the
//    filter to build the visible card list. Its Prefix wraps the incoming filter so our cards are
//    additionally gated on the cached ticked state, leaving every other card's filtering untouched.
//
// A Postfix on UpdateFilter would run too late: by the time it returns, UpdateFilter has already
// synchronously called DisplayCards() -> FilterCards() with the stale filter (DisplayCards is an
// async Task method, but it runs synchronously up to its first await, and its call to FilterCards
// comes before any await), so the very click that changed the checkbox would render one step
// behind. Prefixing UpdateFilter avoids that entirely.
public static class CardLibraryMultiplayerFilterPatch
{
    private static bool _viewingMultiplayerCards = true;

    [HarmonyPatch(typeof(NCardLibrary), "UpdateFilter")]
    [HarmonyPrefix]
    public static void UpdateFilterPrefix(NCardLibrary __instance)
    {
        var tickbox = Traverse.Create(__instance).Field("_viewMultiplayerCards").GetValue<NTickbox>();
        if (tickbox != null)
        {
            _viewingMultiplayerCards = tickbox.IsTicked;
        }
    }

    [HarmonyPatch(typeof(NCardLibraryGrid), nameof(NCardLibraryGrid.FilterCards), new[] { typeof(Func<CardModel, bool>), typeof(List<SortingOrders>) })]
    [HarmonyPrefix]
    public static void FilterCardsPrefix(ref Func<CardModel, bool> filter)
    {
        Func<CardModel, bool> original = filter;
        filter = card => original(card) && (card is not SingleplayerCardCard || !_viewingMultiplayerCards);
    }
}
