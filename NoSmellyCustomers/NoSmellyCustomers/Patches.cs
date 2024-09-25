using HarmonyLib;

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
