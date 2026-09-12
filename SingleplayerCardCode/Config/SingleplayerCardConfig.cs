using BaseLib.Config;
using Godot;

namespace SingleplayerCard.SingleplayerCardCode.Config;

// The "Duplicate Rework Option" (DRO, see .claude/loadmap.md) toggle. When on, ported cards use
// their rebalanced singleplayer effect; when off, they fall back to the simpler "xDRO" effect noted
// per-card in loadmap.md. Cards should never read DuplicateReworkOption directly -- go through
// DuplicateReworkManager so a future per-card override or multiplayer host resolution only needs to
// change the manager, not every card.
public sealed class SingleplayerCardConfig : SimpleModConfig
{
    // ModConfig's property scanner only picks up static properties (CheckConfigProperties in the
    // decompiled BaseLib source requires propertyInfo.GetMethod.IsStatic, silently skipping instance
    // properties with just a warning log) -- an instance property here compiles fine but leaves
    // ConfigProperties empty, so HasSettings()/VisibleInModList() both stay false and
    // ModConfigRegistry.Register() silently declines to register the mod at all.
    public static bool DuplicateReworkOption { get; set; } = true;

    public override void SetupConfigUI(Control optionContainer)
    {
        GenerateOptionsForAllProperties(optionContainer);
    }
}
