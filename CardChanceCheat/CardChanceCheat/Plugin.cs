using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
using System.Reflection;

namespace CardChanceCheat
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static readonly Harmony Harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource Log { get; set; }

        internal static bool DataWasSaved = false;
        internal static GameReportDataCollect DataCollectPermanent = default;
        internal static ConfigEntry<bool> EnableMod { get; private set; }
        internal static ConfigEntry<bool> PerPackChances { get; private set; }
        internal static ConfigEntry<bool> FullGhostPack { get; private set; }
        internal static ConfigEntry<float> FoilChance { get; private set; }
        internal static ConfigEntry<float> FirstEdChance { get; private set; }
        internal static ConfigEntry<float> SilverChance { get; private set; }
        internal static ConfigEntry<float> GoldChance { get; private set; }
        internal static ConfigEntry<float> EXChance { get; private set; }
        internal static ConfigEntry<float> FullArtChance { get; private set; }
        internal static ConfigEntry<float> GhostChance { get; private set; }

        // BASIC
        internal static ConfigEntry<float> FoilChanceBasic { get; private set; }
        internal static ConfigEntry<float> FirstEdChanceBasic { get; private set; }
        internal static ConfigEntry<float> SilverChanceBasic { get; private set; }
        internal static ConfigEntry<float> GoldChanceBasic { get; private set; }
        internal static ConfigEntry<float> EXChanceBasic { get; private set; }
        internal static ConfigEntry<float> FullArtChanceBasic { get; private set; }
        internal static ConfigEntry<float> GhostChanceBasic { get; private set; }

        // RARE
        internal static ConfigEntry<float> FoilChanceRare { get; private set; }
        internal static ConfigEntry<float> FirstEdChanceRare { get; private set; }
        internal static ConfigEntry<float> SilverChanceRare { get; private set; }
        internal static ConfigEntry<float> GoldChanceRare { get; private set; }
        internal static ConfigEntry<float> EXChanceRare { get; private set; }
        internal static ConfigEntry<float> FullArtChanceRare { get; private set; }
        internal static ConfigEntry<float> GhostChanceRare { get; private set; }

        // EPIC
        internal static ConfigEntry<float> FoilChanceEpic { get; private set; }
        internal static ConfigEntry<float> FirstEdChanceEpic { get; private set; }
        internal static ConfigEntry<float> SilverChanceEpic { get; private set; }
        internal static ConfigEntry<float> GoldChanceEpic { get; private set; }
        internal static ConfigEntry<float> EXChanceEpic { get; private set; }
        internal static ConfigEntry<float> FullArtChanceEpic { get; private set; }
        internal static ConfigEntry<float> GhostChanceEpic { get; private set; }

        // LEGENDARY
        internal static ConfigEntry<float> FoilChanceLegend { get; private set; }
        internal static ConfigEntry<float> FirstEdChanceLegend { get; private set; }
        internal static ConfigEntry<float> SilverChanceLegend { get; private set; }
        internal static ConfigEntry<float> GoldChanceLegend { get; private set; }
        internal static ConfigEntry<float> EXChanceLegend { get; private set; }
        internal static ConfigEntry<float> FullArtChanceLegend { get; private set; }
        internal static ConfigEntry<float> GhostChanceLegend { get; private set; }
        private void Awake()
        {
            Log = Logger;

            EnableMod = Config.Bind("1. Cheat Chances",
                                    "Enable mod",
                                    true,
                                    new ConfigDescription("Enable this mod.", null, new ConfigurationManagerAttributes { Order = 10 }));

            PerPackChances = Config.Bind("1. Cheat Chances",
                                    "Per pack type chances",
                                    false,
                                    new ConfigDescription("Enable configuration for chances per pack type.", null, new ConfigurationManagerAttributes { Order = 9 }));

            FullGhostPack = Config.Bind("1. Cheat Chances",
                                    "Full ghost pack",
                                    false,
                                    new ConfigDescription("If ghost chance is hit, all cards in the pack will be ghost cards.", null, new ConfigurationManagerAttributes { Order = 8 }));

            FoilChance = Config.Bind("1. Cheat Chances",
                                         "Foil chance",
                                         5f,
                                         new ConfigDescription("Chance for a card to be foil.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 7 }));

            FirstEdChance = Config.Bind("1. Cheat Chances",
                                         "First edition chance",
                                         20f,
                                         new ConfigDescription("Chance for a card to be first edition.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 6 }));

            SilverChance = Config.Bind("1. Cheat Chances",
                                         "Silver chance",
                                         8f,
                                         new ConfigDescription("Chance for a card to be silver.\nHas priority over: First Edition.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 5 }));

            GoldChance = Config.Bind("1. Cheat Chances",
                                         "Gold chance",
                                         4f,
                                         new ConfigDescription("Chance for a card to be gold.\nHas priority over: Silver, First Edition.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 4 }));

            EXChance = Config.Bind("1. Cheat Chances",
                                         "EX chance",
                                         1f,
                                         new ConfigDescription("Chance for a card to be EX.\nHas priority over: Gold, Silver, First Edition.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 3 }));

            FullArtChance = Config.Bind("1. Cheat Chances",
                                         "Full art chance",
                                         0.25f,
                                         new ConfigDescription("Chance for a card to be full art.\nHas priority over: EX, Gold, Silver, First Edition.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 2 }));

            GhostChance = Config.Bind("1. Cheat Chances",
                                         "Ghost chance",
                                         0.1f,
                                         new ConfigDescription("Base chance for a ghost card in the pack.\nThis is doubled for Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 1 }));

            // BASIC
            FoilChanceBasic = Config.Bind("2. Basic Pack Chances",
                                         "Foil chance",
                                         5f,
                                         new ConfigDescription("Chance for a card to be foil.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 7 }));

            FirstEdChanceBasic = Config.Bind("2. Basic Pack Chances",
                                         "First edition chance",
                                         20f,
                                         new ConfigDescription("Chance for a card to be first edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 6 }));

            SilverChanceBasic = Config.Bind("2. Basic Pack Chances",
                                         "Silver chance",
                                         8f,
                                         new ConfigDescription("Chance for a card to be silver.\nHas priority over: First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 5 }));

            GoldChanceBasic = Config.Bind("2. Basic Pack Chances",
                                         "Gold chance",
                                         4f,
                                         new ConfigDescription("Chance for a card to be gold.\nHas priority over: Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 4 }));

            EXChanceBasic = Config.Bind("2. Basic Pack Chances",
                                         "EX chance",
                                         1f,
                                         new ConfigDescription("Chance for a card to be EX.\nHas priority over: Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 3 }));

            FullArtChanceBasic = Config.Bind("2. Basic Pack Chances",
                                         "Full art chance",
                                         0.25f,
                                         new ConfigDescription("Chance for a card to be full art.\nHas priority over: EX, Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 2 }));

            GhostChanceBasic = Config.Bind("2. Basic Pack Chances",
                                         "Ghost chance",
                                         0.1f,
                                         new ConfigDescription("Base chance for a ghost card in the pack.\nThis is doubled for Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 1 }));

            // RARE
            FoilChanceRare = Config.Bind("3. Rare Pack Chances",
                                         "Foil chance",
                                         5f,
                                         new ConfigDescription("Chance for a card to be foil.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 7 }));

            FirstEdChanceRare = Config.Bind("3. Rare Pack Chances",
                                         "First edition chance",
                                         20f,
                                         new ConfigDescription("Chance for a card to be first edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 6 }));

            SilverChanceRare = Config.Bind("3. Rare Pack Chances",
                                         "Silver chance",
                                         8f,
                                         new ConfigDescription("Chance for a card to be silver.\nHas priority over: First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 5 }));

            GoldChanceRare = Config.Bind("3. Rare Pack Chances",
                                         "Gold chance",
                                         4f,
                                         new ConfigDescription("Chance for a card to be gold.\nHas priority over: Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 4 }));

            EXChanceRare = Config.Bind("3. Rare Pack Chances",
                                         "EX chance",
                                         1f,
                                         new ConfigDescription("Chance for a card to be EX.\nHas priority over: Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 3 }));

            FullArtChanceRare = Config.Bind("3. Rare Pack Chances",
                                         "Full art chance",
                                         0.25f,
                                         new ConfigDescription("Chance for a card to be full art.\nHas priority over: EX, Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 2 }));

            GhostChanceRare = Config.Bind("3. Rare Pack Chances",
                                         "Ghost chance",
                                         0.1f,
                                         new ConfigDescription("Base chance for a ghost card in the pack.\nThis is doubled for Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 1 }));

            // EPIC
            FoilChanceEpic = Config.Bind("4. Epic Pack Chances",
                                         "Foil chance",
                                         5f,
                                         new ConfigDescription("Chance for a card to be foil.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 7 }));

            FirstEdChanceEpic = Config.Bind("4. Epic Pack Chances",
                                         "First edition chance",
                                         20f,
                                         new ConfigDescription("Chance for a card to be first edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 6 }));

            SilverChanceEpic = Config.Bind("4. Epic Pack Chances",
                                         "Silver chance",
                                         8f,
                                         new ConfigDescription("Chance for a card to be silver.\nHas priority over: First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 5 }));

            GoldChanceEpic = Config.Bind("4. Epic Pack Chances",
                                         "Gold chance",
                                         4f,
                                         new ConfigDescription("Chance for a card to be gold.\nHas priority over: Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 4 }));

            EXChanceEpic = Config.Bind("4. Epic Pack Chances",
                                         "EX chance",
                                         1f,
                                         new ConfigDescription("Chance for a card to be EX.\nHas priority over: Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 3 }));

            FullArtChanceEpic = Config.Bind("4. Epic Pack Chances",
                                         "Full art chance",
                                         0.25f,
                                         new ConfigDescription("Chance for a card to be full art.\nHas priority over: EX, Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 2 }));

            GhostChanceEpic = Config.Bind("4. Epic Pack Chances",
                                         "Ghost chance",
                                         0.1f,
                                         new ConfigDescription("Base chance for a ghost card in the pack.\nThis is doubled for Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 1 }));

            // LEGENDARY
            FoilChanceLegend = Config.Bind("5. Legendary Pack Chances",
                                         "Foil chance",
                                         5f,
                                         new ConfigDescription("Chance for a card to be foil.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 7 }));

            FirstEdChanceLegend = Config.Bind("5. Legendary Pack Chances",
                                         "First edition chance",
                                         20f,
                                         new ConfigDescription("Chance for a card to be first edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 6 }));

            SilverChanceLegend = Config.Bind("5. Legendary Pack Chances",
                                         "Silver chance",
                                         8f,
                                         new ConfigDescription("Chance for a card to be silver.\nHas priority over: First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 5 }));

            GoldChanceLegend = Config.Bind("5. Legendary Pack Chances",
                                         "Gold chance",
                                         4f,
                                         new ConfigDescription("Chance for a card to be gold.\nHas priority over: Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 4 }));

            EXChanceLegend = Config.Bind("5. Legendary Pack Chances",
                                         "EX chance",
                                         1f,
                                         new ConfigDescription("Chance for a card to be EX.\nHas priority over: Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 3 }));

            FullArtChanceLegend = Config.Bind("5. Legendary Pack Chances",
                                         "Full art chance",
                                         0.25f,
                                         new ConfigDescription("Chance for a card to be full art.\nHas priority over: EX, Gold, Silver, First Edition.\nAffects Regular and Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 2 }));

            GhostChanceLegend = Config.Bind("5. Legendary Pack Chances",
                                         "Ghost chance",
                                         0.1f,
                                         new ConfigDescription("Base chance for a ghost card in the pack.\nThis is doubled for Destiny packs.", new AcceptableValueRange<float>(0, 100), new ConfigurationManagerAttributes { Order = 1 }));

        }

        private void OnEnable()
        {
            Harmony.PatchAll(Assembly.GetExecutingAssembly());
            L($"Plugin {PluginInfo.PLUGIN_NAME} is loaded! Made by WiseHorror (Nargacuga on Nexus)");
        }

        private void OnDisable()
        {
            Harmony.UnpatchSelf();
            L($"Plugin {PluginInfo.PLUGIN_NAME} is unloaded!");
        }
        private void Update()
        {

        }

        public static bool PerPackChancesValue
        {
            get { return PerPackChances.Value; }
        }
        public static float FoilChanceValue
        {
            get { return FoilChance.Value; }
        }
        public static float FirstEdChanceValue
        {
            get { return FirstEdChance.Value; }
        }
        public static float SilverChanceValue
        {
            get { return SilverChance.Value; }
        }
        public static float GoldChanceValue
        {
            get { return GoldChance.Value; }
        }
        public static float EXChanceValue
        {
            get { return EXChance.Value; }
        }
        public static float FullArtChanceValue
        {
            get { return FullArtChance.Value; }
        }
        public static float GhostChanceValue
        {
            get { return GhostChance.Value; }
        }
        public static bool FullGhostPackValue
        {
            get { return FullGhostPack.Value; }
        }

        // BASIC
        public static float FoilChanceBasicValue
        {
            get { return FoilChanceBasic.Value; }
        }
        public static float FirstEdChanceBasicValue
        {
            get { return FirstEdChanceBasic.Value; }
        }
        public static float SilverChanceBasicValue
        {
            get { return SilverChanceBasic.Value; }
        }
        public static float GoldChanceBasicValue
        {
            get { return GoldChanceBasic.Value; }
        }
        public static float EXChanceBasicValue
        {
            get { return EXChanceBasic.Value; }
        }
        public static float FullArtChanceBasicValue
        {
            get { return FullArtChanceBasic.Value; }
        }
        public static float GhostChanceBasicValue
        {
            get { return GhostChanceBasic.Value; }
        }

        // RARE
        public static float FoilChanceRareValue
        {
            get { return FoilChanceRare.Value; }
        }
        public static float FirstEdChanceRareValue
        {
            get { return FirstEdChanceRare.Value; }
        }
        public static float SilverChanceRareValue
        {
            get { return SilverChanceRare.Value; }
        }
        public static float GoldChanceRareValue
        {
            get { return GoldChanceRare.Value; }
        }
        public static float EXChanceRareValue
        {
            get { return EXChanceRare.Value; }
        }
        public static float FullArtChanceRareValue
        {
            get { return FullArtChanceRare.Value; }
        }
        public static float GhostChanceRareValue
        {
            get { return GhostChanceRare.Value; }
        }

        // EPIC
        public static float FoilChanceEpicValue
        {
            get { return FoilChanceEpic.Value; }
        }
        public static float FirstEdChanceEpicValue
        {
            get { return FirstEdChanceEpic.Value; }
        }
        public static float SilverChanceEpicValue
        {
            get { return SilverChanceEpic.Value; }
        }
        public static float GoldChanceEpicValue
        {
            get { return GoldChanceEpic.Value; }
        }
        public static float EXChanceEpicValue
        {
            get { return EXChanceEpic.Value; }
        }
        public static float FullArtChanceEpicValue
        {
            get { return FullArtChanceEpic.Value; }
        }
        public static float GhostChanceEpicValue
        {
            get { return GhostChanceEpic.Value; }
        }

        // LEGENDARY
        public static float FoilChanceLegendValue
        {
            get { return FoilChanceLegend.Value; }
        }
        public static float FirstEdChanceLegendValue
        {
            get { return FirstEdChanceLegend.Value; }
        }
        public static float SilverChanceLegendValue
        {
            get { return SilverChanceLegend.Value; }
        }
        public static float GoldChanceLegendValue
        {
            get { return GoldChanceLegend.Value; }
        }
        public static float EXChanceLegendValue
        {
            get { return EXChanceLegend.Value; }
        }
        public static float FullArtChanceLegendValue
        {
            get { return FullArtChanceLegend.Value; }
        }
        public static float GhostChanceLegendValue
        {
            get { return GhostChanceLegend.Value; }
        }

        internal static void L(string message, bool info = false)
        {
            if (info)
            {
                Log.LogInfo(message);
                return;
            }
            Log.LogWarning(message);
        }
    }
}
