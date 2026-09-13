using System.Reflection;

// RitsuLib (com.ritsukage.sts2-RitsuLib, a widely-used compat/utility mod many players have installed)
// automatically mirrors every BaseLib-registered ModConfig -- including this mod's -- into its OWN
// settings UI via reflection over BaseLib.Config.ModConfigRegistry, with no opt-in needed on our side.
// This defeats MultiplayerConfigAuthority's run-freeze design: BaseLib's own settings screen already
// correctly blocks navigating to mod configs while a run is in progress (confirmed via a real
// playtest), but RitsuLib's mirrored copy of the SAME settings was reachable at any time, including
// mid-combat -- and separately renders hover tooltips incorrectly. RitsuLib's mirroring code
// (STS2RitsuLib.Settings.ModSettingsMirrorInteropPolicy.ShouldMirror, decompiled) specifically checks
// for AssemblyMetadataAttribute entries named "RitsuLib.ModSettingsMirror.Mod.<modId>.DisableSources"
// on any loaded assembly before mirroring a given mod's config -- this is that opt-out, scoped to this
// mod's own ModId ("SingleplayerCard", see MainFile.ModId). With this in place, BaseLib's own settings
// screen (and its existing gameplay-in-progress lock) is the only way to reach this mod's settings.
[assembly: AssemblyMetadata("RitsuLib.ModSettingsMirror.Mod.SingleplayerCard.DisableSources", "baselib")]
