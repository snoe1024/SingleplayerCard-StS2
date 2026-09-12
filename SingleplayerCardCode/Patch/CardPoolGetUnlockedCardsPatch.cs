using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using SingleplayerCard.SingleplayerCardCode.Cards;
using SingleplayerCard.SingleplayerCardCode.Config;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// Backs .claude/loadmap.md's "特定のマルチプレイヤーカードを出現しなくする": a card whose
// SingleplayerCardConfig dropdown is set to Disabled should not appear anywhere a pool is queried for
// what's obtainable (rewards, shops, etc.) or browsable (Card Library). CardPoolModel.GetUnlockedCards
// is the single choke point for both (see decompiled CardPoolModel.cs's own doc comment), so a Postfix
// here covers every case without needing to patch each screen/system separately.
//
// IMPORTANT: [HarmonyPatch] must go on the class, with [HarmonyPostfix] on the method inside -- see
// harmony-patching.md in sts2_dev_knowledge for why (this game's bundled Harmony build silently no-ops
// a [HarmonyPatch] placed directly on the method).
[HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
public static class CardPoolGetUnlockedCardsPatch
{
    [HarmonyPostfix]
    public static void Postfix(ref IEnumerable<CardModel> __result)
    {
        __result = __result.Where(card => card is not SingleplayerCardCard soloCard || !DuplicateReworkManager.IsDisabled(soloCard.VanillaCardIdForConfig));
    }
}
