using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// Mirrors StartRunLobbyConstructorPatch's registration: CleanUp is StartRunLobby's own documented
// teardown point, called BOTH when the lobby is handing off to a run that's actually starting
// (disconnectSession: false -- the same NetService/connection carries over into the run, see
// RunManager.InitializeShared(lobby.NetService, ...) in the decompiled source) AND when the player
// genuinely leaves without starting one (disconnectSession: true). Only the latter should tear down
// MultiplayerConfigAuthority's registrations -- unregistering unconditionally would silently kill
// mid-run config sync the moment every run begins, which is the opposite of what's needed for a host's
// settings to actually be authoritative for the rest of a multiplayer session.
//
// IMPORTANT: [HarmonyPatch] must go on the CLASS, with [HarmonyPostfix] on the method inside -- see
// harmony-patching.md in sts2_dev_knowledge for why (this game's bundled Harmony build silently no-ops
// a [HarmonyPatch] placed directly on the method).
[HarmonyPatch(typeof(StartRunLobby), nameof(StartRunLobby.CleanUp))]
public static class StartRunLobbyCleanUpPatch
{
    [HarmonyPostfix]
    public static void Postfix(StartRunLobby __instance, bool disconnectSession)
    {
        MultiplayerConfigAuthority.OnLobbyCleanedUp(__instance, disconnectSession);
    }
}
