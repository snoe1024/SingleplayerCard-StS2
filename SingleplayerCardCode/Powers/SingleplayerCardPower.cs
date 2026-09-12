using BaseLib.Abstracts;
using BaseLib.Extensions;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using Godot;

namespace SingleplayerCard.SingleplayerCardCode.Powers;

public abstract class SingleplayerCardPower : CustomPowerModel
{
    //Loads from SingleplayerCard/images/powers/your_power.png
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}