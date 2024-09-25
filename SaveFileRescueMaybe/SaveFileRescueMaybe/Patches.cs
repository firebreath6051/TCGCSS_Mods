using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace SaveFileRescueMaybe
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShelfManager), nameof(ShelfManager.LoadInteractableObjectData))]
        private static bool ShelfManager_LoadInteractableObjectData_Prefix(ref ShelfManager __instance)
        {
            if (!Plugin.EnableMod.Value) return true;

            //CPlayerData.m_InteractableObjectSaveDataList.Clear();

            Plugin.L($"InteractableObject count before removing card pack boxes: {CPlayerData.m_PackageBoxItemSaveDataList.Count}");
            for (int i = CPlayerData.m_PackageBoxItemSaveDataList.Count - 1; i >= 0; i--)
            {
                if (CPlayerData.m_PackageBoxItemSaveDataList[i].itemTypeAmount.itemType.ToString().Contains("CardPack"))
                {
                    CPlayerData.m_PackageBoxItemSaveDataList.RemoveAt(i);
                }
            }
            Plugin.L($"InteractableObject count after removing card pack boxes: {CPlayerData.m_PackageBoxItemSaveDataList.Count}");

            return true;
        }
    }
}
