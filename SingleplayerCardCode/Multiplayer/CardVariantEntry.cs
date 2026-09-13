using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using SingleplayerCard.SingleplayerCardCode.Config;

namespace SingleplayerCard.SingleplayerCardCode.Multiplayer;

// One card's resolved CardVariant, keyed by vanilla card id -- the element type carried by
// SingleplayerCardConfigSyncMessage.CardVariants. A separate IPacketSerializable struct (rather than a
// raw Dictionary<string, CardVariant>) only exists because PacketWriter.WriteList/PacketReader.ReadList
// require an IPacketSerializable element type with a public parameterless constructor; there's nothing
// else this type needs to do. Variant is stored as a raw byte (not the CardVariant enum directly)
// because PacketWriter.WriteEnum<T> requires T's underlying type be assignable to int and uses a
// variable bit-width scheme meant for large/sparse enums -- overkill for a 3-value enum where writing
// it as a single byte is both simpler and already minimal.
internal struct CardVariantEntry : IPacketSerializable
{
    public string VanillaCardId;
    public byte Variant;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(VanillaCardId);
        writer.WriteByte(Variant);
    }

    public void Deserialize(PacketReader reader)
    {
        VanillaCardId = reader.ReadString();
        Variant = reader.ReadByte();
    }
}
