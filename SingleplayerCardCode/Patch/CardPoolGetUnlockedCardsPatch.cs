using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Unlocks;
using SingleplayerCard.SingleplayerCardCode.Cards;
using SingleplayerCard.SingleplayerCardCode.Config;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// Backs .claude/loadmap.md's "特定のマルチプレイヤーカードを出現しなくする" AND the multiplayer
// "ApplyInMultiplayer" feature: CardPoolModel.GetUnlockedCards is the single choke point for both what's
// obtainable (rewards, shops, etc.) and browsable (Card Library) -- see decompiled CardPoolModel.cs's
// own doc comment -- so a Postfix here covers every case without needing to patch each screen/system
// separately.
//
// IMPORTANT: [HarmonyPatch] must go on the class, with [HarmonyPostfix] on the method inside -- see
// harmony-patching.md in sts2_dev_knowledge for why (this game's bundled Harmony build silently no-ops
// a [HarmonyPatch] placed directly on the method).
[HarmonyPatch(typeof(CardPoolModel), nameof(CardPoolModel.GetUnlockedCards))]
public static class CardPoolGetUnlockedCardsPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardPoolModel __instance, UnlockState unlockState, CardMultiplayerConstraint multiplayerConstraint, ref IEnumerable<CardModel> __result)
    {
        List<CardModel> list = __result.ToList();

        if (multiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly)
        {
            bool effectiveApplyInMultiplayer = MultiplayerConfigAuthority.EffectiveApplyInMultiplayer;
            if (effectiveApplyInMultiplayer)
            {
                HashSet<string> portedVanillaIds = DuplicateReworkManager.AllPortedVanillaCardIds.ToHashSet();
                int removed = list.RemoveAll(card => card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly && portedVanillaIds.Contains(card.Id.Entry));

                int added = 0;
                foreach (CardModel card in __instance.GetUnlockedCards(unlockState, CardMultiplayerConstraint.None))
                {
                    if (card is SingleplayerCardCard && !list.Contains(card))
                    {
                        list.Add(card);
                        added++;
                    }
                }

                Log.Info($"[SingleplayerCard][CardPoolGetUnlockedCardsPatch] MultiplayerOnly query result: removed={removed}, added={added}, afterCount={list.Count}");
            }
        }

        list.RemoveAll(card => card is SingleplayerCardCard soloCard && DuplicateReworkManager.IsDisabled(soloCard.VanillaCardIdForConfig));

        __result = list;
    }
}
