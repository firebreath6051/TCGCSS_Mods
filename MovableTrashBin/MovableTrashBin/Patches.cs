using HarmonyLib;
using System;
using UnityEngine;

namespace MovableTrashBin
{
    [HarmonyPatch]
    public static class Patches
    {
        private static bool _isTrashMoving { get; set; }
        private static int _currentSaveSlot { get; set; }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveLoadGameSlotSelectScreen), nameof(SaveLoadGameSlotSelectScreen.OnPressLoadGame))]
        private static void SaveLoadGameSlotSelectScreen_OnPressLoadGame_Postfix(ref SaveLoadGameSlotSelectScreen __instance, int slotIndex)
        {
            if (!Plugin.EnableMod.Value) return;

            _currentSaveSlot = slotIndex;

            return;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InputTooltipListDisplay), nameof(InputTooltipListDisplay.SetCurrentGameState))]
        private static void InputTooltipListDisplay_SetCurrentGameState_Postfix(ref InputTooltipListDisplay __instance, EGameState state)
        {
            if (!Plugin.EnableMod.Value) return;

            if (state == EGameState.MovingObjectState && _isTrashMoving)
            {
                __instance.RemoveTooltip(EGameAction.BoxUpShelf);
            }

            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractableObject), nameof(InteractableObject.Init))]
        private static bool InteractableObject_Init_Prefix(ref InteractableObject __instance)
        {
            if (!Plugin.EnableMod.Value || __instance.name != "Interactable_TrashBin") return true;

            
            TrashBinSaveData loadedData = TrashBinManager.LoadTrashBinData(_currentSaveSlot);
            if (loadedData != null)
            {
                __instance.transform.position = loadedData.pos.Data;
                __instance.transform.rotation = loadedData.rot.Data;
                __instance.m_ObjectType = loadedData.objectType;
            }

            if (!__instance.m_CanPickupMoveObject)
            {
                __instance.m_CanPickupMoveObject = true;
                __instance.m_PlaceObjectInShopOnly = false;
                __instance.m_PlaceObjectInWarehouseOnly = false;
                __instance.m_GameActionInputDisplayList.Add(EGameAction.StartMoveObject);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractableObject), nameof(InteractableObject.StartMoveObject))]
        private static bool InteractableObject_StartMoveObject_Prefix(ref InteractableObject __instance)
        {
            if (!Plugin.EnableMod.Value || __instance.name != "Interactable_TrashBin") return true;

            _isTrashMoving = true;

            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractableObject), nameof(InteractableObject.OnPlacedMovedObject))]
        private static void InteractableObject_OnPlacedMovedObject_Postfix(ref InteractableObject __instance)
        {
            if (!Plugin.EnableMod.Value || __instance.name != "Interactable_TrashBin") return;
            
            TrashBinSaveData saveData = new TrashBinSaveData();
            saveData.pos.SetData(__instance.transform.position);
            saveData.rot.SetData(__instance.transform.rotation);
            saveData.objectType = __instance.m_ObjectType;

            TrashBinManager.SaveTrashBinData(saveData, _currentSaveSlot);

            _isTrashMoving = false;

            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractableObject), nameof(InteractableObject.Update))]
        private static bool InteractableObject_Update_Prefix(ref InteractableObject __instance)
        {
            if (__instance.name != "Interactable_TrashBin") return true;

            if (!Plugin.EnableMod.Value)
            {
                if (__instance.m_CanPickupMoveObject)
                {
                    __instance.m_CanBoxUpObject = false;
                    __instance.m_CanPickupMoveObject = false;
                    __instance.m_PlaceObjectInShopOnly = true;
                    __instance.m_PlaceObjectInWarehouseOnly = true;
                    __instance.m_GameActionInputDisplayList.Add(EGameAction.StartMoveObject);

                    return true;
                }
                return true;
            }

            if (__instance.m_IsLerpingToPos)
            {
                __instance.m_LerpPosTimer += Time.deltaTime * __instance.m_LerpPosSpeed;
                __instance.transform.position = Vector3.Lerp(__instance.m_StartLerpPos, __instance.m_TargetLerpTransform.position, __instance.m_LerpPosTimer);
                __instance.transform.rotation = Quaternion.Lerp(__instance.m_StartLerpRot, __instance.m_TargetLerpTransform.rotation, __instance.m_LerpPosTimer);
                __instance.transform.localScale = Vector3.Lerp(__instance.m_StartLerpScale, __instance.m_TargetLerpTransform.localScale, __instance.m_LerpPosTimer);
                if (__instance.m_LerpPosTimer >= 1f)
                {
                    __instance.m_LerpPosTimer = 0f;
                    __instance.m_IsLerpingToPos = false;
                    __instance.OnFinishLerp();
                    if (__instance.m_IsHideAfterFinishLerp)
                    {
                        __instance.m_IsHideAfterFinishLerp = false;
                        __instance.gameObject.SetActive(false);
                        return false;
                    }
                }
            }
            else if (__instance.m_IsMovingObject)
            {
                if (!__instance.m_IsSnappingPos)
                {
                    __instance.transform.position = Vector3.Lerp(__instance.transform.position, __instance.m_TargetMoveObjectPosition, Time.deltaTime * 7.5f);
                }
                __instance.EvaluateSnapping();
                int mask = LayerMask.GetMask(new string[]
                {
                "Customer"
                });
                Collider[] array = Physics.OverlapBox(__instance.m_MoveStateValidArea.position, __instance.m_MoveStateValidArea.lossyScale / 2f, __instance.m_MoveStateValidArea.rotation, mask);
                bool flag = true;
                if (__instance.m_PlaceObjectInShopOnly)
                {
                    int mask2 = LayerMask.GetMask(new string[]
                    {
                    "MoveStateValidArea"
                    });
                    if (Physics.OverlapBox(__instance.m_MoveStateValidArea.position, __instance.m_MoveStateValidArea.lossyScale / 2f, __instance.m_MoveStateValidArea.rotation, mask2).Length == 0)
                    {
                        flag = false;
                    }
                }
                else if (__instance.m_PlaceObjectInWarehouseOnly)
                {
                    int mask3 = LayerMask.GetMask(new string[]
                    {
                    "MoveStateValidWarehouseArea"
                    });
                    if (Physics.OverlapBox(__instance.m_MoveStateValidArea.position, __instance.m_MoveStateValidArea.lossyScale / 2f, __instance.m_MoveStateValidArea.rotation, mask3).Length == 0)
                    {
                        flag = false;
                    }
                }
                if (array.Length != 0 || __instance.transform.position.y > 0.1f)
                {
                    flag = false;
                }
                if (__instance.m_IsMovingObjectValidState != flag)
                {
                    __instance.m_IsMovingObjectValidState = flag;
                    ShelfManager.SetMoveObjectPreviewModelValidState(__instance.m_IsMovingObjectValidState);
                }
            }
            return false;
        }
    }
}
