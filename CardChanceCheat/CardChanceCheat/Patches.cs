using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace CardChanceCheat
{
    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardOpeningSequence), nameof(CardOpeningSequence.OpenScreen))]
        private static bool CardOpeningSequence_OpenScreen_Prefix(ref CardOpeningSequence __instance, ECollectionPackType collectionPackType, bool isMultiPack, bool isPremiumPack = false)
        {
            if (!Plugin.EnableMod.Value)
            {
                return true;
            }

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
                    int num = Random.Range(0, 10000);
                    bool isTetramon = false;
                    if (cardExpansionType == ECardExpansionType.Tetramon)
                    {
                        num = Random.Range(0, 20000);
                        isTetramon = true;
                    }
                    if (Plugin.PerPackChancesValue)
                    {
                        // BASIC
                        if (collectionPackType == ECollectionPackType.BasicCardPack || collectionPackType == ECollectionPackType.DestinyBasicCardPack)
                        {
                            if (num < Plugin.GhostChanceBasicValue * (isTetramon ? 200 : 100) && CPlayerData.m_ShopLevel > 1)
                            {
                                __instance.GetPackContent(true, isPremiumPack, true, ECollectionPackType.GhostPack);
                                if (__instance.m_SecondaryRolledCardDataList.Count > 0)
                                {
                                    if (Plugin.FullGhostPackValue)
                                    {
                                        int rolledGhostCard;
                                        for (int i = 0; i < __instance.m_RolledCardDataList.Count; i++)
                                        {
                                            rolledGhostCard = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                            __instance.m_RolledCardDataList[i] = __instance.m_SecondaryRolledCardDataList[rolledGhostCard];
                                            __instance.m_CardValueList[i] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[rolledGhostCard]);
                                        }
                                    }
                                    else
                                    {
                                        int num2 = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                        __instance.m_RolledCardDataList[__instance.m_RolledCardDataList.Count - 1] = __instance.m_SecondaryRolledCardDataList[num2];
                                        __instance.m_CardValueList[__instance.m_RolledCardDataList.Count - 1] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[num2]);
                                    }
                                }
                            }
                        }
                        // RARE
                        else if (collectionPackType == ECollectionPackType.RareCardPack || collectionPackType == ECollectionPackType.DestinyRareCardPack)
                        {
                            if (num < Plugin.GhostChanceRareValue * (isTetramon ? 200 : 100) && CPlayerData.m_ShopLevel > 1)
                            {
                                __instance.GetPackContent(true, isPremiumPack, true, ECollectionPackType.GhostPack);
                                if (__instance.m_SecondaryRolledCardDataList.Count > 0)
                                {
                                    if (Plugin.FullGhostPackValue)
                                    {
                                        int rolledGhostCard;
                                        for (int i = 0; i < __instance.m_RolledCardDataList.Count; i++)
                                        {
                                            rolledGhostCard = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                            __instance.m_RolledCardDataList[i] = __instance.m_SecondaryRolledCardDataList[rolledGhostCard];
                                            __instance.m_CardValueList[i] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[rolledGhostCard]);
                                        }
                                    }
                                    else
                                    {
                                        int num2 = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                        __instance.m_RolledCardDataList[__instance.m_RolledCardDataList.Count - 1] = __instance.m_SecondaryRolledCardDataList[num2];
                                        __instance.m_CardValueList[__instance.m_RolledCardDataList.Count - 1] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[num2]);
                                    }
                                }
                            }
                        }
                        // EPIC
                        else if (collectionPackType == ECollectionPackType.EpicCardPack || collectionPackType == ECollectionPackType.DestinyEpicCardPack)
                        {
                            if (num < Plugin.GhostChanceEpicValue * (isTetramon ? 200 : 100) && CPlayerData.m_ShopLevel > 1)
                            {
                                __instance.GetPackContent(true, isPremiumPack, true, ECollectionPackType.GhostPack);
                                if (__instance.m_SecondaryRolledCardDataList.Count > 0)
                                {
                                    if (Plugin.FullGhostPackValue)
                                    {
                                        int rolledGhostCard;
                                        for (int i = 0; i < __instance.m_RolledCardDataList.Count; i++)
                                        {
                                            rolledGhostCard = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                            __instance.m_RolledCardDataList[i] = __instance.m_SecondaryRolledCardDataList[rolledGhostCard];
                                            __instance.m_CardValueList[i] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[rolledGhostCard]);
                                        }
                                    }
                                    else
                                    {
                                        int num2 = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                        __instance.m_RolledCardDataList[__instance.m_RolledCardDataList.Count - 1] = __instance.m_SecondaryRolledCardDataList[num2];
                                        __instance.m_CardValueList[__instance.m_RolledCardDataList.Count - 1] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[num2]);
                                    }
                                }
                            }
                        }
                        // LEGEND
                        else if (collectionPackType == ECollectionPackType.LegendaryCardPack || collectionPackType == ECollectionPackType.DestinyLegendaryCardPack)
                        {
                            if (num < Plugin.GhostChanceLegendValue * (isTetramon ? 200 : 100) && CPlayerData.m_ShopLevel > 1)
                            {
                                __instance.GetPackContent(true, isPremiumPack, true, ECollectionPackType.GhostPack);
                                if (__instance.m_SecondaryRolledCardDataList.Count > 0)
                                {
                                    if (Plugin.FullGhostPackValue)
                                    {
                                        int rolledGhostCard;
                                        for (int i = 0; i < __instance.m_RolledCardDataList.Count; i++)
                                        {
                                            rolledGhostCard = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                            __instance.m_RolledCardDataList[i] = __instance.m_SecondaryRolledCardDataList[rolledGhostCard];
                                            __instance.m_CardValueList[i] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[rolledGhostCard]);
                                        }
                                    }
                                    else
                                    {
                                        int num2 = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                        __instance.m_RolledCardDataList[__instance.m_RolledCardDataList.Count - 1] = __instance.m_SecondaryRolledCardDataList[num2];
                                        __instance.m_CardValueList[__instance.m_RolledCardDataList.Count - 1] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[num2]);
                                    }
                                }
                            }
                        }
                        // NOT ANY OF THE ABOVE
                        else
                        {
                            if (num < Plugin.GhostChanceValue * (isTetramon ? 200 : 100) && CPlayerData.m_ShopLevel > 1)
                            {
                                __instance.GetPackContent(true, isPremiumPack, true, ECollectionPackType.GhostPack);
                                if (__instance.m_SecondaryRolledCardDataList.Count > 0)
                                {
                                    if (Plugin.FullGhostPackValue)
                                    {
                                        int rolledGhostCard;
                                        for (int i = 0; i < __instance.m_RolledCardDataList.Count; i++)
                                        {
                                            rolledGhostCard = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                            __instance.m_RolledCardDataList[i] = __instance.m_SecondaryRolledCardDataList[rolledGhostCard];
                                            __instance.m_CardValueList[i] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[rolledGhostCard]);
                                        }
                                    }
                                    else
                                    {
                                        int num2 = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                        __instance.m_RolledCardDataList[__instance.m_RolledCardDataList.Count - 1] = __instance.m_SecondaryRolledCardDataList[num2];
                                        __instance.m_CardValueList[__instance.m_RolledCardDataList.Count - 1] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[num2]);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (num < Plugin.GhostChanceValue * (isTetramon ? 200 : 100) && CPlayerData.m_ShopLevel > 1)
                        {
                            __instance.GetPackContent(true, isPremiumPack, true, ECollectionPackType.GhostPack);
                            if (__instance.m_SecondaryRolledCardDataList.Count > 0)
                            {
                                if (Plugin.FullGhostPackValue)
                                {
                                    int rolledGhostCard;
                                    for (int i = 0; i < __instance.m_RolledCardDataList.Count; i++)
                                    {
                                        rolledGhostCard = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                        __instance.m_RolledCardDataList[i] = __instance.m_SecondaryRolledCardDataList[rolledGhostCard];
                                        __instance.m_CardValueList[i] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[rolledGhostCard]);
                                    }
                                }
                                else
                                {
                                    int num2 = Random.Range(0, __instance.m_SecondaryRolledCardDataList.Count);
                                    __instance.m_RolledCardDataList[__instance.m_RolledCardDataList.Count - 1] = __instance.m_SecondaryRolledCardDataList[num2];
                                    __instance.m_CardValueList[__instance.m_RolledCardDataList.Count - 1] = CPlayerData.GetCardMarketPrice(__instance.m_SecondaryRolledCardDataList[num2]);
                                }
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

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(CardOpeningSequence), nameof(CardOpeningSequence.GetPackContent))]
        private static IEnumerable<CodeInstruction> CardOpeningSequence_GetPackContent_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            bool exWasAssigned = false;

            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_R4)
                {
                    if ((float)codes[i].operand == 5f)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, GetPackTypeSpecificValue("Foil")));
                        continue;
                    }
                    else if ((float)codes[i].operand == 20f)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, GetPackTypeSpecificValue("FirstEd")));
                        continue;
                    }
                    else if ((float)codes[i].operand == 8f)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, GetPackTypeSpecificValue("Silver")));
                        continue;
                    }
                    else if ((float)codes[i].operand == 4f)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, GetPackTypeSpecificValue("Gold")));
                        continue;
                    }
                    else if ((float)codes[i].operand == 1f && !exWasAssigned)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, GetPackTypeSpecificValue("EX")));
                        exWasAssigned = true;
                        continue;
                    }
                    else if ((float)codes[i].operand == 0.25f)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, GetPackTypeSpecificValue("FullArt")));
                        continue;
                    }
                }
            }
            return codes;
        }
        private static MethodInfo GetPackTypeSpecificValue(string chanceType)
        {
            return typeof(Patches).GetMethod($"{chanceType}ChanceValueForCurrentPack", BindingFlags.Static | BindingFlags.Public);
        }

        public static float FoilChanceValueForCurrentPack(CardOpeningSequence instance)
        {
            if (!Plugin.PerPackChancesValue) return Plugin.FoilChanceValue;

            switch (instance.m_CollectionPackType)
            {
                case ECollectionPackType.BasicCardPack:
                case ECollectionPackType.DestinyBasicCardPack:
                    return Plugin.FoilChanceBasicValue;
                case ECollectionPackType.RareCardPack:
                case ECollectionPackType.DestinyRareCardPack:
                    return Plugin.FoilChanceRareValue;
                case ECollectionPackType.EpicCardPack:
                case ECollectionPackType.DestinyEpicCardPack:
                    return Plugin.FoilChanceEpicValue;
                case ECollectionPackType.LegendaryCardPack:
                case ECollectionPackType.DestinyLegendaryCardPack:
                    return Plugin.FoilChanceLegendValue;
                default:
                    return Plugin.FoilChanceValue;  // Default to non-pack specific value if no matching type is found
            }
        }
        public static float FirstEdChanceValueForCurrentPack(CardOpeningSequence instance)
        {
            if (!Plugin.PerPackChancesValue) return Plugin.FirstEdChanceValue;

            switch (instance.m_CollectionPackType)
            {
                case ECollectionPackType.BasicCardPack:
                case ECollectionPackType.DestinyBasicCardPack:
                    return Plugin.FirstEdChanceBasicValue;
                case ECollectionPackType.RareCardPack:
                case ECollectionPackType.DestinyRareCardPack:
                    return Plugin.FirstEdChanceRareValue;
                case ECollectionPackType.EpicCardPack:
                case ECollectionPackType.DestinyEpicCardPack:
                    return Plugin.FirstEdChanceEpicValue;
                case ECollectionPackType.LegendaryCardPack:
                case ECollectionPackType.DestinyLegendaryCardPack:
                    return Plugin.FirstEdChanceLegendValue;
                default:
                    return Plugin.FirstEdChanceValue;  // Default to non-pack specific value if no matching type is found
            }
        }
        public static float SilverChanceValueForCurrentPack(CardOpeningSequence instance)
        {
            if (!Plugin.PerPackChancesValue) return Plugin.SilverChanceValue;

            switch (instance.m_CollectionPackType)
            {
                case ECollectionPackType.BasicCardPack:
                case ECollectionPackType.DestinyBasicCardPack:
                    return Plugin.SilverChanceBasicValue;
                case ECollectionPackType.RareCardPack:
                case ECollectionPackType.DestinyRareCardPack:
                    return Plugin.SilverChanceRareValue;
                case ECollectionPackType.EpicCardPack:
                case ECollectionPackType.DestinyEpicCardPack:
                    return Plugin.SilverChanceEpicValue;
                case ECollectionPackType.LegendaryCardPack:
                case ECollectionPackType.DestinyLegendaryCardPack:
                    return Plugin.SilverChanceLegendValue;
                default:
                    return Plugin.SilverChanceValue;  // Default to non-pack specific value if no matching type is found
            }
        }
        public static float GoldChanceValueForCurrentPack(CardOpeningSequence instance)
        {
            if (!Plugin.PerPackChancesValue) return Plugin.GoldChanceValue;

            switch (instance.m_CollectionPackType)
            {
                case ECollectionPackType.BasicCardPack:
                case ECollectionPackType.DestinyBasicCardPack:
                    return Plugin.GoldChanceBasicValue;
                case ECollectionPackType.RareCardPack:
                case ECollectionPackType.DestinyRareCardPack:
                    return Plugin.GoldChanceRareValue;
                case ECollectionPackType.EpicCardPack:
                case ECollectionPackType.DestinyEpicCardPack:
                    return Plugin.GoldChanceEpicValue;
                case ECollectionPackType.LegendaryCardPack:
                case ECollectionPackType.DestinyLegendaryCardPack:
                    return Plugin.GoldChanceLegendValue;
                default:
                    return Plugin.GoldChanceValue;  // Default to non-pack specific value if no matching type is found
            }
        }
        public static float EXChanceValueForCurrentPack(CardOpeningSequence instance)
        {
            if (!Plugin.PerPackChancesValue) return Plugin.EXChanceValue;

            switch (instance.m_CollectionPackType)
            {
                case ECollectionPackType.BasicCardPack:
                case ECollectionPackType.DestinyBasicCardPack:
                    return Plugin.EXChanceBasicValue;
                case ECollectionPackType.RareCardPack:
                case ECollectionPackType.DestinyRareCardPack:
                    return Plugin.EXChanceRareValue;
                case ECollectionPackType.EpicCardPack:
                case ECollectionPackType.DestinyEpicCardPack:
                    return Plugin.EXChanceEpicValue;
                case ECollectionPackType.LegendaryCardPack:
                case ECollectionPackType.DestinyLegendaryCardPack:
                    return Plugin.EXChanceLegendValue;
                default:
                    return Plugin.EXChanceValue;  // Default to non-pack specific value if no matching type is found
            }
        }
        public static float FullArtChanceValueForCurrentPack(CardOpeningSequence instance)
        {
            if (!Plugin.PerPackChancesValue) return Plugin.FullArtChanceValue;

            switch (instance.m_CollectionPackType)
            {
                case ECollectionPackType.BasicCardPack:
                case ECollectionPackType.DestinyBasicCardPack:
                    return Plugin.FullArtChanceBasicValue;
                case ECollectionPackType.RareCardPack:
                case ECollectionPackType.DestinyRareCardPack:
                    return Plugin.FullArtChanceRareValue;
                case ECollectionPackType.EpicCardPack:
                case ECollectionPackType.DestinyEpicCardPack:
                    return Plugin.FullArtChanceEpicValue;
                case ECollectionPackType.LegendaryCardPack:
                case ECollectionPackType.DestinyLegendaryCardPack:
                    return Plugin.FullArtChanceLegendValue;
                default:
                    return Plugin.FullArtChanceValue;  // Default to non-pack specific value if no matching type is found
            }
        }
    }
}
