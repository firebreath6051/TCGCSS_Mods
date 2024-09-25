using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PhoneStats
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_PhoneScreen), nameof(UI_PhoneScreen.OnOpenScreen))]
        private static void UI_PhoneScreen_OnOpenScreen_Postfix(ref UI_PhoneScreen __instance)
        {
            if (!Plugin.EnableMod.Value) return;

            __instance.m_DayText.text = "";
            __instance.m_DayText.transform.localPosition = new Vector3(__instance.m_DayText.transform.localPosition.x, 3, __instance.m_DayText.transform.localPosition.z);
            __instance.m_DayText.fontSizeMin = 1.4f;

            foreach (FieldInfo field in CPlayerData.m_GameReportDataCollectPermanent.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                string fieldName = field.Name;
                object fieldValue = field.GetValue(CPlayerData.m_GameReportDataCollectPermanent);

                if (field == CPlayerData.m_GameReportDataCollectPermanent.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance).Last())
                {
                    __instance.m_DayText.text += $"{fieldName}: {fieldValue}";
                }
                else
                {
                    __instance.m_DayText.text += $"{fieldName}: {fieldValue}\n";
                }
            }

            return;
        }
    }
}
