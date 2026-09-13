namespace SingleplayerCard.SingleplayerCardCode.Multiplayer;

// In-memory-only IRunConfigSnapshotStore. Only used as RunConfigSnapshot.Store's default value before
// MainFile.Initialize() replaces it with ExtendedSaveRunConfigSnapshotStore (the real, save-file-backed
// implementation) -- kept around mainly so unit-test-style usage or any code that runs before mod init
// completes still has a harmless, functioning store rather than a null reference.
internal sealed class InMemoryRunConfigSnapshotStore : IRunConfigSnapshotStore
{
    private SingleplayerCardConfigSyncMessage? _snapshot;

    public void Save(SingleplayerCardConfigSyncMessage snapshot)
    {
        _snapshot = snapshot;
    }

    public SingleplayerCardConfigSyncMessage? Load()
    {
        return _snapshot;
    }

    public void Clear()
    {
        _snapshot = null;
    }
}

internal static class RunConfigSnapshot
{
    public static IRunConfigSnapshotStore Store { get; set; } = new InMemoryRunConfigSnapshotStore();
}
