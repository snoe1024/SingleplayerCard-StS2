using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// RunManager.InitializeShared is the one place every run-start path funnels through -- a brand new run,
// AND resuming a previously saved one (see its call sites in the decompiled source) -- and it assigns
// RunManager.Instance.NetService as its very first action, so a Postfix here is the earliest point that
// property is reliably non-null for the run that's actually beginning. See
// MultiplayerConfigAuthority.EnsureFrozenForCurrentRun for why freezing here (rather than, say, inside
// StartRunLobby before the run object even exists) matters: it must run for a resumed run too, but only
// takes effect there if nothing was already frozen for it.
//
// IMPORTANT: [HarmonyPatch] must go on the CLASS, with [HarmonyPostfix] on the method inside -- see
// harmony-patching.md in sts2_dev_knowledge for why (this game's bundled Harmony build silently no-ops
// a [HarmonyPatch] placed directly on the method).
[HarmonyPatch(typeof(RunManager), "InitializeShared")]
public static class RunManagerInitializeSharedPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        MultiplayerConfigAuthority.EnsureFrozenForCurrentRun();
    }
}
