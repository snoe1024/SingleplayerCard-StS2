namespace SingleplayerCard.SingleplayerCardCode.Multiplayer;

// Persists the ONE SingleplayerCardConfigSyncMessage snapshot that applies for a specific run's entire
// lifetime -- frozen once when that run begins (see MultiplayerConfigAuthority.EnsureFrozenForCurrentRun)
// and never re-derived from live SingleplayerCardConfig again for as long as that run exists, including
// across a save-and-quit/resume of that same run. Exists as an interface, rather than baking storage
// directly into MultiplayerConfigAuthority, so the resolution logic never needs to know how persistence
// actually works. ExtendedSaveRunConfigSnapshotStore (registered as RunConfigSnapshot.Store from
// MainFile.Initialize) is the real implementation, backed by
// BaseLib.Patches.Saves.ExtendedSaveHandlers&lt;IRunState, SerializableRun&gt; -- BaseLib's own
// general-purpose "attach extra data to a run's save file" primitive, the same plumbing [SavedProperty]
// itself is built on. InMemoryRunConfigSnapshotStore is only RunConfigSnapshot.Store's placeholder
// default before that registration runs.
internal interface IRunConfigSnapshotStore
{
    void Save(SingleplayerCardConfigSyncMessage snapshot);
    SingleplayerCardConfigSyncMessage? Load();
    void Clear();
}
