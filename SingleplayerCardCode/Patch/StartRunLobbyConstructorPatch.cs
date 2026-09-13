using HarmonyLib;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode.Patch;

// StartRunLobby is the screen that exists before a run begins (see its own doc comment) and is the
// earliest point a NetGameService a client can register message handlers on / a host can send through
// actually exists -- well before CardPoolModel.GetUnlockedCards is ever queried for deck construction
// or rewards. Hooking its constructor lets MultiplayerConfigAuthority register its message handler and
// host-side "a player joined" callback from the moment the lobby exists, so the host's settings are
// always already known by the time any pool decision needs them.
//
// Only one of StartRunLobby's two public constructors needs patching: the 5-arg overload (used for
// Daily runs) delegates to this 4-arg one via `: this(...)`, so a Postfix here fires for both.
//
// IMPORTANT: [HarmonyPatch] must go on the CLASS, with [HarmonyPostfix] on the method inside -- see
// harmony-patching.md in sts2_dev_knowledge for why (this game's bundled Harmony build silently no-ops
// a [HarmonyPatch] placed directly on the method).
[HarmonyPatch(typeof(StartRunLobby), MethodType.Constructor, typeof(GameMode), typeof(INetGameService), typeof(IStartRunLobbyListener), typeof(int))]
public static class StartRunLobbyConstructorPatch
{
    [HarmonyPostfix]
    public static void Postfix(StartRunLobby __instance)
    {
        MultiplayerConfigAuthority.OnLobbyCreated(__instance);
    }
}
