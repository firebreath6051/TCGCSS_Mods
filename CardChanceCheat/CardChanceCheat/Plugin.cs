using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace CardChanceCheat
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }

        internal static bool DataWasSaved = false;
        internal static GameReportDataCollect DataCollectPermanent {  get; set; }
        internal static ConfigEntry<bool> EnableMod { get; private set; }
        internal static ConfigEntry<bool> FullGhostPack { get; private set; }
        internal static ConfigEntry<float> FoilChance { get; private set; }
        internal static ConfigEntry<float> FirstEdChance { get; private set; }
        internal static ConfigEntry<float> SilverChance { get; private set; }
        internal static ConfigEntry<float> GoldChance { get; private set; }
        internal static ConfigEntry<float> EXChance { get; private set; }
        internal static ConfigEntry<float> FullArtChance { get; private set; }
        internal static ConfigEntry<float> GhostChance { get; private set; }
        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind("Cheat Chances",
                                    "Enable mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 9 }));

            FullGhostPack = Config.Bind("Cheat Chances",
                                    "Full ghost pack",
                                    false,
                                    new ConfigDescription("If a ghost pack contains only ghost cards", null, new ConfigurationManagerAttributes { Order = 8 }));

            FoilChance = Config.Bind("Cheat Chances",
                                         "Foil Chance",
                                         5f,
                                         new ConfigDescription("Chance for a card to be foil", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 7 }));

            FirstEdChance = Config.Bind("Cheat Chances",
                                         "First Edition Chance",
                                         20f,
                                         new ConfigDescription("Chance for a card to be first edition", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 6 }));

            SilverChance = Config.Bind("Cheat Chances",
                                         "Silver Chance",
                                         8f,
                                         new ConfigDescription("Chance for a card to be silver", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 5 }));

            GoldChance = Config.Bind("Cheat Chances",
                                         "Gold Chance",
                                         4f,
                                         new ConfigDescription("Chance for a card to be gold", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 4 }));

            EXChance = Config.Bind("Cheat Chances",
                                         "EX Chance",
                                         1f,
                                         new ConfigDescription("Chance for a card to be EX", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 3 }));

            FullArtChance = Config.Bind("Cheat Chances",
                                         "Full Art Chance",
                                         0.25f,
                                         new ConfigDescription("Chance for a card to be full art", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 2 }));

            GhostChance = Config.Bind("Cheat Chances",
                                         "Ghost Pack Chance",
                                         0.1f,
                                         new ConfigDescription("Chance for a regular pack to instead be a ghost pack, which contains a guaranteed ghost card", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 1 }));

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
        private void Update()
        {
            if (EnableMod.Value)
            {
                RevertData();
            }
        }

        private void RevertData(bool showLog = false)
        {
            if (DataWasSaved)
            {
                if (!CPlayerData.m_GameReportDataCollectPermanent.Equals(DataCollectPermanent))
                {
                    CPlayerData.m_GameReportDataCollectPermanent = DataCollectPermanent;
                    if (showLog) L("Data reverted");
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
    }
}
