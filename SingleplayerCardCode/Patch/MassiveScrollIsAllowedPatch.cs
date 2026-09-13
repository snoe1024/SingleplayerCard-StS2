using HarmonyLib;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Runs;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// MassiveScroll ("Giant Scroll") grants a random vanilla MultiplayerOnly card. When
// MultiplayerConfigAuthority.EffectiveApplyInMultiplayer is on, CardPoolGetUnlockedCardsPatch strips
// every vanilla MultiplayerOnly card out of card pools -- but MassiveScroll.AfterObtained's
// CardCreationOptions.CardPoolFilter demands MultiplayerConstraint == MultiplayerOnly specifically, so
// it never accepts this mod's SingleplayerOnly-flagged replacements either. The result is an empty
// candidate list, which CardFactory can't roll a rarity for -- soft-locking whatever screen tried to
// grant the relic's card (confirmed via a real playtest: picking Giant Scroll at Neow left the run
// stuck). Vanilla's own RelicModel.IsAllowed(IRunState) is the exact mechanism that already gates
// MassiveScroll's presence at Neow (Neow.GenerateInitialOptions removes any relic option whose
// IsAllowedAtNeow -> IsAllowed returns false) and is also reused by RelicGrabBag elsewhere, so a
// Postfix here removes Giant Scroll from every reward source that could soft-lock this way, not just
// Neow specifically, with no need to touch Neow's own (private) option-building logic.
//
// IMPORTANT: [HarmonyPatch] must go on the class, with [HarmonyPostfix] on the method inside -- see
// harmony-patching.md in sts2_dev_knowledge for why (this game's bundled Harmony build silently no-ops
// a [HarmonyPatch] placed directly on the method).
[HarmonyPatch(typeof(MassiveScroll), nameof(MassiveScroll.IsAllowed))]
public static class MassiveScrollIsAllowedPatch
{
    [HarmonyPostfix]
    public static void Postfix(IRunState runState, ref bool __result)
    {
        if (MultiplayerConfigAuthority.EffectiveApplyInMultiplayer)
        {
            __result = false;
        }
    }
}
