using BaseLib.Abstracts;
using BaseLib.Extensions;
using SingleplayerCard.SingleplayerCardCode.Extensions;
using Godot;

namespace SingleplayerCard.SingleplayerCardCode.Relics;

public abstract class SingleplayerCardRelic : CustomRelicModel
{
    //SingleplayerCard/images/relics
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();

    protected override string PackedIconOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();

    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}