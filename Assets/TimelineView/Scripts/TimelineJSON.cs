using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KingsleyPhD
{

    [Serializable]
    public class TimelineSlot
    {
        public string id;
    }


    [Serializable]
    public class TimelineJSON
    {
        public TimelineSlot[] slots;

        private string mFileName;

        public string FileName { get { return mFileName; } }

        public static TimelineJSON Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Debug.LogError("JSON file not found: " + fileName);
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(fileName);
                TimelineJSON instance = JsonUtility.FromJson<TimelineJSON>(jsonText);
                if (instance != null)
                {
                    instance.mFileName = fileName;
                    return instance;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to load JSON: " + ex.Message);
                return null;
            }

            return null;
        }

        public TimelineJSON(string fileName)
        {
            mFileName = fileName;
        }

        public void AddSlot(string id)
        {
            List<TimelineSlot> slotList = (slots != null && slots.Length > 0) ? slots.ToList() : new List<TimelineSlot>();
            slotList.Add(new TimelineSlot() { id = id });
            slots = slotList.ToArray();

            Save();
        }

        public void RemoveSlot(string id)
        {
            List<TimelineSlot> slotList = (slots != null && slots.Length > 0) ? slots.ToList() : new List<TimelineSlot>();
            slotList.RemoveAt(slotList.FindIndex(x => x.id == id));
            slots = slotList.ToArray();

            Save();
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(mFileName, JsonUtility.ToJson(this));
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to save JSON: " + ex.Message);
            }
        }
    }

}