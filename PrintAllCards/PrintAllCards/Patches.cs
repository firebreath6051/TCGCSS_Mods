using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PrintAllCards
{
    [HarmonyPatch]
    public static class Patches
    {
        public static List<MonsterData> MonsterDatas = new List<MonsterData>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnFinishHideLoadingScreen))]
        private static void InteractionPlayerController_OnFinishHideLoadingScreen_Postfix(ref InteractionPlayerController __instance)
        {
            MonsterDatas = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_DataList;
        }
    }
}
