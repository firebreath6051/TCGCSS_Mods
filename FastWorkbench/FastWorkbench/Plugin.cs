using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;

namespace FastWorkbench
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<float> MaxPriceLimit { get; private set; }
        public static ConfigEntry<int> SpeedMultiplier { get; private set; }
        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            EnableMod = Config.Bind("Fast Workbench",
                                    "Enable Mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 1 }));

            MaxPriceLimit = Config.Bind("Fast Workbench",
                                         "Max price limit",
                                         1f,
                                         new ConfigDescription("Maximum price limit for workbench cards", new AcceptableValueRange<float>(1, 1000), new ConfigurationManagerAttributes { Order = 1 }));

            SpeedMultiplier = Config.Bind("Fast Workbench",
                                         "Speed multiplier",
                                         1,
                                         new ConfigDescription("Speed multiplier", new AcceptableValueRange<int>(1, 10), new ConfigurationManagerAttributes { Order = 1 }));

            MaxPriceLimit.SettingChanged += (_, _) =>
            {
                MaxPriceLimit.Value = (float)Math.Round(MaxPriceLimit.Value, 2, MidpointRounding.AwayFromZero);
            };
        }

        private void OnEnable()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            L($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is unloaded!");
        }

        public static int SpeedMultiplierValue
        {
            get { return EnableMod.Value ? SpeedMultiplier.Value : 1; }
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
