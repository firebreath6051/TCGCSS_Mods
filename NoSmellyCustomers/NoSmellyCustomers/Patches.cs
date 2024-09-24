using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BepInEx.Logging;
using System.Reflection;
using System.Reflection.Emit;

namespace NoSmellyCustomers
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Customer), nameof(Customer.SetSmelly))]
        private static bool Customer_SetSmelly_Prefix(ref Customer __instance)
        {
            if (!Plugin.EnableMod.Value) return true;

            return false;
        }
    }
}
