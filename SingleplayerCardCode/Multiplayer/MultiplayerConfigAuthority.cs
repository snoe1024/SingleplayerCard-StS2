using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Runs;
using SingleplayerCard.SingleplayerCardCode.Config;

namespace SingleplayerCard.SingleplayerCardCode.Multiplayer;

// Everyone in a multiplayer session must agree on this mod's settings for the WHOLE lifetime of a run --
// not just at the moment it starts. If the ruleset could still change mid-run, host and client could
// independently compute different candidate pools for the same reward/shop screen (each machine builds
// its own pool locally rather than the host literally shipping it over the wire), which is a genuine
// desync risk, not just a cosmetic staleness. The game's own pause-menu Settings screen is reachable at
// any time, including mid-combat (confirmed via NPauseMenu -> NSettingsScreen), so "mid-run" is a real
// scenario, not a hypothetical.
//
// The fix is the same principle SingleplayerCardCard.DroWasOnAtCreation already uses per-card, applied
// at the run level instead: once a run begins, this mod's effective settings for THAT run are frozen
// (EnsureFrozenForCurrentRun) and never re-derived from live SingleplayerCardConfig again, for host and
// client alike -- including across a save-and-quit/resume of that same run, via RunConfigSnapshot.Store
// (see its own doc comment for what's still a placeholder there). Before a run begins (still in the
// lobby picking characters), resolution instead reflects whatever the host has broadcast so far, so
// players still see accurate hover text/pool previews while deciding.
//
// DuplicateReworkManager.Resolve and CardPoolGetUnlockedCardsPatch both go through here instead of
// reading SingleplayerCardConfig directly, so neither needs its own multiplayer-awareness.
internal static class MultiplayerConfigAuthority
{
    // TEMPORARY diagnostic logging (2026-09-14): added to track down a real playtest where a client
    // with ApplyInMultiplayer enabled locally never saw Solo cards substituted into a multiplayer
    // run's pool, despite that being explainable via BuildLocalSnapshot() alone even if host sync
    // failed outright. SingleplayerCardConfigSyncMessage never appeared in that session's log at all
    // (send or receive), which these lines would have caught directly instead of requiring static code
    // analysis after the fact. Remove once the root cause is confirmed and fixed.
    private const string DiagTag = "[SingleplayerCard][MultiplayerConfigAuthority]";

    private static StartRunLobby? _activeLobby;
    private static SingleplayerCardConfigSyncMessage? _hostSnapshot;

    // RunManager.Instance always exists, but its NetService is only assigned once a run actually begins
    // (RunManager.InitializeShared) -- null here means "no active run" (main menu, Card Library
    // browsing from outside a run, still in the pre-run lobby, etc).
    private static bool IsClient => RunManager.Instance.NetService?.Type == NetGameType.Client;
    private static bool IsActiveMultiplayerRun => RunManager.Instance.NetService?.Type.IsMultiplayer() ?? false;

    public static bool EffectiveApplyInMultiplayer => ResolveSnapshot().ApplyInMultiplayer;

    // False means "resolve locally as usual" (singleplayer, or the host itself before any run-level
    // freeze exists yet). True always produces a variant (Rework if nothing else says otherwise), since
    // silently falling through to THIS machine's own local dropdown value once we've decided host
    // authority applies would defeat the entire point of it.
    public static bool TryGetHostVariant(string vanillaCardId, out CardVariant variant)
    {
        if (!IsActiveMultiplayerRun && !IsClient)
        {
            variant = default;
            return false;
        }
        variant = CardVariant.Rework;
        foreach (CardVariantEntry entry in ResolveSnapshot().CardVariants)
        {
            if (entry.VanillaCardId == vanillaCardId)
            {
                variant = (CardVariant)entry.Variant;
                break;
            }
        }
        return true;
    }

    // Once a run's snapshot is frozen, it is THE answer for the rest of that run -- for the host too, so
    // a host tweaking settings mid-run can never cause host and client to disagree. Before that (still
    // in the lobby), a client answers with whatever the host has broadcast so far and the host/
    // singleplayer just answers with its own current local config, matching pre-freeze behavior.
    private static SingleplayerCardConfigSyncMessage ResolveSnapshot()
    {
        if (IsActiveMultiplayerRun && RunConfigSnapshot.Store.Load() is { } frozen)
        {
            return frozen;
        }
        return IsClient ? _hostSnapshot ?? BuildLocalSnapshot() : BuildLocalSnapshot();
    }

    // Called via a Harmony postfix on RunManager.InitializeShared (see MultiplayerRunFreezePatch),
    // which every run-start path -- brand new run AND resuming a saved one -- funnels through, right
    // after NetService is assigned. Only actually freezes when nothing is stored yet: a resumed run
    // should keep whatever was true when IT began, never re-freeze from this machine's current live
    // config (that's the entire point -- see RunConfigSnapshot's doc comment for the one known gap in
    // today's placeholder storage).
    public static void EnsureFrozenForCurrentRun()
    {
        bool alreadyFrozen = RunConfigSnapshot.Store.Load() != null;
        Log.Info($"{DiagTag} EnsureFrozenForCurrentRun: IsActiveMultiplayerRun={IsActiveMultiplayerRun}, IsClient={IsClient}, alreadyFrozen={alreadyFrozen}");
        if (IsActiveMultiplayerRun && !alreadyFrozen)
        {
            SingleplayerCardConfigSyncMessage snapshot = ResolveSnapshot();
            RunConfigSnapshot.Store.Save(snapshot);
            Log.Info($"{DiagTag} EnsureFrozenForCurrentRun: froze ApplyInMultiplayer={snapshot.ApplyInMultiplayer}, {snapshot.CardVariants.Count} card variant(s), source={(IsClient ? (_hostSnapshot != null ? "hostSnapshot" : "localFallback") : "localSnapshot(host/singleplayer)")}");
        }
    }

    public static void OnLobbyCreated(StartRunLobby lobby)
    {
        Log.Info($"{DiagTag} OnLobbyCreated: NetService.Type={lobby.NetService.Type}");
        _activeLobby = lobby;
        _hostSnapshot = null;
        // A brand new lobby means whatever was frozen for the PREVIOUS run (if any, within this same
        // process -- see RunConfigSnapshot's own doc comment) is no longer relevant: if this lobby leads
        // to a genuinely new run, EnsureFrozenForCurrentRun must freeze fresh from current live config,
        // not silently reuse a finished run's stale values just because nothing happened to overwrite
        // the in-memory field yet. A resumed run instead repopulates this from its own save data (via
        // ExtendedSaveRunConfigSnapshotStore's registered setter) before EnsureFrozenForCurrentRun ever
        // runs, so clearing here doesn't affect that path.
        RunConfigSnapshot.Store.Clear();
        lobby.NetService.RegisterMessageHandler<SingleplayerCardConfigSyncMessage>(HandleConfigSyncMessage);
        lobby.PlayerConnected += OnPlayerConnected;
    }

    // disconnectSession mirrors StartRunLobby.CleanUp's own parameter: false means the lobby is simply
    // handing off to the run that's about to begin (same NetService instance carried over via
    // RunManager.InitializeShared(lobby.NetService, ...) -- see decompiled RunManager.cs), so our
    // handler must stay registered for the run to keep receiving updates while still connected. Only a
    // real disconnect (leaving the lobby without starting, the run ending, or the connection dropping)
    // should tear things down and clear stale state for whatever comes next.
    public static void OnLobbyCleanedUp(StartRunLobby lobby, bool disconnectSession)
    {
        Log.Info($"{DiagTag} OnLobbyCleanedUp: disconnectSession={disconnectSession}");
        if (!disconnectSession)
        {
            return;
        }
        lobby.NetService.UnregisterMessageHandler<SingleplayerCardConfigSyncMessage>(HandleConfigSyncMessage);
        lobby.PlayerConnected -= OnPlayerConnected;
        if (_activeLobby == lobby)
        {
            _activeLobby = null;
            _hostSnapshot = null;
            RunConfigSnapshot.Store.Clear();
        }
    }

    // Called from MainFile via SingleplayerCardConfig's BaseLib-provided ConfigChanged event. Only
    // reaches other players while still in the pre-run lobby or mid-run-but-not-yet-frozen for some
    // reason; once EnsureFrozenForCurrentRun has run for this run, ResolveSnapshot ignores this
    // broadcast for the rest of it by construction -- so a host who opens Settings mid-run and tweaks a
    // dropdown harmlessly sends an update that has no effect on the CURRENT run, matching "changes take
    // effect on your next run" rather than silently doing nothing at all (which would look like the
    // toggle didn't register).
    public static void NotifyLocalConfigChanged()
    {
        Log.Info($"{DiagTag} NotifyLocalConfigChanged: activeLobby={_activeLobby != null}, isHost={_activeLobby?.NetService.Type == NetGameType.Host}");
        if (_activeLobby != null && _activeLobby.NetService.Type == NetGameType.Host)
        {
            SingleplayerCardConfigSyncMessage snapshot = BuildLocalSnapshot();
            Log.Info($"{DiagTag} NotifyLocalConfigChanged: broadcasting ApplyInMultiplayer={snapshot.ApplyInMultiplayer}, {snapshot.CardVariants.Count} card variant(s)");
            _activeLobby.NetService.SendMessage(snapshot);
        }
    }

    private static void OnPlayerConnected(StartRunLobbyPlayer player)
    {
        bool isHost = _activeLobby?.NetService.Type == NetGameType.Host;
        bool isSelf = _activeLobby != null && player.id == _activeLobby.NetService.NetId;
        Log.Info($"{DiagTag} OnPlayerConnected: player={player.id}, isHost={isHost}, isSelf={isSelf}");
        if (_activeLobby == null || !isHost || isSelf)
        {
            return;
        }
        SingleplayerCardConfigSyncMessage snapshot = BuildLocalSnapshot();
        Log.Info($"{DiagTag} OnPlayerConnected: sending ApplyInMultiplayer={snapshot.ApplyInMultiplayer}, {snapshot.CardVariants.Count} card variant(s) to player {player.id}");
        _activeLobby.NetService.SendMessage(snapshot, player.id);
    }

    private static void HandleConfigSyncMessage(SingleplayerCardConfigSyncMessage message, ulong senderId)
    {
        Log.Info($"{DiagTag} HandleConfigSyncMessage: received from {senderId}, ApplyInMultiplayer={message.ApplyInMultiplayer}, {message.CardVariants.Count} card variant(s)");
        _hostSnapshot = message;
    }

    private static SingleplayerCardConfigSyncMessage BuildLocalSnapshot()
    {
        List<CardVariantEntry> entries = DuplicateReworkManager.BuildLocalSnapshot()
            .Select(pair => new CardVariantEntry { VanillaCardId = pair.Key, Variant = (byte)pair.Value })
            .ToList();
        return new SingleplayerCardConfigSyncMessage
        {
            ApplyInMultiplayer = SingleplayerCardConfig.ApplyInMultiplayer,
            CardVariants = entries
        };
    }
}
