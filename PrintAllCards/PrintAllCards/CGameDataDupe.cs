using System;
using System.Collections.Generic;

namespace PrintAllCards
{
    [Serializable]
    public class CGameDataDupe
    {
        public static CGameDataDupe instance;

        public int m_SaveIndex;

        public int m_SaveCycle;

        public bool m_CanCloudLoad;

        public DateTime m_LastLocalExitTime;

        public string m_LastLoginPlayfabID;

        public LightTimeData m_LightTimeData;

        public EGameEventFormat m_GameEventFormat;

        public EGameEventFormat m_PendingGameEventFormat;

        public ECardExpansionType m_GameEventExpansionType;

        public ECardExpansionType m_PendingGameEventExpansionType;

        public GameReportDataCollect m_GameReportDataCollect;

        public GameReportDataCollect m_GameReportDataCollectPermanent;

        public List<GameReportDataCollect> m_GameReportDataCollectPastList;

        public List<ShelfSaveData> m_ShelfSaveDataList;

        public List<WarehouseShelfSaveData> m_WarehouseShelfSaveDataList;

        public List<PackageBoxItemaveData> m_PackageBoxItemSaveDataList;

        public List<InteractableObjectSaveData> m_InteractableObjectSaveDataList;

        public List<CardShelfSaveData> m_CardShelfSaveDataList;

        public List<PlayTableSaveData> m_PlayTableSaveDataList;

        public List<AutoCleanserSaveData> m_AutoCleanserSaveDataList;

        public List<CustomerSaveData> m_CustomerSaveDataList;

        public List<WorkerSaveData> m_WorkerSaveDataList;

        public List<TutorialData> m_TutorialDataList;

        public List<BillData> m_BillList;

        public List<CardData> m_HoldCardDataList;

        public List<EItemType> m_HoldItemTypeList;

        public bool m_IsShopOpen;

        public bool m_IsShopOnceOpen;

        public bool m_IsWarehouseDoorClosed;

        public bool m_IsItemPriceGenerated;

        public bool m_IsCardPriceGenerated;

        public List<int> m_CurrentTotalItemCountList;

        public List<float> m_SetItemPriceList;

        public List<float> m_AverageItemCostList;

        public List<float> m_GeneratedCostPriceList;

        public List<float> m_GeneratedMarketPriceList;

        public List<float> m_ItemPricePercentChangeList;

        public List<FloatList> m_ItemPricePercentPastChangeList;

        public List<float> m_SetGameEventPriceList;

        public List<float> m_GeneratedGameEventPriceList;

        public List<float> m_GameEventPricePercentChangeList;

        public List<int> m_StockSoldList;

        public List<int> m_CollectionCardPackCountList;

        public List<int> m_CardCollectedList;

        public List<int> m_CardCollectedListDestiny;

        public List<int> m_CardCollectedListGhost;

        public List<int> m_CardCollectedListGhostBlack;

        public List<int> m_CardCollectedListMegabot;

        public List<int> m_CardCollectedListFantasyRPG;

        public List<int> m_CardCollectedListCatJob;

        public List<bool> m_IsCardCollectedList;

        public List<bool> m_IsCardCollectedListDestiny;

        public List<bool> m_IsCardCollectedListGhost;

        public List<bool> m_IsCardCollectedListGhostBlack;

        public List<bool> m_IsCardCollectedListMegabot;

        public List<bool> m_IsCardCollectedListFantasyRPG;

        public List<bool> m_IsCardCollectedListCatJob;

        public List<float> m_CardPriceSetList;

        public List<float> m_CardPriceSetListDestiny;

        public List<float> m_CardPriceSetListGhost;

        public List<float> m_CardPriceSetListGhostBlack;

        public List<float> m_CardPriceSetListMegabot;

        public List<float> m_CardPriceSetListFantasyRPG;

        public List<float> m_CardPriceSetListCatJob;

        public List<MarketPrice> m_GenCardMarketPriceList;

        public List<MarketPrice> m_GenCardMarketPriceListDestiny;

        public List<MarketPrice> m_GenCardMarketPriceListGhost;

        public List<MarketPrice> m_GenCardMarketPriceListGhostBlack;

        public List<MarketPrice> m_GenCardMarketPriceListMegabot;

        public List<MarketPrice> m_GenCardMarketPriceListFantasyRPG;

        public List<MarketPrice> m_GenCardMarketPriceListCatJob;

        public List<int> m_CollectionSortingMethodIndexList;

        public List<int> m_ChampionCardCollectedList;

        public List<bool> m_IsItemLicenseUnlocked;

        public List<bool> m_IsWorkerHired;

        public List<bool> m_IsAchievementUnlocked;

        public string m_PlayerName;

        public float m_CoinAmount;

        public int m_FamePoint;

        public int m_TotalFameAdd;

        public bool m_IsWarehouseRoomUnlocked;

        public int m_UnlockRoomCount;

        public int m_UnlockWarehouseRoomCount;

        public int m_CurrentDay;

        public int m_ShopExpPoint;

        public int m_ShopLevel;

        public int m_CloudSaveCountdown;

        public int m_TutorialIndex;

        public int m_TutorialSubgroupIndex;

        public bool m_HasFinishedTutorial;

        public bool m_IsMainGame;

        public float m_MusicVolumeDecrease;

        public float m_SoundVolumeDecrease;

        public int m_WorkbenchMinimumCardLimit;

        public float m_WorkbenchPriceLimit;

        public ERarity m_WorkbenchRarityLimit;

        public ECardExpansionType m_WorkbenchCardExpansionType;

        public string m_DebugString;

        public string m_DebugString2;

        public int m_DebugDataCount;
        public CGameDataDupe(CGameData cGameData)
        {
            if (instance == null)
            {
                instance = this;
            }

            this.m_SaveIndex = cGameData.m_SaveIndex;
            this.m_SaveCycle = cGameData.m_SaveCycle;
            this.m_CanCloudLoad = cGameData.m_CanCloudLoad;
            this.m_LastLocalExitTime = cGameData.m_LastLocalExitTime;
            this.m_LastLoginPlayfabID = cGameData.m_LastLoginPlayfabID;
            this.m_LightTimeData = cGameData.m_LightTimeData;
            this.m_GameEventFormat = cGameData.m_GameEventFormat;
            this.m_PendingGameEventFormat = cGameData.m_PendingGameEventFormat;
            this.m_GameEventExpansionType = cGameData.m_GameEventExpansionType;
            this.m_PendingGameEventExpansionType = cGameData.m_PendingGameEventExpansionType;
            this.m_GameReportDataCollect = cGameData.m_GameReportDataCollect;
            this.m_GameReportDataCollectPermanent = cGameData.m_GameReportDataCollectPermanent;
            this.m_GameReportDataCollectPastList = cGameData.m_GameReportDataCollectPastList;
            this.m_ShelfSaveDataList = cGameData.m_ShelfSaveDataList;
            this.m_WarehouseShelfSaveDataList = cGameData.m_WarehouseShelfSaveDataList;
            this.m_PackageBoxItemSaveDataList = cGameData.m_PackageBoxItemSaveDataList;
            this.m_InteractableObjectSaveDataList = cGameData.m_InteractableObjectSaveDataList;
            this.m_CardShelfSaveDataList = cGameData.m_CardShelfSaveDataList;
            this.m_PlayTableSaveDataList = cGameData.m_PlayTableSaveDataList;
            this.m_AutoCleanserSaveDataList = cGameData.m_AutoCleanserSaveDataList;
            this.m_CustomerSaveDataList = cGameData.m_CustomerSaveDataList;
            this.m_WorkerSaveDataList = cGameData.m_WorkerSaveDataList;
            this.m_TutorialDataList = cGameData.m_TutorialDataList;
            this.m_BillList = cGameData.m_BillList;
            this.m_HoldCardDataList = cGameData.m_HoldCardDataList;
            this.m_HoldItemTypeList = cGameData.m_HoldItemTypeList;
            this.m_IsShopOpen = cGameData.m_IsShopOpen;
            this.m_IsShopOnceOpen = cGameData.m_IsShopOnceOpen;
            this.m_IsWarehouseDoorClosed = cGameData.m_IsWarehouseDoorClosed;
            this.m_IsItemPriceGenerated = cGameData.m_IsItemPriceGenerated;
            this.m_IsCardPriceGenerated = cGameData.m_IsCardPriceGenerated;
            this.m_CurrentTotalItemCountList = cGameData.m_CurrentTotalItemCountList;
            this.m_SetItemPriceList = cGameData.m_SetItemPriceList;
            this.m_AverageItemCostList = cGameData.m_AverageItemCostList;
            this.m_GeneratedCostPriceList = cGameData.m_GeneratedCostPriceList;
            this.m_GeneratedMarketPriceList = cGameData.m_GeneratedMarketPriceList;
            this.m_ItemPricePercentChangeList = cGameData.m_ItemPricePercentChangeList;
            this.m_ItemPricePercentPastChangeList = cGameData.m_ItemPricePercentPastChangeList;
            this.m_SetGameEventPriceList = cGameData.m_SetGameEventPriceList;
            this.m_GeneratedGameEventPriceList = cGameData.m_GeneratedGameEventPriceList;
            this.m_GameEventPricePercentChangeList = cGameData.m_GameEventPricePercentChangeList;
            this.m_StockSoldList = cGameData.m_StockSoldList;
            this.m_CollectionCardPackCountList = cGameData.m_CollectionCardPackCountList;
            this.m_CardCollectedList = cGameData.m_CardCollectedList;
            this.m_CardCollectedListDestiny = cGameData.m_CardCollectedListDestiny;
            this.m_CardCollectedListGhost = cGameData.m_CardCollectedListGhost;
            this.m_CardCollectedListGhostBlack = cGameData.m_CardCollectedListGhostBlack;
            this.m_CardCollectedListMegabot = cGameData.m_CardCollectedListMegabot;
            this.m_CardCollectedListFantasyRPG = cGameData.m_CardCollectedListFantasyRPG;
            this.m_CardCollectedListCatJob = cGameData.m_CardCollectedListCatJob;
            this.m_IsCardCollectedList = cGameData.m_IsCardCollectedList;
            this.m_IsCardCollectedListDestiny = cGameData.m_IsCardCollectedListDestiny;
            this.m_IsCardCollectedListGhost = cGameData.m_IsCardCollectedListGhost;
            this.m_IsCardCollectedListGhostBlack = cGameData.m_IsCardCollectedListGhostBlack;
            this.m_IsCardCollectedListMegabot = cGameData.m_IsCardCollectedListMegabot;
            this.m_IsCardCollectedListFantasyRPG = cGameData.m_IsCardCollectedListFantasyRPG;
            this.m_IsCardCollectedListCatJob = cGameData.m_IsCardCollectedListCatJob;
            this.m_CardPriceSetList = cGameData.m_CardPriceSetList;
            this.m_CardPriceSetListDestiny = cGameData.m_CardPriceSetListDestiny;
            this.m_CardPriceSetListGhost = cGameData.m_CardPriceSetListGhost;
            this.m_CardPriceSetListGhostBlack = cGameData.m_CardPriceSetListGhostBlack;
            this.m_CardPriceSetListMegabot = cGameData.m_CardPriceSetListMegabot;
            this.m_CardPriceSetListFantasyRPG = cGameData.m_CardPriceSetListFantasyRPG;
            this.m_CardPriceSetListCatJob = cGameData.m_CardPriceSetListCatJob;
            this.m_GenCardMarketPriceList = cGameData.m_GenCardMarketPriceList;
            this.m_GenCardMarketPriceListDestiny = cGameData.m_GenCardMarketPriceListDestiny;
            this.m_GenCardMarketPriceListGhost = cGameData.m_GenCardMarketPriceListGhost;
            this.m_GenCardMarketPriceListGhostBlack = cGameData.m_GenCardMarketPriceListGhostBlack;
            this.m_GenCardMarketPriceListMegabot = cGameData.m_GenCardMarketPriceListMegabot;
            this.m_GenCardMarketPriceListFantasyRPG = cGameData.m_GenCardMarketPriceListFantasyRPG;
            this.m_GenCardMarketPriceListCatJob = cGameData.m_GenCardMarketPriceListCatJob;
            this.m_CollectionSortingMethodIndexList = cGameData.m_CollectionSortingMethodIndexList;
            this.m_ChampionCardCollectedList = cGameData.m_ChampionCardCollectedList;
            this.m_IsItemLicenseUnlocked = cGameData.m_IsItemLicenseUnlocked;
            this.m_IsWorkerHired = cGameData.m_IsWorkerHired;
            this.m_IsAchievementUnlocked = cGameData.m_IsAchievementUnlocked;
            this.m_PlayerName = cGameData.m_PlayerName;
            this.m_CoinAmount = cGameData.m_CoinAmount;
            this.m_FamePoint = cGameData.m_FamePoint;
            this.m_TotalFameAdd = cGameData.m_TotalFameAdd;
            this.m_IsWarehouseRoomUnlocked = cGameData.m_IsWarehouseRoomUnlocked;
            this.m_UnlockRoomCount = cGameData.m_UnlockRoomCount;
            this.m_UnlockWarehouseRoomCount = cGameData.m_UnlockWarehouseRoomCount;
            this.m_CurrentDay = cGameData.m_CurrentDay;
            this.m_ShopExpPoint = cGameData.m_ShopExpPoint;
            this.m_ShopLevel = cGameData.m_ShopLevel;
            this.m_CloudSaveCountdown = cGameData.m_CloudSaveCountdown;
            this.m_TutorialIndex = cGameData.m_TutorialIndex;
            this.m_TutorialSubgroupIndex = cGameData.m_TutorialSubgroupIndex;
            this.m_HasFinishedTutorial = cGameData.m_HasFinishedTutorial;
            this.m_IsMainGame = cGameData.m_IsMainGame;
            this.m_MusicVolumeDecrease = cGameData.m_MusicVolumeDecrease;
            this.m_SoundVolumeDecrease = cGameData.m_SoundVolumeDecrease;
            this.m_WorkbenchMinimumCardLimit = cGameData.m_WorkbenchMinimumCardLimit;
            this.m_WorkbenchPriceLimit = cGameData.m_WorkbenchPriceLimit;
            this.m_WorkbenchRarityLimit = cGameData.m_WorkbenchRarityLimit;
            this.m_WorkbenchCardExpansionType = cGameData.m_WorkbenchCardExpansionType;
            this.m_DebugString = cGameData.m_DebugString;
            this.m_DebugString2 = cGameData.m_DebugString2;
            this.m_DebugDataCount = cGameData.m_DebugDataCount;
        }
    }
}
