using EchoCore.Scripts.Registry;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace EchoCore.Scripts.Init;

[ModInitializer("Init")]
public static class Entry
{
    private static Harmony? _harmony;

    public static void Init()
    {
        // 初始化时重建注册表，避免热重载或重复初始化留下脏数据。
        EchoRegistry.Clear();
        VanillaEchoBootstrap.RegisterAll();

        _harmony = new Harmony("sts2.echo.core");
        _harmony.PatchAll();

        Log.Info($"Echo Core initialized. echoes={EchoRegistry.Echoes.Count}, affixes={EchoRegistry.Affixes.Count}, sonatas={EchoRegistry.Sonatas.Count}");
    }
}
