using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WorkerPersonalRestock
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(ShelfManager), nameof(ShelfManager.GetShelfListToRestockItem))]
        public static bool ShelfManager_GetShelfListToRestockItem_Prefix(ref List<Shelf> __result, EItemType itemType, bool ignoreNoneType = false)
        {
            if (!Plugin.EnableMod.Value) return true;

            List<Shelf> list = new List<Shelf>();
            for (int i = 0; i < CSingleton<ShelfManager>.Instance.m_ShelfList.Count; i++)
            {
                if (CSingleton<ShelfManager>.Instance.m_ShelfList[i].IsValidObject() && CSingleton<ShelfManager>.Instance.m_ShelfList[i].GetNonFullItemCompartment(itemType, ignoreNoneType))
                {
                    list.Add(CSingleton<ShelfManager>.Instance.m_ShelfList[i]);
                }
            }
            __result = list;

            return false;
        }
    }
}
