using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FastPackOpening
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<bool> EnableMaxHoldPacks { get; private set; }
        internal static ConfigEntry<KeyboardShortcut> AutoOpenKey { get; private set; }
        internal static ConfigEntry<int> HighValueThreshold { get; private set; }
        internal static ConfigEntry<bool> StopAutoHighValue { get; private set; }
        internal static ConfigEntry<float> SpeedMultiplier { get; private set; }
        internal static ConfigEntry<float> PickupSpeedMultiplier { get; private set; }
        internal static ConfigEntry<int> MaxHoldPacks { get; private set; }
        internal static ConfigEntry<float> TextPositionX { get; private set; }
        internal static ConfigEntry<float> TextPositionY { get; private set; }
        internal static ConfigEntry<int> TextSize { get; private set; }
        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind("Config Options",
                                    "Enable mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 11 }));

            EnableMaxHoldPacks = Config.Bind("Config Options",
                                    "Enable max hold limit",
                                    true,
                                    new ConfigDescription("Enable holding more than 8 packs", null, new ConfigurationManagerAttributes { Order = 10 }));

            AutoOpenKey = Config.Bind("Config Options",
                                   "Auto open key",
                                   new KeyboardShortcut(KeyCode.BackQuote),
                                   new ConfigDescription("Key to toggle automatic opening of packs.", null, new ConfigurationManagerAttributes { Order = 9 }));

            StopAutoHighValue = Config.Bind("Config Options",
                                    "Stop auto open at high value cards",
                                    true,
                                    new ConfigDescription("Stops automatically opening packs when you get a high value card.", null, new ConfigurationManagerAttributes { Order = 8 }));

            HighValueThreshold = Config.Bind("Config Options",
                                    "High value threshold",
                                    10,
                                    new ConfigDescription("This value determines when a card is considered \"High Value\" and plays a special animation so you can see what card it is and its value. Default is 10.",
                                                            null, new ConfigurationManagerAttributes { Order = 7 }));

            SpeedMultiplier = Config.Bind("Config Options",
                                         "Speed multiplier",
                                         1f,
                                         new ConfigDescription("Speed multiplier", new AcceptableValueRange<float>(1, 10), new ConfigurationManagerAttributes { Order = 6 }));

            PickupSpeedMultiplier = Config.Bind("Config Options",
                                         "Pickup speed multiplier",
                                         1f,
                                         new ConfigDescription("Speed multiplier for card pack pick up and put down.", new AcceptableValueRange<float>(1, 10), new ConfigurationManagerAttributes { Order = 5 }));

            MaxHoldPacks = Config.Bind("Config Options",
                                         "Held pack limit",
                                         8,
                                         new ConfigDescription("How many packs you can hold in your hand.", new AcceptableValueRange<int>(1, 64), new ConfigurationManagerAttributes { Order = 4 }));

            TextPositionX = Config.Bind("Held Item Text",
                                         "Text position X",
                                         100f,
                                         new ConfigDescription("X coordinate for the held item counter.\nZero is bottom of the screen", new AcceptableValueRange<float>(0, Screen.currentResolution.width), new ConfigurationManagerAttributes { Order = 3 }));

            TextPositionY = Config.Bind("Held Item Text",
                                         "Text position Y",
                                         20f,
                                         new ConfigDescription("Y coordinate for the held item counter.\nZero is left side of the screen.", new AcceptableValueRange<float>(0, Screen.currentResolution.height), new ConfigurationManagerAttributes { Order = 2 }));

            TextSize = Config.Bind("Held Item Text",
                                         "Text size",
                                         20,
                                         new ConfigDescription("Text size for held item counter.", new AcceptableValueRange<int>(1, 32), new ConfigurationManagerAttributes { Order = 1 }));

            EnableMaxHoldPacks.SettingChanged += (_, _) =>
            {
                if (EnableMaxHoldPacksValue && CheckIfIncompatiblePluginsExist("EnableMaxHoldPacks", true))
                {
                    EnableMaxHoldPacks.Value = false;
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)), HarmonyPatchType.Prefix);
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf)), HarmonyPatchType.Transpiler);
                }
                if (EnableMaxHoldPacksValue && !CheckIfIncompatiblePluginsExist("EnableMaxHoldPacks"))
                {
                    Harmony.Patch(
                        AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)),
                        prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.InteractionPlayerController_OnGameDataFinishLoaded_Prefix))
                    );
                    Harmony.Patch(
                        AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf)),
                        transpiler: new HarmonyMethod(typeof(Patches), nameof(Patches.InteractionPlayerController_EvaluateTakeItemFromShelf_Transpiler))
                    );
                }
            };

            SpeedMultiplier.SettingChanged += (_, _) =>
            {
                SpeedMultiplier.Value = (float)Math.Round(SpeedMultiplier.Value, 1, MidpointRounding.AwayFromZero);
            };

            PickupSpeedMultiplier.SettingChanged += (_, _) =>
            {
                PickupSpeedMultiplier.Value = (float)Math.Round(PickupSpeedMultiplier.Value, 1, MidpointRounding.AwayFromZero);
            };

            TextPositionX.SettingChanged += (_, _) =>
            {
                TextPositionX.Value = (float)Math.Round(TextPositionX.Value, 2, MidpointRounding.AwayFromZero);
                Patches.holdItemCountText.transform.localPosition = new Vector3(TextPositionX.Value, Patches.holdItemCountText.transform.localPosition.y, Patches.holdItemCountText.transform.localPosition.z);
            };

            TextPositionY.SettingChanged += (_, _) =>
            {
                TextPositionY.Value = (float)Math.Round(TextPositionY.Value, 2, MidpointRounding.AwayFromZero);
                Patches.holdItemCountText.transform.localPosition = new Vector3(Patches.holdItemCountText.transform.localPosition.x, TextPositionY.Value, Patches.holdItemCountText.transform.localPosition.z);
            };

            TextSize.SettingChanged += (_, _) =>
            {
                Patches.holdItemCountText.fontSizeMax = TextSize.Value;
                Patches.holdItemCountText.fontSize = TextSize.Value;
            };
        }

        private void OnEnable()
        {
            Harmony.PatchAll();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is loaded! Made by WiseHorror (Nargacuga on Nexus)");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is unloaded!");
        }

        private void Update()
        {
            if (EnableMaxHoldPacksValue)
            {
                if (CheckIfIncompatiblePluginsExist("EnableMaxHoldPacks"))
                {
                    EnableMaxHoldPacks.Value = false;
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)), HarmonyPatchType.Prefix);
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf)), HarmonyPatchType.Transpiler);
                }
            }
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
        public static float SpeedMultiplierValue
        {
            get { return SpeedMultiplier.Value; }
        }
        public static float PickupSpeedMultiplierValue
        {
            get { return PickupSpeedMultiplier.Value; }
        }
        public static int MaxHoldPacksValue
        {
            get { return MaxHoldPacks.Value; }
        }
        public static bool EnableModValue
        {
            get { return EnableMod.Value; }
        }
        public static bool EnableMaxHoldPacksValue
        {
            get { return EnableMaxHoldPacks.Value; }
        }
        private static readonly Dictionary<string, string> IncompatiblePlugins = new Dictionary<string, string>
        {
            { "HandIsNotFull", "EnableMaxHoldPacks" }
        };

        private static List<string> LoadedPlugins = new List<string>();
        public bool CheckIfIncompatiblePluginsExist(string setting, bool printInfo = false)
        {
            LoadedPlugins.Clear();
            LoadedPlugins.AddRange(Chainloader.PluginInfos.Keys);
            foreach (string plugin in LoadedPlugins)
            {
                if (IncompatiblePlugins.Count > 0 && IncompatiblePlugins.ContainsKey(plugin))
                {
                    if (printInfo) Plugin.L($"Forcing {setting} to disabled due to incompatible plugin: {plugin}");
                    return true;
                }
            }
            return false;
        }
    }
}
