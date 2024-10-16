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
        public static ConfigEntry<float> MinimumPriceLimit { get; private set; }
        public static ConfigEntry<float> MaxPriceLimit { get; private set; }
        public static ConfigEntry<float> SpeedMultiplier { get; private set; }
        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind("Fast Workbench",
                                    "Enable Mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 4 }));

            MinimumPriceLimit = Config.Bind("Fast Workbench",
                                         "Minimum price limit",
                                         0.01f,
                                         new ConfigDescription("Minimum price limit for workbench cards", new AcceptableValueRange<float>(0.01f, 20000f), new ConfigurationManagerAttributes { Order = 3 }));

            MaxPriceLimit = Config.Bind("Fast Workbench",
                                         "Max price limit",
                                         5f,
                                         new ConfigDescription("Maximum price limit for workbench cards", new AcceptableValueRange<float>(0.01f, 20000f), new ConfigurationManagerAttributes { Order = 2 }));

            SpeedMultiplier = Config.Bind("Fast Workbench",
                                         "Speed multiplier",
                                         1f,
                                         new ConfigDescription("Speed multiplier", new AcceptableValueRange<float>(1, 10), new ConfigurationManagerAttributes { Order = 1 }));

            MaxPriceLimit.SettingChanged += (_, _) =>
            {
                MaxPriceLimit.Value = (float)Math.Round(MaxPriceLimit.Value, 2, MidpointRounding.AwayFromZero);
            };

            MinimumPriceLimit.SettingChanged += (_, _) =>
            {
                MinimumPriceLimit.Value = (float)Math.Round(MinimumPriceLimit.Value, 2, MidpointRounding.AwayFromZero);
            };

            SpeedMultiplier.SettingChanged += (_, _) =>
            {
                SpeedMultiplier.Value = (float)Math.Round(SpeedMultiplier.Value, 1, MidpointRounding.AwayFromZero);
            };
        }

        private void OnEnable()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            L($"Plugin {PluginInfo.PLUGIN_NAME} is loaded! Made by WiseHorror (Nargacuga on Nexus)");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is unloaded!");
        }

        public static float SpeedMultiplierValue
        {
            get { return EnableMod.Value ? SpeedMultiplier.Value : 1f; }
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
