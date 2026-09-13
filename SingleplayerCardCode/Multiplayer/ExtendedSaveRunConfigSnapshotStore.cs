using System;
using System.Collections.Generic;
using BaseLib.Patches.Saves;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace SingleplayerCard.SingleplayerCardCode.Multiplayer;

// Real IRunConfigSnapshotStore backing. BaseLib.Patches.Saves.ExtendedSaveHandlers<IRunState,
// SerializableRun> is BaseLib's own general-purpose "attach extra data to a run's save file" primitive --
// the exact same plumbing [SavedProperty] itself is built on for CardModel/RelicModel/PotionModel/
// Reward/Player (see BaseLib's PostModInitPatch.LatePostInit and SavePatchUtils.IsHolderTypeBaseSupported),
// just not exposed as an attribute for IRunState specifically, since a mod can't subclass it the way it
// subclasses CardModel. RegisterSave hooks RunManager.ToSave / RunState.FromSerializable /
// SerializableRun.Serialize+Deserialize, so whatever this store holds at save time is written into that
// run's own save-file JSON (and the binary path used for netcode/canonicalization) and read back
// identically at load time -- including across a full process relaunch, closing the one gap
// InMemoryRunConfigSnapshotStore's own doc comment called out.
//
// IMPORTANT, found the hard way during an actual playtest: the JSON side of RegisterSave<T> requires T
// to have JsonTypeInfo metadata available to the game's System.Text.Json setup, which runs in
// source-generation-only mode (no reflection fallback -- confirmed by "Failed to save run:
// System.NotSupportedException: JsonTypeInfo metadata... was not provided" appearing on EVERY autosave
// once SingleplayerCardConfigSyncMessage itself was registered directly as T). Only .NET's small set of
// built-in primitive types (bool, byte, string, etc. -- the same category DroWasOnAtCreation's plain
// bool already relies on via [SavedProperty]) have that metadata without a mod supplying its own
// JsonSerializerContext. So T here is `string`, via the 5-arg RegisterSave overload (no
// IPacketSerializable constraint, unlike the convenience 3-arg one) -- Encode/Decode reuse
// SingleplayerCardConfigSyncMessage's EXISTING PacketWriter/PacketReader (de)serialization (the same
// code path used for the live netcode message) via a Base64 string, rather than duplicating that logic
// in a JSON-friendly shape. serializer/deserializer below are for the SEPARATE binary
// packet path (netcode replication/canonicalization of the run state itself), where a plain string is
// trivially IPacketSerializable-compatible via WriteString/ReadString.
internal sealed class ExtendedSaveRunConfigSnapshotStore : IRunConfigSnapshotStore
{
    private const string SaveId = "SingleplayerCard.RunConfigSnapshot";

    private SingleplayerCardConfigSyncMessage? _current;

    public ExtendedSaveRunConfigSnapshotStore()
    {
        ExtendedSaveHandlers<IRunState, SerializableRun>.RegisterSave<string>(
            SaveId,
            _ => Encode(_current ?? EmptySnapshot),
            (_, encoded) => _current = string.IsNullOrEmpty(encoded) ? null : Decode(encoded),
            (str, writer) => writer.WriteString(str),
            reader => reader.ReadString());
    }

    private static SingleplayerCardConfigSyncMessage EmptySnapshot => new()
    {
        ApplyInMultiplayer = false,
        CardVariants = new List<CardVariantEntry>()
    };

    private static string Encode(SingleplayerCardConfigSyncMessage snapshot)
    {
        PacketWriter writer = new();
        snapshot.Serialize(writer);
        writer.ZeroByteRemainder();
        byte[] bytes = new byte[writer.BytePosition];
        Array.Copy(writer.Buffer, bytes, bytes.Length);
        return Convert.ToBase64String(bytes);
    }

    private static SingleplayerCardConfigSyncMessage Decode(string encoded)
    {
        PacketReader reader = new();
        reader.Reset(Convert.FromBase64String(encoded));
        SingleplayerCardConfigSyncMessage snapshot = new();
        snapshot.Deserialize(reader);
        return snapshot;
    }

    public void Save(SingleplayerCardConfigSyncMessage snapshot)
    {
        _current = snapshot;
    }

    public SingleplayerCardConfigSyncMessage? Load()
    {
        return _current;
    }

    public void Clear()
    {
        _current = null;
    }
}
