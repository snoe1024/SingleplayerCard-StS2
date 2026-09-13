using System.Collections.Generic;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace SingleplayerCard.SingleplayerCardCode.Multiplayer;

// Carries one full snapshot of this mod's host-authoritative settings (see MultiplayerConfigAuthority)
// from the host to a client: whether "apply changes in multiplayer too" is on, and the resolved
// CardVariant for every ported card. Any type implementing INetMessage that a mod assembly defines is
// auto-discovered and assigned a deterministic type id shared by host and client (see
// MessageTypes.GetSubtypesInMods/ContentSorter in the decompiled source) -- no manual registration step
// is needed beyond implementing the interface, matching how the base game's own lobby messages (e.g.
// LobbyModifiersChangedMessage) work, which this mirrors.
internal struct SingleplayerCardConfigSyncMessage : INetMessage, IPacketSerializable
{
    public bool ApplyInMultiplayer;
    public List<CardVariantEntry> CardVariants;

    // Not echoed host->client->other-clients: only the host ever sends this (see
    // MultiplayerConfigAuthority), so there's nothing for a receiving client to rebroadcast.
    public bool ShouldBroadcast => false;

    public NetTransferMode Mode => NetTransferMode.Reliable;

    public LogLevel LogLevel => LogLevel.VeryDebug;

    public bool ShouldBuffer => true;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteBool(ApplyInMultiplayer);
        writer.WriteList(CardVariants);
    }

    public void Deserialize(PacketReader reader)
    {
        ApplyInMultiplayer = reader.ReadBool();
        CardVariants = reader.ReadList<CardVariantEntry>();
    }
}
