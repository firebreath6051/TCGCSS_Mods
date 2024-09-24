using HarmonyLib;

namespace CardChanceCheat
{
    [HarmonyPatch]
    public class AchievementManager_Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.UnlockAchievement))]
        private static bool AchievementManager_UnlockAchievement_Prefix(ref AchievementManager __instance, string achievementID)
        {
            if (!Plugin.EnableMod.Value) return true;

            return false;
        }
    }
}
