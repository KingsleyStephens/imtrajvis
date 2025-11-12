using System;
using System.IO;
using UnityEngine;

namespace KingsleyPhD
{

    [Serializable]
    public class DesignSpaceDimensions
    {
        public string name;
        public bool singular;
        public string[] options;
        public int defaultOption;
    }

    [Serializable]
    public class DesignSpaceCategory
    {
        public string name;
        public string icon;
        public DesignSpaceDimensions[] dimensions;
    }


    [Serializable]
    public class DesignSpaceJSON
    {
        public DesignSpaceCategory[] categories;

        public static DesignSpaceJSON Load(string json)
        {
            if (!File.Exists(json))
            {
                Debug.LogError("JSON file not found: " + json);
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(json);
                return JsonUtility.FromJson<DesignSpaceJSON>(jsonText);
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to load JSON: " + ex.Message);
                return null;
            }
        }
    }

}