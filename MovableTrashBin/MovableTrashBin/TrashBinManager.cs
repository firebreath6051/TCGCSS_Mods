using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace MovableTrashBin
{
    public class TrashBinManager
    {
        public static string m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TrashBinData_0.json");
        public TrashBinSaveData m_SaveData { get; set; }

        public static void SaveTrashBinData(TrashBinSaveData data, int saveSlot)
        {
            m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TrashBinData_" + saveSlot + ".json");
            string jsonData = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            File.WriteAllText(m_saveFilePath, jsonData);
        }

        public static TrashBinSaveData LoadTrashBinData(int saveSlot)
        {
            m_saveFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TrashBinData_" + saveSlot + ".json");
            if (File.Exists(m_saveFilePath))
            {
                string jsonData = File.ReadAllText(m_saveFilePath);

                TrashBinSaveData data = JsonConvert.DeserializeObject<TrashBinSaveData>(jsonData);

                return data;
            }

            return null;
        }
    }
}
