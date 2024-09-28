using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace PrintAllCards
{
    public class JsonHandler
    {
        public static string m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CardData.json");

        public static void SaveCardDataToFile(List<MonsterDataParams> data)
        {
            m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CardData.json");
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
            m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CardData.json");
            if (File.Exists(m_saveFilePath))
            {
                string jsonData = File.ReadAllText(m_saveFilePath);

                List<MonsterDataParams> data = JsonConvert.DeserializeObject<List<MonsterDataParams>>(jsonData);

                return data;
            }

            return null;
        }

        public static void WriteSaveDataToFile(CGameDataDupe data)
        {
            string m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "SaveData.json");
            string path = Application.persistentDataPath + "/savedGames_Release" + CGameManager.GetSaveLoadSlotSelectedIndex().ToString() + ".gd";
            if (File.Exists(path))
            {
                using FileStream fileStream = File.Open(path, FileMode.Open);
                CGameData cGameData = new CGameData();
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                if (fileStream.Length > 0L)
                {
                    try
                    {
                        fileStream.Position = 0L;
                        cGameData = (CGameData)binaryFormatter.Deserialize(fileStream);
                        Patches.GameDataDupe = new CGameDataDupe(cGameData);
                        string jsonData = JsonConvert.SerializeObject(Patches.GameDataDupe, Formatting.Indented, new JsonSerializerSettings
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });
                        Plugin.L(jsonData);
                        File.WriteAllText(m_saveFilePath, jsonData);
                        fileStream.Close();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Plugin.L(ex.Message);
                        fileStream.Close();
                        return;
                    }
                }
                fileStream.Close();
                return;
            }
            return;
        }
    }
}
