using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine.Rendering.PostProcessing;

namespace SaveFileRescue
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }
        internal static ConfigEntry<bool> EnableMod { get; private set; }
        internal static ConfigEntry<bool> DeleteCardPackBoxes { get; private set; }
        internal static ConfigEntry<bool> DeleteEmptyBoxes { get; private set; }
        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            EnableMod = Config.Bind("Save File Rescue",
                                    "Enable mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 9 }));

            DeleteEmptyBoxes = Config.Bind("Save File Rescue",
                                    "Delete empty boxes",
                                    true,
                                    new ConfigDescription("Deletes all empty boxes upon loading a save file.", null, new ConfigurationManagerAttributes { Order = 8 }));

            DeleteCardPackBoxes = Config.Bind("Save File Rescue",
                                    "Delete card pack boxes",
                                    true,
                                    new ConfigDescription("Deletes all boxes containing card packs upon loading a save file.", null, new ConfigurationManagerAttributes { Order = 7 }));
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
