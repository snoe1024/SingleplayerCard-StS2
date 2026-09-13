using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using SingleplayerCard.SingleplayerCardCode.Config;
using SingleplayerCard.SingleplayerCardCode.Multiplayer;

namespace SingleplayerCard.SingleplayerCardCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "SingleplayerCard"; //Used for resource filepath
    public const string ResPath = $"res://{ModId}";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static SingleplayerCardConfig Config { get; private set; } = null!;

    public static void Initialize()
    {
        Config = new SingleplayerCardConfig();
        Config.Load();
        ModConfigRegistry.Register(ModId, Config);

        // Lets a multiplayer host who tweaks a card's dropdown (or the ApplyInMultiplayer toggle
        // itself) while clients are already sitting in the lobby push the change out immediately,
        // rather than leaving them stuck with whatever was true at the moment they joined. See
        // MultiplayerConfigAuthority; a no-op unless we're currently hosting an open lobby.
        Config.ConfigChanged += (_, _) => MultiplayerConfigAuthority.NotifyLocalConfigChanged();

        // Must happen before BaseLib's PostModInitPatch.LatePostInit (a prefix on ModelDb.InitIds) --
        // the point of no return for a run's save shape, same deadline [SavedProperty] itself is bound
        // by. Registering here, at the top of this mod's own [ModInitializer], is the earliest and
        // therefore safest place. See ExtendedSaveRunConfigSnapshotStore for what this actually wires up.
        RunConfigSnapshot.Store = new ExtendedSaveRunConfigSnapshotStore();

        Harmony harmony = new(ModId);

        // Pass our own assembly explicitly rather than relying on the parameterless overload's
        // Assembly.GetCallingAssembly(): since BaseLib invokes this method via reflection
        // (MethodInfo.Invoke), that resolution can be unpredictable. Not confirmed as the cause of
        // any specific bug here, but it removes one variable when debugging why a patch didn't
        // apply -- see sts2_dev_knowledge/topics/harmony-patching.md for the actual bug that was
        // found (an attribute-placement mistake, not this).
        harmony.PatchAll(typeof(MainFile).Assembly);
    }
}