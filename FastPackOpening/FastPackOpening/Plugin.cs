using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FastPackOpening
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }

        internal static InteractionPlayerController InteractionPlayerController = new InteractionPlayerController();
        public static bool IsPackPositionsLoaded { get; set; }
        public static bool IsPackPositionsReordered { get; set; }
        internal static bool IsFirstRun { get; set; }
        public static ConfigEntry<bool> EnableMod { get; private set; }
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

            EnableMod = Config.Bind("1. Config Options",
                                    "Enable mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 6 }));

            AutoOpenKey = Config.Bind("1. Config Options",
                                   "Auto open toggle on/off",
                                   new KeyboardShortcut(KeyCode.BackQuote),
                                   new ConfigDescription("Key to toggle automatic opening of packs.", null, new ConfigurationManagerAttributes { Order = 5 }));

            StopAutoHighValue = Config.Bind("1. Config Options",
                                    "Stop auto open at high value cards",
                                    true,
                                    new ConfigDescription("Stops automatically opening packs when you get a high value card.", null, new ConfigurationManagerAttributes { Order = 4 }));

            HighValueThreshold = Config.Bind("1. Config Options",
                                    "High value threshold",
                                    10,
                                    new ConfigDescription("This value determines when a card is considered \"High Value\" and plays a special animation so you can see what card it is and its value. Default is 10.",
                                                            new AcceptableValueRange<int>(1, 10000), new ConfigurationManagerAttributes { Order = 3 }));

            SpeedMultiplier = Config.Bind("1. Config Options",
                                         "Speed multiplier",
                                         1f,
                                         new ConfigDescription("Speed multiplier", new AcceptableValueRange<float>(1, 10), new ConfigurationManagerAttributes { Order = 2 }));

            PickupSpeedMultiplier = Config.Bind("1. Config Options",
                                         "Pickup speed multiplier",
                                         1f,
                                         new ConfigDescription("Speed multiplier for card pack pick up and put down.", new AcceptableValueRange<float>(1, 10), new ConfigurationManagerAttributes { Order = 1 }));

            EnableMaxHoldPacks = Config.Bind("2. Held Pack Options",
                                    "Enable max hold limit",
                                    true,
                                    new ConfigDescription("Enable holding more than 8 packs", null, new ConfigurationManagerAttributes { Order = 6 }));

            MaxHoldPacks = Config.Bind("2. Held Pack Options",
                                         "Held pack limit",
                                         8,
                                         new ConfigDescription("How many packs you can hold in your hand.", new AcceptableValueRange<int>(1, 64), new ConfigurationManagerAttributes { Order = 5 }));

            EnableHeldItemPositions = Config.Bind("2. Held Pack Options",
                                    "Enable pack repositioning",
                                    false,
                                    new ConfigDescription("Makes held packs stack in rows of 8 in your hands, instead of a single row of up to 64.", null, new ConfigurationManagerAttributes { Order = 4 }));

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
                    EnableMaxHoldPacks.Value = false;
                    //Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)), HarmonyPatchType.Prefix);
                    Harmony.Unpatch(AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf)), HarmonyPatchType.Transpiler);
                }
                if (EnableMaxHoldPacksValue && !CheckIfIncompatiblePluginsExist("EnableMaxHoldPacks"))
                {
                    /*Harmony.Patch(
                        AccessTools.Method(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded)),
                        prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.InteractionPlayerController_OnGameDataFinishLoaded_Prefix))
                    );*/
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

            EnableHeldItemPositions.SettingChanged += (_, _) =>
            {
                if (EnableModValue && EnableMaxHoldPacksValue)
                {
                    if (EnableHeldItemPositionsValue)
                    {
                        MovePackPositions();
                    }
                }
                if (!EnableHeldItemPositionsValue)
                {
                    MovePackPositions();
                }
            };

        }

        private void OnEnable()
        {
            Harmony.PatchAll();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is loaded! Made by WiseHorror (Nargacuga on Nexus)");
            IsFirstRun = true;
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
        public struct TransformData
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;

            public TransformData(Vector3 position, Quaternion rotation, Vector3 scale)
            {
                this.position = position;
                this.rotation = rotation;
                this.scale = scale;
            }
        }

        internal static List<TransformData> OriginalPosList = new List<TransformData>();

        internal static void MovePackPositions()
        {
            List<Transform> transformsList = InteractionPlayerController.instance.m_HoldCardPackPosList;
            Transform baseTransform = transformsList[0].parent;
            Vector3 basePosition = new Vector3(transformsList[0].position.x, transformsList[0].position.y, transformsList[0].position.z);
            Vector3 basePosition2 = new Vector3(transformsList[1].position.x, transformsList[1].position.y, transformsList[1].position.z);
            Quaternion baseRotation = baseTransform.rotation;

            int existingCount = transformsList.Count;
            int totalNeeded = 64;
            int amountPerRow = 8;
            float scaleFactor = 0.85f;

            float xDelta = basePosition2.x - basePosition.x;
            float yDelta = basePosition2.y - basePosition.y;
            float zDelta = 0.03f;

            if (transformsList.Count < totalNeeded)
            {
                for (int i = existingCount; i < totalNeeded; i++)
                {
                    GameObject newGameObject = new GameObject("HoldPackPositionLoc (" + (i) + ")");
                    Transform newTransform = newGameObject.transform;
                    newTransform.parent = baseTransform;
                    newTransform.rotation = EnableHeldItemPositionsValue ? baseRotation : new Quaternion(0, 0, 0, 1);
                    newTransform.localScale = transformsList[0].localScale;
                    newTransform.position = basePosition + new Vector3(xDelta, yDelta, zDelta) * i;
                    transformsList.Add(newTransform);
                }
            }

            // Store original positions, rotations, and scales if not already done
            if (OriginalPosList == null || OriginalPosList.Count == 0)
            {
                OriginalPosList = new List<TransformData>();
                // Ensure that the number of elements in the transformsList is the same as existingCount
                for (int i = 0; i < existingCount; i++)
                {
                    if (i < transformsList.Count)
                    {
                        Transform t = transformsList[i];
                        OriginalPosList.Add(new TransformData(t.position, t.rotation, t.localScale));
                    }
                }
            }

            if (EnableHeldItemPositionsValue)
            {
                for (int i = 0; i < totalNeeded; i++)
                {
                    int rowIndex = i / amountPerRow;
                    int indexInRow = i % amountPerRow;
                    float x = basePosition.x + xDelta * indexInRow;
                    float y = basePosition.y + yDelta * indexInRow;
                    float z;
                    if (rowIndex % 2 == 0)
                    {
                        z = basePosition.z - zDelta * (rowIndex / 2) - 0.0155f;
                    }
                    else
                    {
                        z = basePosition.z + zDelta * ((rowIndex + 1) / 2) - 0.0155f;
                    }

                    transformsList[i].localScale = Vector3.one * scaleFactor;
                    transformsList[i].position = new Vector3(x, y, z);
                    transformsList[i].rotation = baseRotation;
                }
                IsPackPositionsReordered = true;
            }
            else
            {
                // Ensure we do not access out-of-bounds indexes
                for (int i = 0; i < existingCount; i++)
                {
                    if (i < transformsList.Count && i < OriginalPosList.Count)
                    {
                        transformsList[i].position = OriginalPosList[i].position;
                        transformsList[i].rotation = OriginalPosList[i].rotation;
                        transformsList[i].localScale = OriginalPosList[i].scale;
                    }
                }
                IsPackPositionsReordered = false;
            }
            IsFirstRun = false;
            IsPackPositionsLoaded = true;
        }

        /*internal static void MovePackPositionsReorder()
        {
            if (!IsPackPositionsLoaded)
            {
                MovePackPositions();
            }
            if (InteractionPlayerController.instance.m_HoldCardPackPosList.Count < 10)
            {
                return;
            }
            L("MovePackPositionsReorder");
            if (InteractionPlayerController.instance.m_HoldCardPackPosList.Count < 64)
            {
                List<Transform> transformsList = InteractionPlayerController.instance.m_HoldCardPackPosList;
                Transform baseTransform = transformsList.First();
                Vector3 basePosition = baseTransform.position;
                Vector3 lastDelta = transformsList[transformsList.Count - 1].position - transformsList[transformsList.Count - 2].position;

                for (int i = 0; i < 54; i++)
                {
                    Vector3 newPosition = transformsList[transformsList.Count - 1].position + lastDelta;
                    newPosition = basePosition + (newPosition - basePosition);
                    GameObject newGameObject = new GameObject("HoldPackPositionLoc (" + (transformsList.Count) + ")");
                    Transform newTransform = newGameObject.transform;
                    newTransform.parent = baseTransform;
                    newTransform.rotation = baseTransform.rotation;
                    newTransform.localScale = baseTransform.localScale;
                    newTransform.position = newPosition;
                    transformsList.Add(newTransform);
                }
            }
        }
        internal static void ResetPacksPos()
        {
            if ((!IsPackPositionsReordered && IsPackPositionsLoaded) || OriginalPosList.Count == 0)
            {
                return;
            }
            L("ResetPackPos");
            try
            {
                if (InteractionPlayerController.instance.m_HoldCardPackPosList.Count < 64)
                {
                    List<Transform> transformsList = InteractionPlayerController.instance.m_HoldCardPackPosList;
                    Transform baseTransform = transformsList.First();
                    Vector3 basePosition = baseTransform.position;
                    Vector3 lastDelta = OriginalPosList[transformsList.Count - 1].position - OriginalPosList[transformsList.Count - 2].position;

                    for (int i = 0; i < transformsList.Count; i++)
                    {
                        Vector3 newPosition = OriginalPosList[OriginalPosList.Count - 1].position + lastDelta;
                        newPosition = basePosition + (newPosition - basePosition);
                        GameObject newGameObject = new GameObject("HoldPackPositionLoc (" + (transformsList.Count) + ")");
                        Transform newTransform = newGameObject.transform;
                        newTransform.parent = baseTransform;
                        newTransform.rotation = baseTransform.rotation;
                        newTransform.localScale = baseTransform.localScale;
                        newTransform.position = newPosition;
                        transformsList[i] = newTransform;
                    }
                }
            }
            catch (Exception e)
            {
                L(e.Message);
            }
            IsPackPositionsReordered = false;
        }*/

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
                    if (printInfo) L($"Forcing {setting} to disabled due to incompatible plugin: {plugin}");
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
