using MegaCrit.Sts2.Core.Localization;

namespace GamblerMod.GamblerModCode.Patch;

public interface IDynamicCardTitleLocString
{
    public LocString GetDynamicTitleLocString(LocString original);
}

public interface IDynamicCardTitle
{
    public string GetDynamicTitle(string original);
}

public interface IDynamicCardDescription
{
    public LocString GetDynamicDescription(LocString original);
}