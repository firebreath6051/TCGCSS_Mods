using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PrintAllCards
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }
        public static ConfigEntry<bool> EnableMod { get; private set; }
        internal static ConfigEntry<KeyboardShortcut> PrintCardDataKey { get; private set; }
        internal static ConfigEntry<KeyboardShortcut> SaveDataKey { get; private set; }
        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind("Print ALl Card Data",
                                    "Enable Mod",
                                    true,
                                    new ConfigDescription("Enable this mod", null, new ConfigurationManagerAttributes { Order = 3 }));

            PrintCardDataKey = Config.Bind("Print ALl Card Data",
                                   "Print card data key",
                                   new KeyboardShortcut(KeyCode.P),
                                   new ConfigDescription("Key to print card data to json file.", null, new ConfigurationManagerAttributes { Order = 2 }));

            SaveDataKey = Config.Bind("Print ALl Card Data",
                                   "Save savegame data key",
                                   new KeyboardShortcut(KeyCode.PageDown),
                                   new ConfigDescription("Saves the current savegame data to a json file.", null, new ConfigurationManagerAttributes { Order = 1 }));
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
            if (!EnableMod.Value) return;
            if (SaveDataKey.Value.IsDown())
            {
                JsonHandler.WriteSaveDataToFile(Patches.GameDataDupe);
            }
            if (PrintCardDataKey.Value.IsDown())
            {
                if (Patches.MonsterDatas.Count > 0)
                {
                    try
                    {
                        L($"Update: {Patches.MonsterDatas.Count}");
                        List<MonsterDataParams> monsterDataToSave = new List<MonsterDataParams>();
                        for (int i = 0; i < Patches.MonsterDatas.Count; i++)
                        {
                            MonsterDataParams mon = new MonsterDataParams(Patches.MonsterDatas[i]);
                            MonsterData monData = Patches.MonsterDatas[i];
                            monsterDataToSave.Add(mon);

                            if (InventoryBase.GetShownMonsterList(ECardExpansionType.Tetramon).Contains(monData.MonsterType))
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add(Enum.GetName(typeof(ECardExpansionType), ECardExpansionType.Tetramon));
                            }
                            if (InventoryBase.GetShownMonsterList(ECardExpansionType.Destiny).Contains(monData.MonsterType))
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add(Enum.GetName(typeof(ECardExpansionType), ECardExpansionType.Destiny));
                            }
                            if (InventoryBase.GetShownMonsterList(ECardExpansionType.Ghost).Contains(monData.MonsterType))
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add(Enum.GetName(typeof(ECardExpansionType), ECardExpansionType.Ghost));
                            }
                            if (InventoryBase.GetShownMonsterList(ECardExpansionType.FantasyRPG).Contains(monData.MonsterType))
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add(Enum.GetName(typeof(ECardExpansionType), ECardExpansionType.FantasyRPG));
                            }
                            if (InventoryBase.GetShownMonsterList(ECardExpansionType.CatJob).Contains(monData.MonsterType))
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add(Enum.GetName(typeof(ECardExpansionType), ECardExpansionType.CatJob));
                            }
                            if (InventoryBase.GetShownMonsterList(ECardExpansionType.FoodieGO).Contains(monData.MonsterType))
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add(Enum.GetName(typeof(ECardExpansionType), ECardExpansionType.FoodieGO));
                            }
                            if (InventoryBase.GetShownMonsterList(ECardExpansionType.Megabot).Contains(monData.MonsterType))
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add(Enum.GetName(typeof(ECardExpansionType), ECardExpansionType.Megabot));
                            }
                            else
                            {
                                monsterDataToSave[i].CardExpansionTypes.Add("None");
                            }
                            L($"Monster {monsterDataToSave[i].Name} added to list");
                        }
                        JsonHandler.SaveCardDataToFile(monsterDataToSave);
                    }
                    catch (Exception ex) 
                    {
                        L($"Exception occurred: {ex.Message}");
                    }
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
