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
        public static bool IsPackPositionsLoaded { get; set; }
        public static bool IsPackPositionsReordered { get; set; }
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<bool> SkipPackEndScreen { get; private set; }
        public static ConfigEntry<float> PackResultsTimer { get; private set; }
        internal static ConfigEntry<KeyboardShortcut> AutoOpenKey { get; private set; }
        internal static ConfigEntry<int> HighValueThreshold { get; private set; }
        internal static ConfigEntry<bool> StopAutoHighValue { get; private set; }
        internal static ConfigEntry<float> SpeedMultiplier { get; private set; }
        internal static ConfigEntry<float> PickupSpeedMultiplier { get; private set; }
        internal static ConfigEntry<float> TextPositionX { get; private set; }
        internal static ConfigEntry<float> TextPositionY { get; private set; }
        internal static ConfigEntry<int> TextSize { get; private set; }
        public static ConfigEntry<bool> EnableMaxHoldPacks { get; private set; }
        public static ConfigEntry<bool> EnableHeldItemPositions { get; private set; }
        internal static ConfigEntry<int> MaxHoldPacks { get; private set; }
        private void Awake()
        {
            Log = Logger;

            // Config Options

            EnableMod = Config.Bind("1. Config Options",
                                    "Enable mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 8 }));

            AutoOpenKey = Config.Bind("1. Config Options",
                                   "Auto open toggle on/off",
                                   new KeyboardShortcut(KeyCode.BackQuote),
                                   new ConfigDescription("Key to toggle automatic opening of packs.", null, new ConfigurationManagerAttributes { Order = 7 }));

            StopAutoHighValue = Config.Bind("1. Config Options",
                                    "Stop auto open at high value cards",
                                    true,
                                    new ConfigDescription("Stops automatically opening packs when you get a high value card.", null, new ConfigurationManagerAttributes { Order = 6 }));

            SkipPackEndScreen = Config.Bind("1. Config Options",
                                    "Speed up pack results screen",
                                    false,
                                    new ConfigDescription("Speeds up the animation at the end of opening a pack.", null, new ConfigurationManagerAttributes { Order = 5 }));

            PackResultsTimer = Config.Bind("1. Config Options",
                                    "Pack results screen timer",
                                    1f,
                                    new ConfigDescription("Amount of time the pack results screen is displayed before auto open proceeds.", new AcceptableValueRange<float>(0, 10), new ConfigurationManagerAttributes { Order = 4 }));

            HighValueThreshold = Config.Bind("1. Config Options",
                                    "High value threshold",
                                    10,
                                    new ConfigDescription("This value determines when a card is considered \"High Value\" and plays a special animation so you can see what card it is and its value. Default is 10.",
                                                            new AcceptableValueRange<int>(1, 10000), new ConfigurationManagerAttributes { Order = 3 }));

            SpeedMultiplier = Config.Bind("1. Config Options",
                                         "Speed multiplier",
                                         1f,
                                         new ConfigDescription("Speed multiplier", new AcceptableValueRange<float>(1, 100), new ConfigurationManagerAttributes { Order = 2 }));

            PickupSpeedMultiplier = Config.Bind("1. Config Options",
                                         "Pickup speed multiplier",
                                         1f,
                                         new ConfigDescription("Speed multiplier for card pack pick up and put down.", new AcceptableValueRange<float>(1, 100), new ConfigurationManagerAttributes { Order = 1 }));

            // Held Pack Options

            EnableMaxHoldPacks = Config.Bind("2. Held Pack Options",
                                    "Enable max hold limit",
                                    true,
                                    new ConfigDescription("Enable holding more than 8 packs", null, new ConfigurationManagerAttributes { Order = 6 }));

            MaxHoldPacks = Config.Bind("2. Held Pack Options",
                                         "Held pack limit",
                                         8,
                                         new ConfigDescription("How many packs you can hold in your hand.", new AcceptableValueRange<int>(1, 1024), new ConfigurationManagerAttributes { Order = 5 }));

            EnableHeldItemPositions = Config.Bind("2. Held Pack Options",
                                    "Enable hold item repositioning",
                                    false,
                                    new ConfigDescription("Makes held packs stack in several rows in your hands, instead of a single line.", null, new ConfigurationManagerAttributes { Order = 4 }));

            TextPositionX = Config.Bind("2. Held Pack Options",
                                         "Text position X",
                                         100f,
                                         new ConfigDescription("X coordinate for the held item counter.\nZero is bottom of the screen", new AcceptableValueRange<float>(0, Screen.currentResolution.width), new ConfigurationManagerAttributes { Order = 3 }));

            TextPositionY = Config.Bind("2. Held Pack Options",
                                         "Text position Y",
                                         20f,
                                         new ConfigDescription("Y coordinate for the held item counter.\nZero is left side of the screen.", new AcceptableValueRange<float>(0, Screen.currentResolution.height), new ConfigurationManagerAttributes { Order = 2 }));

            TextSize = Config.Bind("2. Held Pack Options",
                                         "Text size",
                                         20,
                                         new ConfigDescription("Text size for held item counter.", new AcceptableValueRange<int>(1, 32), new ConfigurationManagerAttributes { Order = 1 }));

            EnableMaxHoldPacks.SettingChanged += (_, _) =>
            {
                if (EnableMaxHoldPacksValue && CheckIfIncompatiblePluginsExist("EnableMaxHoldPacks", true))
                {
                    /*EnableMaxHoldPacks.Value = false;
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)), HarmonyPatchType.Prefix);
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf)), HarmonyPatchType.Transpiler);*/
                }
                if (EnableMaxHoldPacksValue && !CheckIfIncompatiblePluginsExist("EnableMaxHoldPacks"))
                {
                    /*Harmony.Patch(
                        AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)),
                        prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.InteractionPlayerController_OnGameDataFinishLoaded_Prefix))
                    );
                    Harmony.Patch(
                        AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf)),
                        transpiler: new HarmonyMethod(typeof(Patches), nameof(Patches.InteractionPlayerController_EvaluateTakeItemFromShelf_Transpiler))
                    );*/
                }
                if (!EnableMaxHoldPacks.Value && EnableHeldItemPositions.Value)
                {
                    EnableHeldItemPositions.Value = false;
                }
            };

            EnableMod.SettingChanged += (_, _) =>
            {
                if (!EnableMod.Value && EnableHeldItemPositions.Value)
                {
                    EnableHeldItemPositions.Value = false;
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

            EnableHeldItemPositions.SettingChanged += (_, _) =>
            {
                if (!CSingleton<CGameManager>.Instance.m_IsGameLevel)
                {
                    if (EnableHeldItemPositions.Value)
                    {
                        EnableHeldItemPositions.Value = true;
                    }
                    else
                    {
                        EnableHeldItemPositions.Value = false;
                    }
                }
                else
                {
                    if (EnableHeldItemPositions.Value && !IsPackPositionsReordered)
                    {
                        MovePackPositions();
                    }
                    else if (!EnableHeldItemPositions.Value && IsPackPositionsReordered)
                    {
                        MovePackPositions();
                    }
                }
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
            /*if (EnableMaxHoldPacksValue)
            {
                if (CheckIfIncompatiblePluginsExist("EnableMaxHoldPacks"))
                {
                    EnableMaxHoldPacks.Value = false;
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)), HarmonyPatchType.Prefix);
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf)), HarmonyPatchType.Transpiler);
                }
            }*/
        }

        internal static List<Transform> MovePackPositions()
        {
            if (CSingleton<InteractionPlayerController>.Instance.m_HoldCardPackPosList == null || CSingleton<InteractionPlayerController>.Instance.m_HoldCardPackPosList.Count == 0)
            {
                return null;
            }
            if (!CSingleton<CGameManager>.Instance.m_IsGameLevel)
            {
                return null;
            }

            List<Transform> transformsList = CSingleton<InteractionPlayerController>.Instance.m_HoldCardPackPosList;
            Transform parentTransform = Instantiate(GameObject.Find("HoldPackPosition"), GameObject.Find("FOVAdjustedPositionLoc_Grp").transform, false).transform;
            parentTransform.name = "HoldPackParentTransform";
            Transform baseTransform = Instantiate(GameObject.Find("HoldPackPositionLoc"), GameObject.Find("HoldPackPosition").transform, false).transform;
            baseTransform.name = "HoldPackBaseTransform";
            Transform baseTransform2 = Instantiate(GameObject.Find("HoldPackPositionLoc (1)"), GameObject.Find("HoldPackPosition").transform, false).transform;
            baseTransform2.name = "HoldPackBaseTransform (1)";
            Vector3 basePosition = baseTransform.localPosition;
            Vector3 basePosition2 = baseTransform2.localPosition;
            Quaternion baseRotation = baseTransform.rotation;

            int existingCount = transformsList.Count;
            int totalNeeded = 1024;
            int amountPerRow = 8;
            int numOfRows = 8;
            float scaleFactor = 0.85f;
            float rowSpacing = 0.055f;

            float xDelta = EnableHeldItemPositionsValue ? rowSpacing : (basePosition2.x - basePosition.x);
            float yDelta = basePosition2.y - basePosition.y;
            float zDelta = basePosition2.z - basePosition.z;

            if (transformsList.Count < totalNeeded)
            {
                for (int i = 0; i < totalNeeded; i++)
                {
                    Transform newTransform = Instantiate(baseTransform, parentTransform);
                    newTransform.name = i == 0 ? "HoldPackPositionLoc" : $"HoldPackPositionLoc ({i})";
                    newTransform.rotation = baseRotation;
                    newTransform.localScale = baseTransform.localScale;
                    newTransform.localPosition = basePosition + new Vector3(xDelta, yDelta, zDelta) * i;
                    if (i < existingCount)
                    {
                        transformsList[i] = newTransform;
                    }
                    else
                    {
                        transformsList.Add(newTransform);
                    }
                }
            }

            if ((EnableHeldItemPositionsValue && !IsPackPositionsReordered))
            {
                for (int i = 0; i < totalNeeded; i++)
                {
                    int rowIndex = (i / amountPerRow) % numOfRows;
                    int indexInRow = i % amountPerRow;
                    float xOffset = 0.0267f;
                    int completedRowSets = (i / amountPerRow) / numOfRows;
                    float additionalZOffset = amountPerRow * zDelta;

                    float x;
                    if (rowIndex % 2 == 0)
                    {
                        x = basePosition.x - xDelta * (rowIndex / 2) - xOffset;
                    }
                    else
                    {
                        x = basePosition.x + xDelta * ((rowIndex + 1) / 2) - xOffset;
                    }
                    float y = (basePosition.y + yDelta * indexInRow) - 0.1f;
                    float z = basePosition.z + zDelta * indexInRow + completedRowSets * additionalZOffset;

                    transformsList[i].localScale = baseTransform.localScale * scaleFactor;
                    transformsList[i].localPosition = new Vector3(x, y, z);
                    transformsList[i].rotation = baseRotation;
                }
                IsPackPositionsReordered = true;
            }
            else if ((!EnableHeldItemPositionsValue && IsPackPositionsReordered) || !EnableMaxHoldPacksValue || !EnableModValue)
            {
                for (int i = 0; i < totalNeeded; i++)
                {
                    int rowIndex = i / totalNeeded;
                    int indexInRow = i % totalNeeded;
                    float x = basePosition.x + xDelta * indexInRow;
                    float y = basePosition.y + yDelta * indexInRow;
                    float z = basePosition.z + zDelta * indexInRow;

                    transformsList[i].localScale = baseTransform.localScale;
                    transformsList[i].localPosition = new Vector3(x, y, z);
                    transformsList[i].rotation = baseRotation;
                }
                IsPackPositionsReordered = false;
            }
            IsPackPositionsLoaded = true;
            
            return transformsList;
        }
        internal static void ReorderPackPositions() // don't ask, just accept it
        {
            if (CSingleton<InteractionPlayerController>.Instance.m_HoldCardPackPosList == null || CSingleton<InteractionPlayerController>.Instance.m_HoldCardPackPosList.Count == 0)
            {
                return;
            }
            if (EnableHeldItemPositions.Value)
            {
                EnableHeldItemPositions.Value = false;
                EnableHeldItemPositions.Value = true;
            }
            else
            {
                EnableHeldItemPositions.Value = true;
                EnableHeldItemPositions.Value = false;
            }

            return;
        }

        public static bool EnableHeldItemPositionsValue
        {
            get { return EnableHeldItemPositions.Value; }
        }
        public static float SpeedMultiplierValue
        {
            get { return SpeedMultiplier.Value; }
        }
        public static float PickupSpeedMultiplierValue
        {
            get { return PickupSpeedMultiplier.Value; }
        }
        public static float PackResultsTimerValue
        {
            get { return PackResultsTimer.Value; }
        }
        public static int MaxHoldPacksValue
        {
            get { return MaxHoldPacks.Value; }
        }
        public static bool EnableModValue
        {
            get { return EnableMod.Value; }
        }
        public static bool SkipPackEndScreenValue
        {
            get { return SkipPackEndScreen.Value; }
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
                    if (printInfo) L($"Incompatible plugin found: {plugin}, both mods may not work as intended, consider removing one.");
                    return true;
                }
            }
            return false;
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
