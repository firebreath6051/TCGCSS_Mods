using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace WorkerPersonalRestock
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }
        public static ConfigEntry<bool> EnableMod { get; private set; }
        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind("1. Config Options",
                                    "Enable mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 0 }));
        }

        private void OnEnable()
        {
            Harmony.PatchAll();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is unloaded!");
        }

        private void Update()
        {

        }

        internal static void L(string message, bool info = false)
        {
            if (info)
            {
                Log.LogInfo(message);
                return;
            }
            Log.LogWarning(message);
        }
    }
}
