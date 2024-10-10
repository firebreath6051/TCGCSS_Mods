using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection.Emit;
using TMPro;

namespace FastPackOpening
{
    [HarmonyPatch]
    public class Patches
    {
        public static bool IsAutoOpen { get; set; }
        public static bool DelayAutoOpen { get; set; }
        public static bool IsFirstLoad = true;
        public static int PacksInHand { get; set; }
        public static float PackSpeedMultiplier { get; set; }
        public static bool IsGetNewOrHighValueCard = false;
        public static float LogTimer { get; set; }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateOpenCardPack))]
        public static bool InteractionPlayerController_EvaluateOpenCardPack_Prefix(ref InteractionPlayerController __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

            if (__instance.CanOpenPack())
            {
                Item item = __instance.m_HoldItemList[0];
                __instance.RemoveHoldItem(item);
                CSingleton<CardOpeningSequence>.Instance.ReadyingCardPack(item);
                __instance.m_IsHoldingMouseDown = false;
                __instance.m_IsHoldingRightMouseDown = false;
                return false;
            }
            if (__instance.CanOpenCardBox())
            {
                Plugin.MovePackPositions();
                __instance.m_IsOpeningCardBox = true;
                Item item2 = __instance.m_HoldItemList[0];
                EItemType eitemType = __instance.CardBoxToCardPack(item2.GetItemType());
                if (eitemType == EItemType.None)
                {
                    return false;
                }
                ItemMeshData itemMeshData = InventoryBase.GetItemMeshData(item2.GetItemType());
                __instance.m_OpenCardBoxMeshFilter.mesh = itemMeshData.mesh;
                __instance.m_OpenCardBoxMesh.material = itemMeshData.material;
                item2.gameObject.SetActive(false);
                __instance.m_HoldItemList.Clear();
                CPlayerData.m_HoldItemTypeList.Clear();
                InteractionPlayerController.RemoveToolTip(EGameAction.OpenCardBox);
                SoundManager.PlayAudio("SFX_OpenCardBox", 0.6f, 1f);
                __instance.m_OpenCardBoxInnerMesh.gameObject.SetActive(true);
                __instance.m_OpenCardBoxInnerMesh.Rewind();
                __instance.m_OpenCardBoxInnerMesh.Play();
                if (Plugin.EnableMod.Value)
                {
                    __instance.m_OpenCardBoxInnerMesh["OpenCardBoxAnim"].speed = Plugin.SpeedMultiplierValue;
                }
                else
                {
                    __instance.m_OpenCardBoxInnerMesh["OpenCardBoxAnim"].speed = 1f;
                }
                for (int i = 0; i < 8; i++)
                {
                    ItemMeshData itemMeshData2 = InventoryBase.GetItemMeshData(eitemType);
                    Item item3 = ItemSpawnManager.GetItem(__instance.m_OpenCardBoxSpawnCardPackPosList[i]);
                    item3.SetMesh(itemMeshData2.mesh, itemMeshData2.material, eitemType, itemMeshData2.meshSecondary, itemMeshData2.materialSecondary);
                    item3.transform.position = __instance.m_OpenCardBoxSpawnCardPackPosList[i].position;
                    item3.transform.rotation = __instance.m_OpenCardBoxSpawnCardPackPosList[i].rotation;
                    item3.transform.parent = __instance.m_OpenCardBoxSpawnCardPackPosList[i];
                    item3.transform.localScale = __instance.m_OpenCardBoxSpawnCardPackPosList[i].localScale;
                    item3.gameObject.SetActive(true);
                    __instance.m_HoldItemList.Add(item3);
                    CPlayerData.m_HoldItemTypeList.Add(item3.GetItemType());
                    __instance.StartCoroutine(__instance.DelayLerpSpawnedCardPackToHand(i, (1.25f + 0.05f * (float)i) / Plugin.SpeedMultiplierValue, item3, __instance.m_HoldCardPackPosList[i], item2));
                }
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.RaycastHoldItemState))]
        public static void InteractionPlayerController_RaycastHoldItemState_Postfix(ref InteractionPlayerController __instance, float __state)
        {
            if (!Plugin.EnableMod.Value) return;

            if (__instance.m_CurrentTrashBin)
            {
                __instance.m_MouseHoldAutoFireRate = 0.15f;
            }
            else
            {
                __instance.m_MouseHoldAutoFireRate = 0.15f / Plugin.PickupSpeedMultiplierValue;
            }

            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded))]
        public static bool InteractionPlayerController_OnGameDataFinishLoaded_Prefix(ref InteractionPlayerController __instance)
        {
            if (IsFirstLoad)
            {
                if (Plugin.EnableMod.Value)
                {
                    __instance.m_MouseHoldAutoFireRate = 0.15f / Plugin.PickupSpeedMultiplierValue;
                }
                else
                {
                    __instance.m_MouseHoldAutoFireRate = 0.15f;
                }
                __instance.m_HoldCardPackPosList = Plugin.MovePackPositions();
                IsFirstLoad = false;
            }
            else
            {
                Plugin.ReorderPackPositions();
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded))]
        public static void InteractionPlayerController_OnGameDataFinishLoaded_Postfix(ref InteractionPlayerController __instance)
        {
            if (Plugin.EnableAutoOpenStatusText.Value)
            {
                autoOpenStatusText.gameObject.SetActive(true);
            }
            else
            {
                autoOpenStatusText.gameObject.SetActive(false);
            }

            CSingleton<InteractionPlayerController>.Instance.m_CameraController.SetRotationAngles(0f, -35f);
            CSingleton<InteractionPlayerController>.Instance.m_CameraController.enabled = false;
            if (CPlayerData.m_HoldItemTypeList.Count > 7 && !CPlayerData.m_HoldItemTypeList[0].ToString().Contains("CardPack"))
            {
                for (int i = CPlayerData.m_HoldItemTypeList.Count - 1; i >= 8; i--)
                {
                    CPlayerData.m_HoldItemTypeList.RemoveAt(i);
                }
            }
            if (CPlayerData.m_HoldCardDataList.Count > 7)
            {
                for (int j = CPlayerData.m_HoldCardDataList.Count - 1; j >= 8; j--)
                {
                    CPlayerData.m_HoldCardDataList.RemoveAt(j);
                }
            }
            if (CPlayerData.m_HoldCardDataList.Count > 0)
            {
                List<InteractableCard3d> list = new List<InteractableCard3d>();
                for (int k = 0; k < CPlayerData.m_HoldCardDataList.Count; k++)
                {
                    Card3dUIGroup cardUI = CSingleton<Card3dUISpawner>.Instance.GetCardUI();
                    InteractableCard3d component = ShelfManager.SpawnInteractableObject(EObjectType.Card3d).GetComponent<InteractableCard3d>();
                    cardUI.m_CardUI.SetCardUI(CPlayerData.m_HoldCardDataList[k]);
                    cardUI.transform.position = CSingleton<InteractionPlayerController>.Instance.m_HoldCardPosList[k].position;
                    cardUI.transform.rotation = CSingleton<InteractionPlayerController>.Instance.m_HoldCardPosList[k].rotation;
                    component.transform.position = CSingleton<InteractionPlayerController>.Instance.m_HoldCardPosList[k].position;
                    component.transform.rotation = CSingleton<InteractionPlayerController>.Instance.m_HoldCardPosList[k].rotation;
                    component.SetCardUIFollow(cardUI);
                    component.SetEnableCollision(false);
                    list.Add(component);
                }
                CPlayerData.m_HoldCardDataList.Clear();
                for (int l = 0; l < list.Count; l++)
                {
                    InteractionPlayerController.AddHoldCard(list[l]);
                }
                __instance.EnterHoldCardMode();
                return;
            }
            if (CPlayerData.m_HoldItemTypeList.Count > 0)
            {
                List<Item> list2 = new List<Item>();
                for (int m = 0; m < CPlayerData.m_HoldItemTypeList.Count; m++)
                {
                    ItemMeshData itemMeshData = InventoryBase.GetItemMeshData(CPlayerData.m_HoldItemTypeList[m]);
                    Item item = ItemSpawnManager.GetItem(__instance.m_HoldCardPackPosList[m]);
                    item.SetMesh(itemMeshData.mesh, itemMeshData.material, CPlayerData.m_HoldItemTypeList[m], itemMeshData.meshSecondary, itemMeshData.materialSecondary);
                    item.transform.position = __instance.m_HoldCardPackPosList[m].position;
                    item.transform.rotation = __instance.m_HoldCardPackPosList[m].rotation;
                    item.SmoothLerpToTransform(__instance.m_HoldCardPackPosList[m], __instance.m_HoldCardPackPosList[m], false);
                    item.gameObject.SetActive(true);
                    list2.Add(item);
                }
                for (int n = 0; n < list2.Count; n++)
                {
                    __instance.m_HoldItemList.Add(list2[n]);
                }
                __instance.SetCurrentGameState(EGameState.HoldingItemState);
                __instance.m_IsHoldItemMode = true;
                Plugin.ReorderPackPositions();
                return;
            }
            return;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf))]
        public static IEnumerable<CodeInstruction> InteractionPlayerController_EvaluateTakeItemFromShelf_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codes = new List<CodeInstruction>(instructions);

            var getEnableModValue = AccessTools.Property(typeof(Plugin), nameof(Plugin.EnableModValue))?.GetGetMethod();
            var getEnableMaxHoldPacksValue = AccessTools.Property(typeof(Plugin), nameof(Plugin.EnableMaxHoldPacksValue))?.GetGetMethod();
            var getMaxHoldPacksValue = AccessTools.Property(typeof(Plugin), nameof(Plugin.MaxHoldPacksValue))?.GetGetMethod();

            var labelOriginalValue = generator.DefineLabel();
            var labelContinue = generator.DefineLabel();
            var labelOriginalValue2 = generator.DefineLabel();
            var labelContinue2 = generator.DefineLabel();

            List<CodeInstruction> newCode = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, getEnableModValue),
                new CodeInstruction(OpCodes.Call, getEnableMaxHoldPacksValue),
                new CodeInstruction(OpCodes.And),
                new CodeInstruction(OpCodes.Brfalse_S, labelOriginalValue),
                new CodeInstruction(OpCodes.Call, getMaxHoldPacksValue),
                new CodeInstruction(OpCodes.Br_S, labelContinue)
            };
            List<CodeInstruction> newCode2 = new List<CodeInstruction>()
            {
                new CodeInstruction(OpCodes.Call, getEnableModValue),
                new CodeInstruction(OpCodes.Call, getEnableMaxHoldPacksValue),
                new CodeInstruction(OpCodes.And),
                new CodeInstruction(OpCodes.Brfalse_S, labelOriginalValue2),
                new CodeInstruction(OpCodes.Call, getMaxHoldPacksValue),
                new CodeInstruction(OpCodes.Br_S, labelContinue2)
            };

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4_8)
                {
                    if (i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Stloc_1)
                    {
                        codes[i].labels.Add(labelOriginalValue);
                        codes[i + 1].labels.Add(labelContinue);
                        codes.InsertRange(i, newCode);

                        i += newCode.Count + 1;
                    }
                    if (i + 1 < codes.Count && codes[i + 1].opcode == OpCodes.Stloc_S)
                    {
                        codes[i].labels.Add(labelOriginalValue2);
                        codes[i + 1].labels.Add(labelContinue2);
                        codes.InsertRange(i, newCode2);

                        i += newCode2.Count + 1;
                    }
                }
            }
            return codes.AsEnumerable();
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.EvaluateTakeItemFromShelf))]
        public static void InteractionPlayerController_EvaluateTakeItemFromShelf_Postfix(ref InteractionPlayerController __instance)
        {
            if (__instance.m_HoldItemList[0].m_ItemType.ToString().Contains("CardPack"))
            {
            }

            return;
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardOpeningSequenceUI), nameof(CardOpeningSequenceUI.Update))]
        public static bool CardOpeningSequenceUI_Update_Prefix(ref CardOpeningSequenceUI __instance)
        {
            if (!Plugin.EnableMod.Value) return true;

            if (__instance.m_IsShowingTotalValue)
            {
                __instance.m_TotalValueLerpTimer += (Time.deltaTime * 0.5f) * Plugin.SpeedMultiplierValue;
                __instance.m_CurrentTotalCardValueLerp = Mathf.Lerp(0f, __instance.m_TargetTotalCardValueLerp, __instance.m_TotalValueLerpTimer);
                __instance.m_TotalCardValueText.text = GameInstance.GetPriceString(__instance.m_CurrentTotalCardValueLerp, false, true, false, "F2");
                if (__instance.m_TotalValueLerpTimer >= 1f)
                {
                    __instance.m_IsShowingTotalValue = false;
                    SoundManager.SetEnableSound_ExpIncrease(false);
                }
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardOpeningSequenceUI), nameof(CardOpeningSequenceUI.ShowSingleCardValue))]
        public static bool CardOpeningSequenceUI_ShowSingleCardValue_Prefix(ref CardOpeningSequenceUI __instance, ref float cardValue)
        {
            if (!Plugin.EnableMod.Value) return true;

            if (!__instance.m_ScreenGrp.activeSelf)
            {
                __instance.m_ScreenGrp.SetActive(true);
            }
            __instance.m_CardValueText.text = GameInstance.GetPriceString(cardValue, false, true, false, "F2");
            __instance.m_CardValueTextGrp.SetActive(true);

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardOpeningSequence), nameof(CardOpeningSequence.ReadyingCardPack))]
        public static bool CardOpeningSequence_ReadyingCardPack_Prefix(ref CardOpeningSequence __instance, Item item)
        {
            if (!Plugin.EnableMod.Value || !Plugin.DisableSoundsValue) return true;

            if (!__instance.m_IsReadyingToOpen)
            {
                __instance.m_IsScreenActive = true;
                CSingleton<InteractionPlayerController>.Instance.EnterLockMoveMode();
                CSingleton<InteractionPlayerController>.Instance.OnEnterOpenPackState();
                __instance.m_IsReadyingToOpen = true;
                __instance.m_IsReadyToOpen = false;
                __instance.m_LerpPosTimer = 0f;
                __instance.m_CurrentItem = item;
                __instance.m_CardPackAnimator.transform.position = __instance.m_StartLerpTransform.position;
                __instance.m_CardPackAnimator.transform.rotation = __instance.m_StartLerpTransform.rotation;
                __instance.m_CardPackAnimator.transform.localScale = __instance.m_StartLerpTransform.localScale;
                __instance.m_CardPackMesh.material = __instance.m_CurrentItem.m_Mesh.sharedMaterial;
                __instance.m_CardPackAnimator.gameObject.SetActive(value: true);
                __instance.m_CurrentItem.gameObject.SetActive(value: false);
                __instance.m_CardOpeningUIGroup.SetActive(value: false);
                __instance.m_CardPackAnimator.Play("PackOpenAnim", -1, 0f);
                CSingleton<InteractionPlayerController>.Instance.m_BlackBGWorldUIFade.SetFadeIn(3f);
                TutorialManager.SetGameUIVisible(isVisible: false);
                CenterDot.SetVisibility(isVisible: false);
                GameUIScreen.HideEnterGoNextDayIndicatorVisible();
                InteractionPlayerController.TempHideToolTip();
                InteractionPlayerController.AddToolTip(EGameAction.OpenPack, isHold: true);
                InteractionPlayerController.AddToolTip(EGameAction.CancelOpenPack);
                InteractionPlayerController.SetAllHoldItemVisibility(isVisible: false);
                CSingleton<InteractionPlayerController>.Instance.m_CameraFOVController.StartLerpToFOV(40f);
            }

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardOpeningSequence), nameof(CardOpeningSequence.ReadyingCardPack))]
        public static void CardOpeningSequence_ReadyingCardPack_Postfix(ref CardOpeningSequence __instance)
        {
            if (!Plugin.EnableMod.Value) return;

            __instance.m_MultiplierStateTimer = (1f + 2.5f * CSingleton<CGameManager>.Instance.m_OpenPackSpeedSlider) * Plugin.SpeedMultiplierValue;

            return;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetEnableSound_ExpIncrease))]
        public static void SoundManager_SetEnableSound_ExpIncrease_Postfix()
        {
            if (!Plugin.EnableMod.Value || !Plugin.DisableSoundsValue) return;

            CSingleton<SoundManager>.Instance.m_ExpIncrease.volume = 0f;

            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardOpeningSequence), nameof(CardOpeningSequence.Update))]
        public static bool CardOpeningSequence_Update_Prefix(ref CardOpeningSequence __instance)
        {
            if (!Plugin.EnableMod.Value)
            {
                __instance.m_HighValueCardThreshold = 10f;
                __instance.m_CardOpeningRotateToFrontAnim["CardOpenSeq1_RotateToFront"].speed = 1f;
                __instance.m_CardOpeningRotateToFrontAnim["CardOpenSeq0_Idle"].speed = 1f;

                foreach (var card in __instance.m_CardAnimList)
                {
                    card["OpenCardNewCard"].speed = 1f;
                    card["OpenCardFinalReveal"].speed = 1f;
                }
                return true;
            }

            if (Plugin.AutoOpenKey.Value.IsDown())
            {
                ToggleAutoOpen();
            }

            __instance.m_HighValueCardThreshold = Plugin.HighValueThreshold.Value;
            __instance.m_CardOpeningRotateToFrontAnim["CardOpenSeq1_RotateToFront"].speed = Plugin.SpeedMultiplierValue;
            __instance.m_CardOpeningRotateToFrontAnim["CardOpenSeq0_Idle"].speed = Plugin.SpeedMultiplierValue;

            foreach (var card in __instance.m_CardAnimList)
            {
                card["OpenCardNewCard"].speed = Plugin.SpeedMultiplierValue;
                card["OpenCardFinalReveal"].speed = Plugin.SpeedMultiplierValue;
            }

            if (PackSpeedMultiplier != (Plugin.SpeedMultiplierValue - __instance.m_MultiplierStateTimer))
            {
                PackSpeedMultiplier = Mathf.Clamp(Plugin.SpeedMultiplierValue - __instance.m_MultiplierStateTimer, 0f, Plugin.SpeedMultiplierValue);
            }

            __instance.m_IsAutoFire = false;
            if (IsAutoOpen)
            {
                __instance.m_IsAutoFire = true;
            }
            else
            {
                __instance.m_IsAutoFire = false;
            }
            if (!__instance.m_IsScreenActive)
            {
                return false;
            }
            if (InputManager.GetKeyDownAction(EGameAction.OpenPack))
            {
                __instance.m_IsAutoFireKeydown = true;
            }
            if (InputManager.GetKeyUpAction(EGameAction.OpenPack))
            {
                __instance.m_IsAutoFireKeydown = false;
            }
            if (__instance.m_IsAutoFireKeydown)
            {
                __instance.m_AutoFireTimer += Time.deltaTime;
                if (__instance.m_AutoFireTimer >= 0.05f)
                {
                    __instance.m_AutoFireTimer = 0f;
                    __instance.m_IsAutoFire = true;
                }
            }
            else if (__instance.m_AutoFireTimer > 0f)
            {
                __instance.m_AutoFireTimer = 0f;
                __instance.m_IsAutoFire = true;
            }
            if (__instance.m_IsReadyingToOpen)
            {
                if (!__instance.m_IsReadyToOpen)
                {
                    if (__instance.m_IsCanceling)
                    {
                        __instance.m_LerpPosTimer -= Time.deltaTime * __instance.m_LerpPosSpeed;
                        if (__instance.m_LerpPosTimer < 0f)
                        {
                            __instance.m_LerpPosTimer = 0f;
                            __instance.m_IsReadyToOpen = false;
                            __instance.m_IsReadyingToOpen = false;
                            __instance.m_IsCanceling = false;
                            __instance.m_IsScreenActive = false;
                            __instance.m_CardPackAnimator.gameObject.SetActive(false);
                            CSingleton<InteractionPlayerController>.Instance.ExitLockMoveMode();
                            CSingleton<InteractionPlayerController>.Instance.OnExitOpenPackState();
                            InteractionPlayerController.RestoreHiddenToolTip();
                            __instance.m_CurrentItem.gameObject.SetActive(true);
                            InteractionPlayerController.SetAllHoldItemVisibility(true);
                            __instance.m_CurrentItem = null;
                            TutorialManager.SetGameUIVisible(true);
                            CenterDot.SetVisibility(true);
                            GameUIScreen.ResetEnterGoNextDayIndicatorVisible();
                        }
                    }
                    else
                    {
                        __instance.m_LerpPosTimer += (Time.deltaTime * __instance.m_LerpPosSpeed) * Plugin.SpeedMultiplierValue;
                        if (__instance.m_LerpPosTimer > 1f)
                        {
                            __instance.m_LerpPosTimer = 1f;
                            __instance.m_IsReadyToOpen = true;
                        }
                    }
                    __instance.m_CardPackAnimator.transform.localPosition = Vector3.Lerp(__instance.m_StartLerpTransform.localPosition, Vector3.zero, __instance.m_LerpPosTimer);
                    __instance.m_CardPackAnimator.transform.localRotation = Quaternion.Lerp(__instance.m_StartLerpTransform.localRotation, Quaternion.identity, __instance.m_LerpPosTimer);
                    __instance.m_CardPackAnimator.transform.localScale = Vector3.Lerp(__instance.m_StartLerpTransform.localScale, Vector3.one, __instance.m_LerpPosTimer);
                    return false;
                }
                if (__instance.m_IsAutoFire)
                {
                    __instance.m_IsReadyingToOpen = false;
                    __instance.OpenScreen(InventoryBase.ItemTypeToCollectionPackType(__instance.m_CurrentItem.GetItemType()), false, false);
                    return false;
                }
                if (InputManager.GetKeyDownAction(EGameAction.CancelOpenPack) && !__instance.m_IsCanceling)
                {
                    CSingleton<InteractionPlayerController>.Instance.AddHoldItemToFront(__instance.m_CurrentItem);
                    __instance.m_IsCanceling = true;
                    __instance.m_IsReadyToOpen = false;
                    CSingleton<InteractionPlayerController>.Instance.m_BlackBGWorldUIFade.SetFadeOut(3f, 0f);
                    InteractionPlayerController.RestoreHiddenToolTip();
                    CSingleton<InteractionPlayerController>.Instance.m_CameraFOVController.StopLerpFOV();
                    SoundManager.GenericPop(1f, 0.9f);
                }
                return false;
            }
            else
            {
                if (!__instance.m_IsScreenActive)
                {
                    return false;
                }
                if (__instance.m_StateIndex == 0)
                {
                    __instance.InitOpenSequence();
                    __instance.m_StateIndex++;
                    return false;
                }
                if (__instance.m_StateIndex == 1)
                {
                    __instance.m_StateTimer += Time.deltaTime * (__instance.m_MultiplierStateTimer + PackSpeedMultiplier);
                    if (__instance.m_StateTimer > 0.05f)
                    {
                        __instance.m_StateTimer = 0f;
                        if (__instance.m_TempIndex < __instance.m_Card3dUIList.Count)
                        {
                            __instance.m_Card3dUIList[__instance.m_TempIndex].gameObject.SetActive(true);
                            __instance.m_TempIndex++;
                        }
                    }
                    if (__instance.m_IsAutoFire || __instance.m_IsAutoFireKeydown || CSingleton<CGameManager>.Instance.m_OpenPacAutoNextCard)
                    {
                        __instance.m_Slider += (0.0065f * (__instance.m_MultiplierStateTimer + PackSpeedMultiplier));
                        __instance.m_CardPackAnimator.Play("PackOpenAnim", -1, __instance.m_Slider);
                        if (__instance.m_Slider >= 0.3f)
                        {
                            __instance.m_OpenPackVFX.Play();
                            if (!Plugin.DisableSoundsValue)
                            {
                                SoundManager.PlayAudio("SFX_OpenPack", 0.6f, 1f);
                                SoundManager.PlayAudio("SFX_BoxOpen", 0.5f, 1f);
                            }
                            __instance.m_StateIndex++;
                            return false;
                        }
                    }
                }
                else if (__instance.m_StateIndex == 2)
                {
                    __instance.m_Slider += Time.deltaTime * 1f * (__instance.m_MultiplierStateTimer + PackSpeedMultiplier);
                    __instance.m_CardPackAnimator.Play("PackOpenAnim", -1, __instance.m_Slider);
                    __instance.m_StateTimer += Time.deltaTime * Plugin.SpeedMultiplierValue;
                    if (__instance.m_StateTimer > 0.05f)
                    {
                        __instance.m_StateTimer = 0f;
                        if (__instance.m_TempIndex < __instance.m_Card3dUIList.Count)
                        {
                            __instance.m_Card3dUIList[__instance.m_TempIndex].gameObject.SetActive(true);
                            __instance.m_TempIndex++;
                        }
                    }
                    if (__instance.m_Slider >= 1f)
                    {
                        InteractionPlayerController.RemoveToolTip(EGameAction.OpenPack);
                        __instance.m_TempIndex = 0;
                        __instance.m_StateTimer = 0f;
                        __instance.m_Slider = 0f;
                        __instance.m_StateIndex++;
                        for (int i = 0; i < __instance.m_Card3dUIList.Count; i++)
                        {
                            __instance.m_Card3dUIList[i].gameObject.SetActive(true);
                        }
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 3)
                {
                    __instance.m_Slider += Time.deltaTime * 1f * (__instance.m_MultiplierStateTimer + PackSpeedMultiplier);
                    if (__instance.m_Slider >= 0.15f)
                    {
                        __instance.m_Slider = 0f;
                        __instance.m_StateIndex++;
                        __instance.m_CardOpeningRotateToFrontAnim.Play("CardOpenSeq1_RotateToFront");
                    }
                    if (__instance.m_IsAutoFire || CSingleton<CGameManager>.Instance.m_OpenPacAutoNextCard)
                    {
                        float num = 0.002f * (float)__instance.m_CurrentOpenedCardIndex;
                        float num2 = 0.001f * (float)__instance.m_CurrentOpenedCardIndex;
                        if (!Plugin.DisableSoundsValue)
                        {
                            SoundManager.PlayAudio("SFX_CardReveal1", 0.6f + num2, 1f + num);
                        }
                        __instance.m_CardOpeningRotateToFrontAnim.Play("CardOpenSeq1_RotateToFront");
                        __instance.m_StateTimer = 0f;
                        __instance.m_StateIndex++;
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 4)
                {
                    __instance.m_Slider += Time.deltaTime * 1f * (__instance.m_MultiplierStateTimer + PackSpeedMultiplier);
                    if (!__instance.m_CardOpeningSequenceUI.m_CardValueTextGrp.activeSelf && __instance.m_CurrentOpenedCardIndex < 6 && __instance.m_Slider >= 0.45f && !__instance.m_IsNewlList[__instance.m_CurrentOpenedCardIndex])
                    {
                        __instance.m_TotalCardValue += __instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex];
                        __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex]);
                    }
                    if (__instance.m_Slider >= 0.8f)
                    {
                        __instance.m_Slider = 0f;
                        if (__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex] >= __instance.m_HighValueCardThreshold)
                        {
                            if (Plugin.StopAutoHighValue.Value && IsAutoOpen)
                            {
                                IsAutoOpen = false;
                                DelayAutoOpen = true;
                                __instance.m_IsAutoFireKeydown = false;
                                IsGetNewOrHighValueCard = true;
                            }
                            SoundManager.PlayAudio("SFX_FinalizeCard", 0.6f, 1.2f);
                            __instance.m_CardAnimList[__instance.m_CurrentOpenedCardIndex].Play("OpenCardNewCard");
                            __instance.m_HighValueCardIcon.SetActive(true);
                            __instance.StartCoroutine(__instance.DelayToState(5, 0.9f / Plugin.SpeedMultiplierValue));
                            __instance.m_TotalCardValue += __instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex];
                            __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex]);
                            __instance.m_IsGetHighValueCard = true;
                            return false;
                        }
                        if (__instance.m_IsNewlList[__instance.m_CurrentOpenedCardIndex])
                        {
                            if (Plugin.StopAutoNewCard.Value && IsAutoOpen)
                            {
                                IsAutoOpen = false;
                                DelayAutoOpen = true;
                                __instance.m_IsAutoFireKeydown = false;
                                IsGetNewOrHighValueCard = true;
                            }
                            SoundManager.PlayAudio("SFX_CardReveal0", 0.6f, 1f);
                            __instance.m_CardAnimList[__instance.m_CurrentOpenedCardIndex].Play("OpenCardNewCard");
                            __instance.m_NewCardIcon.SetActive(true);
                            __instance.StartCoroutine(__instance.DelayToState(5, 0.9f / Plugin.SpeedMultiplierValue));
                            __instance.m_TotalCardValue += __instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex];
                            __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex]);
                            return false;
                        }
                        __instance.m_StateIndex++;
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 5)
                {
                    if (__instance.m_IsAutoFire || (!IsGetNewOrHighValueCard && CSingleton<CGameManager>.Instance.m_OpenPacAutoNextCard))
                    {
                        int num3 = Random.Range(0, 3);
                        float num4 = 0.002f * (float)__instance.m_CurrentOpenedCardIndex;
                        float num5 = 0.001f * (float)__instance.m_CurrentOpenedCardIndex;
                        if (num3 == 0)
                        {
                            if (!Plugin.DisableSoundsValue)
                            {
                                SoundManager.PlayAudio("SFX_CardReveal1", 0.6f + num5, 1f + num4);
                            }
                        }
                        else if (num3 == 1)
                        {
                            if (!Plugin.DisableSoundsValue)
                            {
                                SoundManager.PlayAudio("SFX_CardReveal2", 0.6f + num5, 1f + num4);
                            }
                        }
                        else
                        {
                            if (!Plugin.DisableSoundsValue)
                            {
                                SoundManager.PlayAudio("SFX_CardReveal3", 0.6f + num5, 1f + num4);
                            }
                        }
                        if (__instance.m_CurrentOpenedCardIndex >= 7)
                        {
                            __instance.m_StateIndex = 7;
                        }
                        else
                        {
                            __instance.m_StateIndex++;
                            __instance.m_NewCardIcon.SetActive(false);
                            __instance.m_HighValueCardIcon.SetActive(false);
                            __instance.m_CardAnimList[__instance.m_CurrentOpenedCardIndex].Play("OpenCardSlideExit");
                            __instance.m_CardAnimList[__instance.m_CurrentOpenedCardIndex]["OpenCardSlideExit"].speed = 1f * (__instance.m_MultiplierStateTimer + PackSpeedMultiplier);
                            __instance.m_CardOpeningSequenceUI.HideSingleCardValue();
                        }
                        __instance.m_IsGetHighValueCard = false;
                        IsGetNewOrHighValueCard = false;
                        if ((DelayAutoOpen && !IsAutoOpen) || (__instance.m_IsNewlList[__instance.m_CurrentOpenedCardIndex] && DelayAutoOpen && !IsAutoOpen))
                        {
                            IsAutoOpen = true;
                        }
                        DelayAutoOpen = false;
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 6)
                {
                    __instance.m_Slider += Time.deltaTime * 1f * (__instance.m_MultiplierStateTimer + PackSpeedMultiplier);
                    if (!__instance.m_CardOpeningSequenceUI.m_CardValueTextGrp.activeSelf && __instance.m_CurrentOpenedCardIndex < 6 && __instance.m_Slider >= 0.3f && !__instance.m_IsNewlList[__instance.m_CurrentOpenedCardIndex + 1] && __instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex + 1] < __instance.m_HighValueCardThreshold)
                    {
                        __instance.m_TotalCardValue += __instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex + 1];
                        __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex + 1]);
                    }
                    if (__instance.m_Slider >= 0.5f)
                    {
                        __instance.m_Slider = 0f;
                        if (__instance.m_Card3dUIList.Count > __instance.m_CurrentOpenedCardIndex)
                        {
                            __instance.m_CardAnimList[__instance.m_CurrentOpenedCardIndex].transform.localPosition = Vector3.zero;
                            __instance.m_Card3dUIList[__instance.m_CurrentOpenedCardIndex].gameObject.SetActive(false);
                        }
                        __instance.m_CurrentOpenedCardIndex++;
                        if (__instance.m_CurrentOpenedCardIndex >= 7)
                        {
                            __instance.m_IsGetHighValueCard = false;
                            __instance.m_StateIndex = 7;
                            return false;
                        }
                        if (__instance.m_Card3dUIList.Count > __instance.m_CurrentOpenedCardIndex + 1)
                        {
                            __instance.m_Card3dUIList[__instance.m_CurrentOpenedCardIndex + 1].gameObject.SetActive(true);
                        }
                        if (__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex] >= __instance.m_HighValueCardThreshold)
                        {
                            if (Plugin.StopAutoHighValue.Value && IsAutoOpen)
                            {
                                IsAutoOpen = false;
                                DelayAutoOpen = true;
                                __instance.m_IsAutoFireKeydown = false;
                                IsGetNewOrHighValueCard = true;
                            }
                            SoundManager.PlayAudio("SFX_FinalizeCard", 0.6f, 1.2f);
                            __instance.m_CardAnimList[__instance.m_CurrentOpenedCardIndex].Play("OpenCardNewCard");
                            __instance.m_HighValueCardIcon.SetActive(true);
                            __instance.StartCoroutine(__instance.DelayToState(5, 0.9f / Plugin.SpeedMultiplierValue));
                            __instance.m_TotalCardValue += __instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex];
                            __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex]);
                            __instance.m_IsGetHighValueCard = true;
                            return false;
                        }
                        if (__instance.m_IsNewlList[__instance.m_CurrentOpenedCardIndex])
                        {
                            if (Plugin.StopAutoNewCard.Value && IsAutoOpen)
                            {
                                IsAutoOpen = false;
                                DelayAutoOpen = true;
                                __instance.m_IsAutoFireKeydown = false;
                                IsGetNewOrHighValueCard = true;
                            }
                            SoundManager.PlayAudio("SFX_CardReveal0", 0.6f, 1f);
                            __instance.m_CardAnimList[__instance.m_CurrentOpenedCardIndex].Play("OpenCardNewCard");
                            __instance.m_NewCardIcon.SetActive(true);
                            __instance.StartCoroutine(__instance.DelayToState(5, 0.9f / Plugin.SpeedMultiplierValue));
                            __instance.m_TotalCardValue += __instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex];
                            __instance.m_CardOpeningSequenceUI.ShowSingleCardValue(__instance.m_CardValueList[__instance.m_CurrentOpenedCardIndex]);
                            return false;
                        }
                        __instance.m_StateIndex = 5;
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 7)
                {
                    if (DelayAutoOpen && !IsAutoOpen)
                    {
                        IsAutoOpen = true;
                    }
                    DelayAutoOpen = false;
                    if (__instance.m_StateTimer == 0f && __instance.m_Slider == 0f)
                    {
                        if (!Plugin.DisableSoundsValue)
                        {
                            SoundManager.PlayAudio("SFX_PercStarJingle3", 0.6f, 1f);
                            SoundManager.PlayAudio("SFX_Gift", 0.6f, 1f);
                        }
                    }
                    __instance.m_Slider += Time.deltaTime * (Plugin.SkipPackEndScreenValue ? Plugin.SpeedMultiplierValue : 1f);
                    if (!Plugin.SkipPackEndScreenValue)
                    {
                        if (__instance.m_Slider >= 0.05f)
                        {
                            __instance.m_Slider = 0f;
                            __instance.m_CardAnimList[(int)__instance.m_StateTimer].transform.position = __instance.m_ShowAllCardPosList[(int)__instance.m_StateTimer].position;
                            __instance.m_CardAnimList[(int)__instance.m_StateTimer].transform.rotation = __instance.m_ShowAllCardPosList[(int)__instance.m_StateTimer].rotation;
                            __instance.m_Card3dUIList[(int)__instance.m_StateTimer].gameObject.SetActive(true);
                            __instance.m_CardAnimList[(int)__instance.m_StateTimer].Play("OpenCardFinalReveal");
                            __instance.m_StateTimer += 1f;
                            if (__instance.m_StateTimer >= (float)__instance.m_Card3dUIList.Count)
                            {
                                __instance.m_StateTimer = 0f;
                                __instance.m_StateIndex++;
                                __instance.m_CardOpeningSequenceUI.StartShowTotalValue(__instance.m_TotalCardValue, __instance.m_HasFoilCard);
                                return false;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < __instance.m_CardAnimList.Count; i++)
                        {
                            __instance.m_CardAnimList[i].transform.position = __instance.m_ShowAllCardPosList[i].position;
                            __instance.m_CardAnimList[i].transform.rotation = __instance.m_ShowAllCardPosList[i].rotation;
                            __instance.m_Card3dUIList[i].gameObject.SetActive(true);
                        }
                        __instance.m_StateTimer = 0f;
                        __instance.m_StateIndex++;
                        __instance.m_CardOpeningSequenceUI.StartShowTotalValue(__instance.m_TotalCardValue, __instance.m_HasFoilCard);
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 8)
                {
                    __instance.m_StateTimer += Time.deltaTime * (Plugin.SkipPackEndScreenValue ? Plugin.SpeedMultiplierValue : 1f);
                    if (__instance.m_StateTimer >= 0.02f)
                    {
                        __instance.m_Slider = 0f;
                        if (Plugin.SkipPackEndScreenValue)
                        {
                            for (int i = 0; i < __instance.m_Card3dUIList.Count; i++)
                            {
                                __instance.m_Card3dUIList[i].m_NewCardIndicator.gameObject.SetActive(__instance.m_RolledCardDataList[i].isNew);
                            }
                        }
                        else
                        {
                            __instance.m_Card3dUIList[(int)__instance.m_StateTimer].m_NewCardIndicator.gameObject.SetActive(__instance.m_RolledCardDataList[(int)__instance.m_StateTimer].isNew);
                        }
                        __instance.m_StateTimer += 1f;
                        if (__instance.m_StateTimer >= (float)__instance.m_Card3dUIList.Count)
                        {
                            __instance.m_StateIndex++;
                            return false;
                        }
                    }
                }
                else if (__instance.m_StateIndex == 9)
                {
                    __instance.m_Slider += Time.deltaTime * (Plugin.SkipPackEndScreenValue ? Plugin.SpeedMultiplierValue : 1f);
                    if (__instance.m_Slider >= (Plugin.SkipPackEndScreenValue ? Plugin.PackResultsTimerValue * Plugin.SpeedMultiplierValue : Plugin.PackResultsTimerValue))
                    {
                        __instance.m_Slider = 0f;
                        __instance.m_StateIndex++;
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 10)
                {
                    if (__instance.m_IsAutoFire)
                    {
                        __instance.m_StateIndex++;
                        return false;
                    }
                }
                else if (__instance.m_StateIndex == 11)
                {
                    __instance.m_StateTimer += Time.deltaTime * 1f;
                    if (__instance.m_StateTimer >= 0.01f)
                    {
                        __instance.m_Slider = 0f;
                        __instance.m_IsScreenActive = false;
                        __instance.m_IsReadyToOpen = false;
                        __instance.m_CardPackAnimator.gameObject.SetActive(false);
                        __instance.m_CardOpeningUIGroup.SetActive(false);
                        __instance.m_CardOpeningSequenceUI.HideTotalValue();
                        CSingleton<InteractionPlayerController>.Instance.ExitLockMoveMode();
                        CSingleton<InteractionPlayerController>.Instance.OnExitOpenPackState();
                        __instance.m_CurrentItem.DisableItem();
                        __instance.m_CurrentItem = null;
                        int num6 = 0;
                        __instance.m_TotalCardValue = 0f;
                        __instance.m_TotalExpGained = 0;
                        bool isGet = false;
                        bool isGet2 = false;
                        for (int j = 0; j < __instance.m_RolledCardDataList.Count; j++)
                        {
                            int num7 = (int)((int)(__instance.m_RolledCardDataList[j].GetCardBorderType() + 1) * Mathf.CeilToInt((float)(__instance.m_RolledCardDataList[j].borderType + 1) / 2f));
                            if (__instance.m_RolledCardDataList[j].isFoil)
                            {
                                num7 *= 8;
                            }
                            __instance.m_TotalExpGained += num7;
                            if (__instance.m_RolledCardDataList[j].GetCardBorderType() == ECardBorderType.FullArt && __instance.m_RolledCardDataList[j].isFoil)
                            {
                                isGet = true;
                                if (__instance.m_RolledCardDataList[j].expansionType == ECardExpansionType.Ghost)
                                {
                                    isGet2 = true;
                                }
                            }
                            if (__instance.m_RolledCardDataList[j].isNew)
                            {
                                num6++;
                            }
                        }
                        if (__instance.m_TotalExpGained > 0)
                        {
                            CEventManager.QueueEvent(new CEventPlayer_AddShopExp(__instance.m_TotalExpGained, false));
                        }
                        for (int k = 0; k < __instance.m_CardAnimList.Count; k++)
                        {
                            __instance.m_CardAnimList[k].transform.localPosition = Vector3.zero;
                            __instance.m_CardAnimList[k].transform.localRotation = Quaternion.identity;
                            __instance.m_Card3dUIList[k].m_NewCardIndicator.gameObject.SetActive(false);
                            __instance.m_CardAnimList[k].Play("OpenCardDefaultPos");
                        }
                        if (CSingleton<InteractionPlayerController>.Instance.GetHoldItemCount() <= 0)
                        {
                            TutorialManager.SetGameUIVisible(true);
                            CenterDot.SetVisibility(true);
                            GameUIScreen.ResetEnterGoNextDayIndicatorVisible();
                            CSingleton<InteractionPlayerController>.Instance.m_BlackBGWorldUIFade.SetFadeOut(3f, 0f);
                            CSingleton<InteractionPlayerController>.Instance.m_CameraFOVController.StopLerpFOV();
                            __instance.m_IsAutoFireKeydown = false;
                            __instance.m_AutoFireTimer = 0f;
                        }
                        CSingleton<InteractionPlayerController>.Instance.EvaluateOpenCardPack();
                        TutorialManager.AddTaskValue(ETutorialTaskCondition.OpenPack, 1f);
                        CPlayerData.m_GameReportDataCollect.cardPackOpened = CPlayerData.m_GameReportDataCollect.cardPackOpened + 1;
                        CPlayerData.m_GameReportDataCollectPermanent.cardPackOpened = CPlayerData.m_GameReportDataCollectPermanent.cardPackOpened + 1;
                        AchievementManager.OnCardPackOpened(CPlayerData.m_GameReportDataCollectPermanent.cardPackOpened);
                        AchievementManager.OnGetFullArtFoil(isGet);
                        AchievementManager.OnGetFullArtGhostFoil(isGet2);
                        if (num6 > 0)
                        {
                            AchievementManager.OnCheckAlbumCardCount(CPlayerData.GetTotalCardCollectedAmount());
                        }
                        UnityAnalytic.OpenPack();
                        return false;
                    }
                }
                else
                {
                    if (__instance.m_StateIndex == 12)
                    {
                        __instance.m_IsScreenActive = false;
                        return false;
                    }
                    if (__instance.m_StateIndex == 101)
                    {
                        float stateTimer = __instance.m_StateTimer;
                        __instance.m_StateTimer += Time.deltaTime;
                        if (__instance.m_StateTimer >= 0.05f)
                        {
                            int num8 = UnityEngine.Random.Range(0, 3);
                            float num9 = 0.002f * (float)__instance.m_CurrentOpenedCardIndex;
                            float num10 = 0.001f * (float)__instance.m_CurrentOpenedCardIndex;
                            if (num8 == 0)
                            {
                                if (!Plugin.DisableSoundsValue)
                                {
                                    SoundManager.PlayAudio("SFX_CardReveal1", 0.6f + num10, 1f + num9);
                                }
                            }
                            else if (num8 == 1)
                            {
                                if (!Plugin.DisableSoundsValue)
                                {
                                    SoundManager.PlayAudio("SFX_CardReveal2", 0.6f + num10, 1f + num9);
                                }
                            }
                            else
                            {
                                if (!Plugin.DisableSoundsValue)
                                {
                                    SoundManager.PlayAudio("SFX_CardReveal3", 0.6f + num10, 1f + num9);
                                }
                            }
                            __instance.m_CurrentOpenedCardIndex++;
                            return false;
                        }
                    }
                    else
                    {
                        int stateIndex = __instance.m_StateIndex;
                    }
                }
                return false;
            }
        }
        public static void ToggleAutoOpen()
        {
            if (IsAutoOpen || DelayAutoOpen)
            {
                IsAutoOpen = false;
                DelayAutoOpen = false;
                autoOpenStatusText.text = autoOpenBaseText + "Disabled";
                autoOpenStatusText.color = Color.red;
                return;
            }
            IsAutoOpen = true;
            autoOpenStatusText.text = autoOpenBaseText + "Enabled";
            autoOpenStatusText.color = Color.green;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameUIScreen), nameof(GameUIScreen.Update))]
        public static void GameUIScreen_Update_Postfix(ref GameUIScreen __instance)
        {
            if (!Plugin.EnableModValue) return;

            if (holdItemCountText == null && __instance.m_DayText != null)
            {
                DuplicateUIComponents(__instance, ref holdItemCountText, "HoldItemCountUI");
                holdItemCountText.transform.localPosition = new Vector3(Plugin.HoldTextPositionX.Value, Plugin.HoldTextPositionY.Value, 0);
                holdItemCountText.fontSizeMax = Plugin.HoldTextSize.Value;
                holdItemCountText.fontSize = Plugin.HoldTextSize.Value;
                holdItemCountText.fontSizeMin = 1f;
                holdItemCountText.gameObject.SetActive(true);
            }
            if (autoOpenStatusText == null && __instance.m_DayText != null)
            {
                DuplicateUIComponents(__instance, ref autoOpenStatusText,"AutoOpenStatusUI");
                autoOpenStatusText.transform.localPosition = new Vector3(Plugin.AutoTextPositionX.Value, Plugin.AutoTextPositionY.Value, 0);
                autoOpenStatusText.text = autoOpenBaseText + (IsAutoOpen ? "Enabled" : "Disabled");
                autoOpenStatusText.fontSizeMax = Plugin.AutoTextSize.Value;
                autoOpenStatusText.fontSize = Plugin.AutoTextSize.Value;
                autoOpenStatusText.fontSizeMin = 1f;
                autoOpenStatusText.color = IsAutoOpen ? Color.green : Color.red;
                autoOpenStatusText.gameObject.SetActive(true);
            }
            if (InteractionPlayerController.instance.m_HoldItemList.Count <= 0)
            {
                holdItemCountText.text = "";
            }
            else if (InteractionPlayerController.instance.m_HoldItemList[0].m_ItemType.ToString().Contains("CardPack"))
            {
                holdItemCountText.text = "Held packs: " + InteractionPlayerController.instance.m_HoldItemList.Count.ToString();
            }
            return;
        }

        public static readonly string autoOpenBaseText = "Auto Open ";
        public static TextMeshProUGUI holdItemCountText;
        public static TextMeshProUGUI autoOpenStatusText;
        public static GameObject newGameObject;

        public static void DuplicateUIComponents(GameUIScreen gameUIScreen, ref TextMeshProUGUI tmpro, string name)
        {
            newGameObject = new GameObject(name);
            RectTransform newRectTransform;
            Transform newTransform = gameUIScreen.m_DayText.transform.GetComponentInParent<RectTransform>().transform.parent;
            newGameObject.transform.SetParent(newTransform, false);

            newRectTransform = newGameObject.AddComponent<RectTransform>();
            RectTransform originalRectTransform = gameUIScreen.m_DayText.transform.GetComponentInParent<RectTransform>();

            newRectTransform.anchorMin = originalRectTransform.anchorMin;
            newRectTransform.anchorMax = originalRectTransform.anchorMax;
            newRectTransform.pivot = originalRectTransform.pivot;
            newRectTransform.anchoredPosition = originalRectTransform.anchoredPosition;
            newRectTransform.sizeDelta = originalRectTransform.sizeDelta;

            newRectTransform.anchorMin = new Vector2(0, 0);
            newRectTransform.anchorMax = new Vector2(1, 1);
            newRectTransform.pivot = new Vector2(0, 0);
            newRectTransform.anchoredPosition = Vector2.zero;
            newRectTransform.sizeDelta = new Vector2(200, 100);

            tmpro = Object.Instantiate(gameUIScreen.m_DayText, newRectTransform);
            tmpro.text = "";
            tmpro.fontSizeMax = Plugin.HoldTextSize.Value;
            tmpro.fontSize = Plugin.HoldTextSize.Value;
            tmpro.fontSizeMin = 1f;
            tmpro.autoSizeTextContainer = false;
            tmpro.alignment = TextAlignmentOptions.Left;
        }

        public static void RemoveEmptyBoxes()
        {
            if (!Plugin.EnableModValue || !CSingleton<CGameManager>.Instance.m_IsGameLevel)
            {
                return;
            }

            List<InteractablePackagingBox_Item> boxList = Object.FindObjectsOfType<InteractablePackagingBox_Item>().ToList();

            for (int i = 0; i < boxList.Count; i++)
            {
                if (!boxList[i].m_IsStored)
                {
                    if (boxList[i].m_ItemCompartment.m_ItemAmount == 0 && boxList[i] != InteractionPlayerController.Instance.m_CurrentHoldingItemBox)
                    {
                        Plugin.L($"box {i} item type: {boxList[i].m_ItemType} item count: {boxList[i].m_ItemCompartment.m_ItemAmount}");
                        boxList[i].OnDestroyed();
                    }
                }
            }
            return;
        }
    }

    public class TransformData
    {
        public Vector3 LocalPosition { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public Vector3 Scale { get; set; }
    }
}
