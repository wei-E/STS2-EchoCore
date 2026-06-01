using BaseLib.Config;
using EchoCore.Scripts.Config;
using EchoCore.Scripts.Registry;
using EchoCore.Scripts.Services;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace EchoCore.Scripts.Init;

[ModInitializer("Init")]
public static class Entry
{
    private static Harmony? _harmony;

    public static void Init()
    {
        // 初始化时重建注册表，避免热重载或重复初始化留下脏数据。
        EchoRegistry.Clear();
        EchoContentBootstrap.RegisterAll();
        ModConfigRegistry.Register("EchoCore", new EchoDeveloperConfig());

        // 自定义 modifier 的 SavedProperty 必须进入 STS2 的保存字段缓存，
        // 否则存档里只会写入 modifier id，不会写入 props。
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(EchoRunStateModifier));

        _harmony = new Harmony("sts2.echo.core");
        _harmony.PatchAll();

        Log.Info($"Echo Core initialized. echoes={EchoRegistry.Echoes.Count}, affixes={EchoRegistry.Affixes.Count}, sonatas={EchoRegistry.Sonatas.Count}");
    }
}
