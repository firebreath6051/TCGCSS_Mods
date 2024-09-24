using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace FastWorkbench
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractableWorkbench), nameof(InteractableWorkbench.PlayBundlingCardBoxSequence))]
        private static bool InteractableWorkbench_PlayBundlingCardBoxSequence_Prefix(ref InteractableWorkbench __instance, List<CardData> cardDataListShown, ECardExpansionType cardExpansionType)
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

            __instance.m_JankBoxAnim.gameObject.SetActive(true);
            __instance.m_JankBoxAnim.SetBool("IsClosing", true);
            ItemMeshData itemMeshData = InventoryBase.GetItemMeshData(__instance.GetBulkBoxItemType(cardExpansionType));
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
        private static bool InteractableWorkbench_OnMouseButtonUp_Prefix(ref InteractableWorkbench __instance)
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
        private static bool WorkbenchUIScreen_Update_Prefix(ref WorkbenchUIScreen __instance)
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
        private static bool WorkbenchUIScreen_OnSliderValueChanged_PriceLimit_Prefix(ref WorkbenchUIScreen __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

            if (__instance.m_IgnoreSliderUpdateFunction)
            {
                return false;
            }
            __instance.m_PriceLimit = (float)Mathf.RoundToInt(__instance.m_SliderPriceLimit.value) / 100f;
            __instance.m_PriceLimitText.text = GameInstance.GetPriceString(__instance.m_PriceLimit, false, true, false, "F2");
            CPlayerData.m_WorkbenchPriceLimit = __instance.m_PriceLimit;

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WorkbenchUIScreen), nameof(WorkbenchUIScreen.OpenScreen))]
        private static bool WorkbenchUIScreen_OpenScreen_Prefix(InteractableWorkbench interactableWorkbench)
        {
            if (!Plugin.EnableMod.Value)
            {
                CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceLimit.maxValue = 1f;
                return true;
            }

            CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceLimit.maxValue = Plugin.MaxPriceLimit.Value * 100f;

            if (CPlayerData.m_WorkbenchPriceLimit > 0f)
            {
                CSingleton<WorkbenchUIScreen>.Instance.m_MinimumCardLimit = CPlayerData.m_WorkbenchMinimumCardLimit;
                CSingleton<WorkbenchUIScreen>.Instance.m_PriceLimit = CPlayerData.m_WorkbenchPriceLimit;
                CSingleton<WorkbenchUIScreen>.Instance.m_RarityLimit = CPlayerData.m_WorkbenchRarityLimit;
                CSingleton<WorkbenchUIScreen>.Instance.m_CurrentCardExpansionType = CPlayerData.m_WorkbenchCardExpansionType;
            }
            CSingleton<WorkbenchUIScreen>.Instance.m_IgnoreSliderUpdateFunction = true;
            CSingleton<WorkbenchUIScreen>.Instance.m_SliderPriceLimit.value = CPlayerData.m_WorkbenchPriceLimit * 100f;
            CSingleton<WorkbenchUIScreen>.Instance.m_SliderMinCard.value = (float)CPlayerData.m_WorkbenchMinimumCardLimit / 10f * 10f;
            CSingleton<WorkbenchUIScreen>.Instance.m_PriceLimitMinText.text = GameInstance.GetPriceString(0.01f, false, true, false, "F2");
            CSingleton<WorkbenchUIScreen>.Instance.m_PriceLimitMaxText.text = GameInstance.GetPriceString(Plugin.MaxPriceLimit.Value, false, true, false, "F2");
            CSingleton<WorkbenchUIScreen>.Instance.m_IgnoreSliderUpdateFunction = false;
            CSingleton<WorkbenchUIScreen>.Instance.m_PriceLimitText.text = GameInstance.GetPriceString(CSingleton<WorkbenchUIScreen>.Instance.m_PriceLimit, false, true, false, "F2");
            CSingleton<WorkbenchUIScreen>.Instance.m_MinimumCardText.text = CSingleton<WorkbenchUIScreen>.Instance.m_MinimumCardLimit.ToString();
            CSingleton<WorkbenchUIScreen>.Instance.m_TaskFinishCirlceGrp.SetActive(false);
            CSingleton<WorkbenchUIScreen>.Instance.m_CurrentInteractableWorkbench = interactableWorkbench;
            CSingleton<WorkbenchUIScreen>.Instance.m_ScreenGrp.SetActive(true);
            SoundManager.GenericMenuOpen(1f, 1f);
            ControllerScreenUIExtManager.OnOpenScreen(CSingleton<WorkbenchUIScreen>.Instance.m_ControllerScreenUIExtension);
            TutorialManager.SetGameUIVisible(false);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractableWorkbench), nameof(InteractableWorkbench.Update))]
        private static bool InteractableWorkbench_Update_Prefix(ref InteractableWorkbench __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

            __instance = InteractableObjectUpdate(__instance);
            if (__instance.m_IsPlayCardEnterAnim)
            {
                __instance.m_CardEnterTimer += Time.deltaTime * Plugin.SpeedMultiplier.Value;
                if (__instance.m_CardEnterTimer > 0.1f)
                {
                    __instance.m_CardEnterBoxAnimList[__instance.m_CardEnterIndex].gameObject.SetActive(false);
                    __instance.m_CardEnterBoxAnimList[__instance.m_CardEnterIndex].gameObject.SetActive(true);
                    __instance.m_InteractableCard3dList[__instance.m_CardEnterIndex].m_Card3dUI.SetVisibility(true);
                    __instance.m_CardEnterBoxAnimList[__instance.m_CardEnterIndex].Play();
                    __instance.m_CardEnterTimer = 0f;
                    __instance.m_CardEnterIndex++;
                    if (__instance.m_CardEnterIndex >= __instance.m_InteractableCard3dList.Count)
                    {
                        __instance.m_IsPlayCardEnterAnim = false;
                        __instance.m_CardEnterIndex = 0;
                    }
                }
            }

            return false;
        }

        public static InteractableWorkbench InteractableObjectUpdate(InteractableWorkbench interactableWorkbench)
        {
            if (interactableWorkbench.m_IsLerpingToPos)
            {
                interactableWorkbench.m_LerpPosTimer += Time.deltaTime * interactableWorkbench.m_LerpPosSpeed;
                interactableWorkbench.transform.position = Vector3.Lerp(interactableWorkbench.m_StartLerpPos, interactableWorkbench.m_TargetLerpTransform.position, interactableWorkbench.m_LerpPosTimer);
                interactableWorkbench.transform.rotation = Quaternion.Lerp(interactableWorkbench.m_StartLerpRot, interactableWorkbench.m_TargetLerpTransform.rotation, interactableWorkbench.m_LerpPosTimer);
                interactableWorkbench.transform.localScale = Vector3.Lerp(interactableWorkbench.m_StartLerpScale, interactableWorkbench.m_TargetLerpTransform.localScale, interactableWorkbench.m_LerpPosTimer);
                if (interactableWorkbench.m_LerpPosTimer >= 1f)
                {
                    interactableWorkbench.m_LerpPosTimer = 0f;
                    interactableWorkbench.m_IsLerpingToPos = false;
                    interactableWorkbench.OnFinishLerp();
                    if (interactableWorkbench.m_IsHideAfterFinishLerp)
                    {
                        interactableWorkbench.m_IsHideAfterFinishLerp = false;
                        interactableWorkbench.gameObject.SetActive(false);
                        return interactableWorkbench;
                    }
                }
            }
            else if (interactableWorkbench.m_IsMovingObject)
            {
                if (!interactableWorkbench.m_IsSnappingPos)
                {
                    interactableWorkbench.transform.position = Vector3.Lerp(interactableWorkbench.transform.position, interactableWorkbench.m_TargetMoveObjectPosition, Time.deltaTime * 7.5f);
                }
                interactableWorkbench.EvaluateSnapping();
                int mask = LayerMask.GetMask(new string[]
                {
            "MoveStateBlockedArea",
            "Customer"
                });
                Collider[] array = Physics.OverlapBox(interactableWorkbench.m_MoveStateValidArea.position, interactableWorkbench.m_MoveStateValidArea.lossyScale / 2f, interactableWorkbench.m_MoveStateValidArea.rotation, mask);
                bool flag = true;
                if (interactableWorkbench.m_PlaceObjectInShopOnly)
                {
                    int mask2 = LayerMask.GetMask(new string[]
                    {
                "MoveStateValidArea"
                    });
                    if (Physics.OverlapBox(interactableWorkbench.m_MoveStateValidArea.position, interactableWorkbench.m_MoveStateValidArea.lossyScale / 2f, interactableWorkbench.m_MoveStateValidArea.rotation, mask2).Length == 0)
                    {
                        flag = false;
                    }
                }
                else if (interactableWorkbench.m_PlaceObjectInWarehouseOnly)
                {
                    int mask3 = LayerMask.GetMask(new string[]
                    {
                "MoveStateValidWarehouseArea"
                    });
                    if (Physics.OverlapBox(interactableWorkbench.m_MoveStateValidArea.position, interactableWorkbench.m_MoveStateValidArea.lossyScale / 2f, interactableWorkbench.m_MoveStateValidArea.rotation, mask3).Length == 0)
                    {
                        flag = false;
                    }
                }
                if (array.Length != 0 || interactableWorkbench.transform.position.y > 0.1f)
                {
                    flag = false;
                }
                if (interactableWorkbench.m_IsMovingObjectValidState != flag)
                {
                    interactableWorkbench.m_IsMovingObjectValidState = flag;
                    ShelfManager.SetMoveObjectPreviewModelValidState(interactableWorkbench.m_IsMovingObjectValidState);
                }
            }
            return interactableWorkbench;
        }
    }
}
