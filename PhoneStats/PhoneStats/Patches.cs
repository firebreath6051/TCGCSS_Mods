using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
using System.Reflection.Emit;
using Unity.VisualScripting;
using TMPro;

namespace PhoneStats
{
    [HarmonyPatch]
    public static class Patches
    {
        private static TextMeshProUGUI m_StatsText;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UI_PhoneScreen), nameof(UI_PhoneScreen.OnOpenScreen))]
        private static void UI_PhoneScreen_OnOpenScreen_Postfix(ref UI_PhoneScreen __instance)
        {
            if (!Plugin.EnableMod.Value) return;

            __instance.m_DayText.text = "Packs opened: " + CPlayerData.m_GameReportDataCollectPermanent.cardPackOpened;

            return;
        }
    }
}
