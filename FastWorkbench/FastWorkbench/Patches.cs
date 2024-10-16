using HarmonyLib;
using I2.Loc;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace FastWorkbench
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractableWorkbench), nameof(InteractableWorkbench.PlayBundlingCardBoxSequence))]
        public static bool InteractableWorkbench_PlayBundlingCardBoxSequence_Prefix(ref InteractableWorkbench __instance, List<CardData> cardDataListShown, ECardExpansionType cardExpansionType, float totalPrice)
        {
            if (!Plugin.EnableMod.Value)
            {
                for (int i = 0; i < __instance.m_CardEnterBoxAnimList.Count; i++)
                {
                    foreach (AnimationState state in __instance.m_CardEnterBoxAnimList[i])
                    {
                        state.speed = 1f;
                    }
                }
                return true;
            }

            for (int i = 0; i < __instance.m_CardEnterBoxAnimList.Count; i++)
            {
                if (__instance.m_CardEnterBoxAnimList[i] != null)
                {
                    foreach (AnimationState state in __instance.m_CardEnterBoxAnimList[i])
                    {
                        state.speed = 1 + ((Plugin.SpeedMultiplier.Value - 1) * 10);
                    }
                }
            }

            __instance.m_JankBoxAnim.speed = Plugin.SpeedMultiplier.Value;
            __instance.m_JankBoxAnim.gameObject.SetActive(value: true);
            __instance.m_JankBoxAnim.SetBool("IsClosing", value: true);
            __instance.m_CurrentItemTypeSpawn = __instance.GetBulkBoxItemType(cardExpansionType, totalPrice);
            ItemMeshData itemMeshData = InventoryBase.GetItemMeshData(__instance.m_CurrentItemTypeSpawn);
            __instance.m_JankBoxSkinMesh.material = itemMeshData.material;
            for (int i = 0; i < cardDataListShown.Count; i++)
            {
                __instance.m_InteractableCard3dList[i].m_Card3dUI.m_CardUI.SetCardUI(cardDataListShown[i]);
            }

            __instance.m_IsPlayCardEnterAnim = true;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractableWorkbench), nameof(InteractableWorkbench.OnMouseButtonUp))]
        public static bool InteractableWorkbench_OnMouseButtonUp_Prefix(ref InteractableWorkbench __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

            CSingleton<InteractionPlayerController>.Instance.OnEnterWorkbenchMode();
            __instance.OnRaycastEnded();
            InteractionPlayerController.SetPlayerPos(__instance.m_LockPlayerPos.position);
            CSingleton<InteractionPlayerController>.Instance.EnterUIMode();
            CSingleton<InteractionPlayerController>.Instance.ForceLookAt(__instance.m_PlayerLookRot, 3f);
            __instance.m_NavMeshCutWhenManned.SetActive(true);
            WorkbenchUIScreen.OpenScreen(__instance);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WorkbenchUIScreen), nameof(WorkbenchUIScreen.Update))]
        public static bool WorkbenchUIScreen_Update_Prefix(ref WorkbenchUIScreen __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

            __instance.m_TaskTime = 5f / Plugin.SpeedMultiplier.Value;

            if (__instance.m_IsWorkingOnTask)
            {
                __instance.m_TaskTimer += Time.deltaTime;
                __instance.m_TaskFinishCirlceFillBar.fillAmount = Mathf.Lerp(0f, 1f, __instance.m_TaskTimer / __instance.m_TaskTime);
                if (__instance.m_TaskTimer >= __instance.m_TaskTime)
                {
                    __instance.m_TaskTimer = 0f;
                    __instance.m_IsWorkingOnTask = false;
                    __instance.OnTaskCompleted();
                }
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WorkbenchUIScreen), nameof(WorkbenchUIScreen.OnSliderValueChanged_PriceLimit))]
        public static bool WorkbenchUIScreen_OnSliderValueChanged_PriceLimit_Prefix(ref WorkbenchUIScreen __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

            if (!__instance.m_IgnoreSliderUpdateFunction)
            {
                __instance.m_PriceLimit = (float)Mathf.RoundToInt(__instance.m_SliderPriceLimit.value) / 100f;
                __instance.m_PriceLimitText.text = GameInstance.GetPriceString(__instance.m_PriceLimit);
                CPlayerData.m_WorkbenchPriceLimit = __instance.m_PriceLimit;
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WorkbenchUIScreen), nameof(WorkbenchUIScreen.OnSliderValueChanged_PriceMinimum))]
        public static bool WorkbenchUIScreen_OnSliderValueChanged_PriceMinimum_Prefix(ref WorkbenchUIScreen __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

            if (!__instance.m_IgnoreSliderUpdateFunction)
            {
                __instance.m_PriceMinimum = (float)Mathf.RoundToInt(__instance.m_SliderPriceMinimum.value) / 100f;
                __instance.m_PriceMinimumText.text = GameInstance.GetPriceString(__instance.m_PriceMinimum);
                CPlayerData.m_WorkbenchPriceMinimum = __instance.m_PriceMinimum;
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(WorkbenchUIScreen), nameof(WorkbenchUIScreen.OpenScreen))]
        public static void WorkbenchUIScreen_OpenScreen_Postfix(InteractableWorkbench interactableWorkbench)
        {
            if (!Plugin.EnableMod.Value)
            {
                CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceLimit.maxValue = CPlayerData.m_WorkbenchPriceLimit * 100f;
                CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceMinimum.value = CPlayerData.m_WorkbenchPriceMinimum * 100f;
                return;
            }

            CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceLimit.minValue = Plugin.MinimumPriceLimit.Value * 100f;
            CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceLimit.maxValue = Plugin.MaxPriceLimit.Value * 100f;
            CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceMinimum.minValue = Plugin.MinimumPriceLimit.Value * 100f;
            CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceMinimum.maxValue = Plugin.MaxPriceLimit.Value * 100f;
            CSingleton<WorkbenchUIScreen>.Instance.m_PriceLimitMinText.text = GameInstance.GetPriceString(Plugin.MinimumPriceLimit.Value);
            CSingleton<WorkbenchUIScreen>.Instance.m_PriceLimitMaxText.text = GameInstance.GetPriceString(Plugin.MaxPriceLimit.Value);
            CSingleton<WorkbenchUIScreen>.Instance.m_PriceMinimumMinText.text = GameInstance.GetPriceString(Plugin.MinimumPriceLimit.Value);
            CSingleton<WorkbenchUIScreen>.Instance.m_PriceMinimumMaxText.text = GameInstance.GetPriceString(Plugin.MaxPriceLimit.Value);
            return;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(InteractableWorkbench), nameof(InteractableWorkbench.Update))]
        public static IEnumerable<CodeInstruction> InteractableWorkbench_Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            MethodInfo deltaTimeGetter = AccessTools.Property(typeof(Time), nameof(Time.deltaTime)).GetGetMethod();
            MethodInfo speedMultiplierGetter = AccessTools.Property(typeof(Plugin), nameof(Plugin.SpeedMultiplierValue)).GetGetMethod();

            for (int i = 0; i < codes.Count; i++)
            {
                if ((codes[i].opcode == OpCodes.Call || codes[i].opcode == OpCodes.Callvirt) &&
                    codes[i].operand is MethodInfo method &&
                    method == deltaTimeGetter)
                {
                    codes.InsertRange(i + 1, new List<CodeInstruction>
                    {
                        new CodeInstruction(OpCodes.Call, speedMultiplierGetter),
                        new CodeInstruction(OpCodes.Mul)
                    });

                    i += 2;

                    break;
                }
            }

            return codes;
        }
    }
}
