using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace DisplayCustomerMoney
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }
        public static ConfigEntry<bool> EnableMod { get; private set; }
        public static ConfigEntry<KeyboardShortcut> ShowMoneyKey { get; private set; }

        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind("Display Customer Money",
                                    "Enable Mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 2 }));

            ShowMoneyKey = Config.Bind("Keybindings",
                                   "Show Money Key",
                                   new KeyboardShortcut(KeyCode.Q),
                                   new ConfigDescription("Key to show customer money.", null, new ConfigurationManagerAttributes { Order = 1 }));
        }
        private void Update()
        {
            if (EnableMod.Value && ShowMoneyKey.Value.IsDown())
            {
                Patches.ShowCustomersMoney();
            }
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
