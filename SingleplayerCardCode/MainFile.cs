using BaseLib.Config;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using SingleplayerCard.SingleplayerCardCode.Config;

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