using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using CC;

namespace PrintAllCards
{
    public class JsonHandler
    {
        public static string m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CardData.json");

        public static void SaveCardDataToFile(List<MonsterDataParams> data)
        {
            try
            {
                string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                });

                File.WriteAllText(m_saveFilePath, jsonData);
            }
            catch (Exception ex)
            {
                Plugin.L(ex.Message);
            }
        }

        public static List<MonsterDataParams> LoadCardDataFromFile(int saveSlot)
        {
            if (File.Exists(m_saveFilePath))
            {
                string jsonData = File.ReadAllText(m_saveFilePath);

                List<MonsterDataParams> data = JsonConvert.DeserializeObject<List<MonsterDataParams>>(jsonData);

                return data;
            }

            return null;
        }
    }
}
