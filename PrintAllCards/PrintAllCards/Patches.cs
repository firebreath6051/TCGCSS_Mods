using HarmonyLib;
using System.Collections.Generic;

namespace PrintAllCards
{
    [HarmonyPatch]
    public static class Patches
    {
        public static CGameDataDupe GameDataDupe { get; set; }

        public static List<MonsterData> MonsterDatas = new List<MonsterData>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnFinishHideLoadingScreen))]
        private static void InteractionPlayerController_OnFinishHideLoadingScreen_Postfix(ref InteractionPlayerController __instance)
        {
            MonsterDatas = CSingleton<InventoryBase>.Instance.m_MonsterData_SO.m_DataList;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CSaveLoad), nameof(CSaveLoad.Load))]
        private static void CSaveLoad_Load_Postfix()
        {

        }
    }
}
