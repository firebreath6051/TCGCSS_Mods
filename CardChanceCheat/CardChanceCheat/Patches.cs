using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace CardChanceCheat
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AchievementManager), nameof(AchievementManager.UnlockAchievement))]
        private static bool AchievementManager_UnlockAchievement_Prefix(ref AchievementManager __instance, string achievementID)
        {
            if (!Plugin.EnableMod.Value) return true;

            return false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(InteractionPlayerController), nameof(InteractionPlayerController.OnGameDataFinishLoaded))]
        private static void InteractionPlayerController_OnGameDataFinishLoaded_Postfix(ref InteractionPlayerController __instance)
        {
            Plugin.DataCollectPermanent = CPlayerData.m_GameReportDataCollectPermanent;
            Plugin.DataWasSaved = true;

            return;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardOpeningSequence), nameof(CardOpeningSequence.OpenScreen))]
        private static bool CardOpeningSequence_OpenScreen_Prefix(ref CardOpeningSequence __instance, ECollectionPackType collectionPackType, bool isMultiPack, bool isPremiumPack = false)
        {
            if (!Plugin.EnableMod.Value) return true;

            __instance.m_IsScreenActive = true;
            __instance.m_CollectionPackType = collectionPackType;
            __instance.m_IsNewlList.Clear();
            __instance.m_TotalExpGained = 0;
            __instance.m_TotalCardValue = 0f;
            __instance.m_HasFoilCard = false;
            if (isMultiPack)
            {
                __instance.m_RolledCardDataList.Clear();
                __instance.m_CardValueList.Clear();
                __instance.m_SecondaryRolledCardDataList.Clear();
                __instance.m_StateIndex = -1;
                __instance.GetPackContent(false, isPremiumPack, false, ECollectionPackType.None);
                __instance.GetPackContent(false, isPremiumPack, false, ECollectionPackType.None);
                __instance.GetPackContent(false, isPremiumPack, false, ECollectionPackType.None);
                __instance.GetPackContent(false, isPremiumPack, false, ECollectionPackType.None);
                __instance.GetPackContent(false, isPremiumPack, false, ECollectionPackType.None);
            }
            else
            {
                __instance.m_StateIndex = 0;
                __instance.GetPackContent(true, isPremiumPack, false, ECollectionPackType.None);
                ECardExpansionType cardExpansionType = InventoryBase.GetCardExpansionType(collectionPackType);
                if (cardExpansionType == ECardExpansionType.Tetramon || cardExpansionType == ECardExpansionType.Destiny)
                {
                    int num = UnityEngine.Random.Range(0, 10000);
                    bool isTetramon = false;
                    if (cardExpansionType == ECardExpansionType.Tetramon)
                    {
                        num = UnityEngine.Random.Range(0, 20000);
                        isTetramon = true;
                    }
                    if (num < Plugin.GhostChance.Value * (isTetramon ? 200 : 100) && CPlayerData.m_ShopLevel > 1)
                    {
                        __instance.GetPackContent(true, isPremiumPack, true, ECollectionPackType.GhostPack);
                        if (__instance.m_SecondaryRolledCardDataList.Count > 0)
                        {
                            if (Plugin.FullGhostPack.Value)
                            {
                                int rolledGhostCard;
                                for (int i = 0; i < __instance.m_RolledCardDataList.Count; i++)
                                {
                                    rolledGhostCard = UnityEngine.Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                    __instance.m_RolledCardDataList[i] = __instance.m_SecondaryRolledCardDataList[rolledGhostCard];
                                    __instance.m_CardValueList[i] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[rolledGhostCard]);
                                }
                            }
                            else
                            {
                                int num2 = UnityEngine.Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                __instance.m_RolledCardDataList[__instance.m_RolledCardDataList.Count - 1] = __instance.m_SecondaryRolledCardDataList[num2];
                                __instance.m_CardValueList[__instance.m_RolledCardDataList.Count - 1] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[num2]);
                            }
                        }
                    }
                }
            }
            __instance.m_CurrentOpenedCardIndex = 0;
            for (int i = 0; i < __instance.m_Card3dUIList.Count; i++)
            {
                __instance.m_Card3dUIList[i].gameObject.SetActive(false);
            }
            for (int j = 0; j < __instance.m_RolledCardDataList.Count; j++)
            {
                CPlayerData.AddCard(__instance.m_RolledCardDataList[j], 1);
                __instance.m_Card3dUIList[j].m_CardUI.SetCardUI(__instance.m_RolledCardDataList[j]);
                if (__instance.m_RolledCardDataList[j].monsterType > EMonsterType.None && CPlayerData.GetCardAmount(__instance.m_RolledCardDataList[j]) == 1)
                {
                    __instance.m_RolledCardDataList[j].isNew = true;
                    if (CSingleton<CGameManager>.Instance.m_OpenPackShowNewCard)
                    {
                        __instance.m_IsNewlList.Add(true);
                    }
                    else
                    {
                        __instance.m_IsNewlList.Add(false);
                    }
                }
                else
                {
                    __instance.m_RolledCardDataList[j].isNew = false;
                    __instance.m_IsNewlList.Add(false);
                }
            }
            CSingleton<InteractionPlayerController>.Instance.m_CollectionBinderFlipAnimCtrl.SetCanUpdateSort(true);
            if (isMultiPack)
            {
                __instance.m_StateTimer = 0f;
                __instance.m_StateTimer = 0f;
                __instance.m_StateIndex = 101;
                for (int k = 0; k < __instance.m_RolledCardDataList.Count; k++)
                {
                    __instance.m_Card3dUIList[k].m_CardUI.SetCardUI(__instance.m_RolledCardDataList[k]);
                    if (__instance.m_RolledCardDataList[k].monsterType > EMonsterType.None && CPlayerData.GetCardAmount(__instance.m_RolledCardDataList[k]) == 1)
                    {
                        __instance.m_RolledCardDataList[k].isNew = true;
                        if (CSingleton<CGameManager>.Instance.m_OpenPackShowNewCard)
                        {
                            __instance.m_IsNewlList.Add(true);
                        }
                        else
                        {
                            __instance.m_IsNewlList.Add(false);
                        }
                    }
                    else
                    {
                        __instance.m_RolledCardDataList[k].isNew = false;
                        __instance.m_IsNewlList.Add(false);
                    }
                }
                CEventManager.QueueEvent(new CEventPlayer_SetCanvasGroupVisibility(false));
            }

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardOpeningSequence), nameof(CardOpeningSequence.GetPackContent))]
        private static bool CardOpeningSequence_GetPackContent_Prefix(ref CardOpeningSequence __instance, bool clearList, bool isPremiumPack, bool isSecondaryRolledData = false, ECollectionPackType overrideCollectionPackType = ECollectionPackType.None)
        {
            if (!Plugin.EnableMod.Value) return true;

            if (clearList)
            {
                if (isSecondaryRolledData)
                {
                    __instance.m_SecondaryRolledCardDataList.Clear();
                }
                else
                {
                    __instance.m_RolledCardDataList.Clear();
                    __instance.m_CardValueList.Clear();
                }
            }
            List<EMonsterType> list = new List<EMonsterType>();
            List<EMonsterType> list2 = new List<EMonsterType>();
            List<EMonsterType> list3 = new List<EMonsterType>();
            List<EMonsterType> list4 = new List<EMonsterType>();
            List<EMonsterType> list5 = new List<EMonsterType>();
            ECardExpansionType cardExpansionType = InventoryBase.GetCardExpansionType(__instance.m_CollectionPackType);
            if (isSecondaryRolledData)
            {
                cardExpansionType = InventoryBase.GetCardExpansionType(overrideCollectionPackType);
            }
            List<EMonsterType> shownMonsterList = InventoryBase.GetShownMonsterList(cardExpansionType);
            CardUISetting cardUISetting = InventoryBase.GetCardUISetting(cardExpansionType);
            bool openPackCanUseRarity = cardUISetting.openPackCanUseRarity;
            bool openPackCanHaveDuplicate = cardUISetting.openPackCanHaveDuplicate;
            Plugin.L($"All possible Monsters in Ghost packs:");
            for (int i = 0; i < shownMonsterList.Count; i++)
            {
                MonsterData monsterData = InventoryBase.GetMonsterData(shownMonsterList[i]);
                EMonsterType monsterType = monsterData.MonsterType;
                ERarity rarity = monsterData.Rarity;
                list.Add(monsterType);
                if (rarity == ERarity.Legendary)
                {
                    list5.Add(monsterType);
                }
                else if (rarity == ERarity.Epic)
                {
                    list4.Add(monsterType);
                }
                else if (rarity == ERarity.Rare)
                {
                    list3.Add(monsterType);
                }
                else
                {
                    list2.Add(monsterType);
                }
                Plugin.L($"{i}: Name: {monsterData.Name}");
            }
            int commonCardCounter = 0;
            int rareCardCounter = 0;
            int epicCardCounter = 0;
            int legendaryCardCounter = 0;
            int commonCardsNum = 1;
            int rareCardsNum = 0;
            int epicCardsNum = 0;
            int legendaryCardsNum = 0;
            float rareChance = 10f; // used for non-tetramon card packs
            float epicChance = 2f; // used for non-tetramon card packs
            float legendaryChance = 0.1f; // used for non-tetramon card packs
            float foilChance = Plugin.FoilChance.Value;
            ECardBorderType borderType = ECardBorderType.Base;
            float firstEditionChance = Plugin.FirstEdChance.Value;
            float silverChance = Plugin.SilverChance.Value;
            float goldChance = Plugin.GoldChance.Value;
            float exChance = Plugin.EXChance.Value;
            float fullArtChance = Plugin.FullArtChance.Value;
            ERarity erarity = ERarity.Common;
            int maxCardsPerPack = 7;
            if (__instance.m_CollectionPackType == ECollectionPackType.RareCardPack || __instance.m_CollectionPackType == ECollectionPackType.DestinyRareCardPack)
            {
                commonCardsNum = 0;
                rareCardsNum = 7;
                rareChance += 45f;
                epicChance += 2f;
                legendaryChance += 1f;
            }
            else if (__instance.m_CollectionPackType == ECollectionPackType.EpicCardPack || __instance.m_CollectionPackType == ECollectionPackType.DestinyEpicCardPack)
            {
                commonCardsNum = 0;
                rareCardsNum = 1;
                epicCardsNum = 7;
                legendaryCardsNum = 0;
                rareChance += 65f;
                epicChance += 45f;
                legendaryChance += 3f;
            }
            else if (__instance.m_CollectionPackType == ECollectionPackType.LegendaryCardPack || __instance.m_CollectionPackType == ECollectionPackType.DestinyLegendaryCardPack)
            {
                commonCardsNum = 0;
                rareCardsNum = 0;
                epicCardsNum = 1;
                legendaryCardsNum = 7;
                rareChance += 65f;
                epicChance += 55f;
                legendaryChance += 35f;
            }
            else if (__instance.m_CollectionPackType == ECollectionPackType.BasicCardPack || __instance.m_CollectionPackType == ECollectionPackType.DestinyBasicCardPack)
            {
                commonCardsNum = 7;
            }
            int currentCardIndex = 0;
            while (currentCardIndex < maxCardsPerPack && list.Count > 0)
            {
                int rolledCard = UnityEngine.Random.Range(0, list.Count);
                if (legendaryCardsNum - legendaryCardCounter > 0 && list5.Count > 0)
                {
                    erarity = ERarity.Legendary;
                    legendaryCardCounter++;
                }
                else if (epicCardsNum - epicCardCounter > 0 && list4.Count > 0)
                {
                    erarity = ERarity.Epic;
                    epicCardCounter++;
                }
                else if (rareCardsNum - rareCardCounter > 0 && list3.Count > 0)
                {
                    erarity = ERarity.Rare;
                    rareCardCounter++;
                }
                else if (commonCardsNum - commonCardCounter > 0 && list2.Count > 0)
                {
                    erarity = ERarity.Common;
                    commonCardCounter++;
                }
                else
                {
                    int randomRoll = UnityEngine.Random.Range(0, 10000);
                    int legendaryAmountMax = 4 - rareCardCounter;
                    int epicAmountMax = 4 - epicCardCounter;
                    int rareAmountMax = 4 - legendaryCardCounter;
                    bool isRarityChosen = false;
                    if (!isRarityChosen && legendaryChance > 0f && list5.Count > 0 && rareAmountMax > 0)
                    {
                        int rollChance = Mathf.RoundToInt(legendaryChance * 100f);
                        if (randomRoll < rollChance)
                        {
                            isRarityChosen = true;
                            erarity = ERarity.Legendary;
                            legendaryCardCounter++;
                        }
                    }
                    if (!isRarityChosen && epicChance > 0f && list4.Count > 0 && epicAmountMax > 0)
                    {
                        int rollChance = Mathf.RoundToInt(epicChance * 100f);
                        if (randomRoll < rollChance)
                        {
                            isRarityChosen = true;
                            erarity = ERarity.Epic;
                            epicCardCounter++;
                        }
                    }
                    if (!isRarityChosen && rareChance > 0f && list3.Count > 0 && legendaryAmountMax > 0)
                    {
                        int rollChance = Mathf.RoundToInt(rareChance * 100f);
                        if (randomRoll < rollChance)
                        {
                            isRarityChosen = true;
                            erarity = ERarity.Rare;
                            rareCardCounter++;
                        }
                    }
                    if (!isRarityChosen)
                    {
                        erarity = ERarity.Common;
                        commonCardCounter++;
                    }
                }
                int monsterType2;
                if (openPackCanUseRarity)
                {
                    if (erarity == ERarity.Legendary)
                    {
                        rolledCard = UnityEngine.Random.Range(0, list5.Count);
                        monsterType2 = (int)list5[rolledCard];
                        if (!openPackCanHaveDuplicate)
                        {
                            list5.RemoveAt(rolledCard);
                        }
                    }
                    else if (erarity == ERarity.Epic)
                    {
                        rolledCard = UnityEngine.Random.Range(0, list4.Count);
                        monsterType2 = (int)list4[rolledCard];
                        if (!openPackCanHaveDuplicate)
                        {
                            list4.RemoveAt(rolledCard);
                        }
                    }
                    else if (erarity == ERarity.Rare)
                    {
                        rolledCard = UnityEngine.Random.Range(0, list3.Count);
                        monsterType2 = (int)list3[rolledCard];
                        if (!openPackCanHaveDuplicate)
                        {
                            list3.RemoveAt(rolledCard);
                        }
                    }
                    else
                    {
                        rolledCard = UnityEngine.Random.Range(0, list2.Count);
                        monsterType2 = (int)list2[rolledCard];
                        if (!openPackCanHaveDuplicate)
                        {
                            list2.RemoveAt(rolledCard);
                        }
                    }
                }
                else
                {
                    rolledCard = UnityEngine.Random.Range(0, list.Count);
                    monsterType2 = (int)list[rolledCard];
                    if (!openPackCanHaveDuplicate)
                    {
                        list.RemoveAt(rolledCard);
                    }
                }
                CardData cardData = new CardData();
                cardData.monsterType = (EMonsterType)monsterType2;
                if (UnityEngine.Random.Range(0, 10000) < Mathf.RoundToInt(foilChance * 100f))
                {
                    cardData.isFoil = true;
                    __instance.m_HasFoilCard = true;
                }
                else
                {
                    cardData.isFoil = false;
                }
                if (CPlayerData.m_TutorialIndex < 10 && CPlayerData.m_GameReportDataCollectPermanent.cardPackOpened == 0 && !__instance.m_HasFoilCard && currentCardIndex == maxCardsPerPack - 1)
                {
                    cardData.isFoil = true;
                    __instance.m_HasFoilCard = true;
                }
                bool isBorderTypeChosen = false;
                if (UnityEngine.Random.Range(0, 10000) < Mathf.RoundToInt(fullArtChance * 100f))
                {
                    borderType = ECardBorderType.FullArt;
                    isBorderTypeChosen = true;
                }
                if (!isBorderTypeChosen && UnityEngine.Random.Range(0, 10000) < Mathf.RoundToInt(exChance * 100f))
                {
                    borderType = ECardBorderType.EX;
                    isBorderTypeChosen = true;
                }
                if (!isBorderTypeChosen && UnityEngine.Random.Range(0, 10000) < Mathf.RoundToInt(goldChance * 100f))
                {
                    borderType = ECardBorderType.Gold;
                    isBorderTypeChosen = true;
                }
                if (!isBorderTypeChosen && UnityEngine.Random.Range(0, 10000) < Mathf.RoundToInt(silverChance * 100f))
                {
                    borderType = ECardBorderType.Silver;
                    isBorderTypeChosen = true;
                }
                if (!isBorderTypeChosen && UnityEngine.Random.Range(0, 10000) < Mathf.RoundToInt(firstEditionChance * 100f))
                {
                    borderType = ECardBorderType.FirstEdition;
                    isBorderTypeChosen = true;
                }
                if (!isBorderTypeChosen || cardExpansionType == ECardExpansionType.Ghost)
                {
                    borderType = ECardBorderType.Base;
                }
                cardData.borderType = borderType;
                cardData.expansionType = cardExpansionType;
                if (cardData.expansionType == ECardExpansionType.Tetramon)
                {
                    cardData.isDestiny = false;
                }
                else if (cardData.expansionType == ECardExpansionType.Destiny)
                {
                    cardData.isDestiny = true;
                }
                else if (cardData.expansionType == ECardExpansionType.Ghost)
                {
                    int ghostDestinyRand = UnityEngine.Random.Range(0, 100);
                    cardData.isDestiny = (ghostDestinyRand < 50);
                }
                else
                {
                    cardData.isDestiny = false;
                }
                if (isSecondaryRolledData)
                {
                    __instance.m_SecondaryRolledCardDataList.Add(cardData);
                }
                else
                {
                    __instance.m_RolledCardDataList.Add(cardData);
                    __instance.m_CardValueList.Add(CPlayerData.GetCardMarketPrice(cardData));
                }
                currentCardIndex++;
            }

            return false;
        }
    }
}
