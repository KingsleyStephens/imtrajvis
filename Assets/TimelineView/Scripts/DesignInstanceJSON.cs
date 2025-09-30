using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KingsleyPhD
{

    [Serializable]
    public class DesignInstanceDimensions
    {
        public string name;
        public bool singular;
        public string[] options;
        public bool[] choices;
    }

    [Serializable]
    public class DesignInstanceCategory
    {
        public string name;
        public DesignInstanceDimensions[] dimensions;
    }


    [Serializable]
    public class DesignInstanceJSON
    {
        public DesignInstanceCategory[] categories;

        private string mFileName;

        public string FileName { get { return mFileName; } }

        public static DesignInstanceJSON Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Debug.LogError("JSON file not found: " + fileName);
                return null;
            }

            try
            {
                string jsonText = File.ReadAllText(fileName);
                DesignInstanceJSON instance = JsonUtility.FromJson<DesignInstanceJSON>(jsonText);
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

        public DesignInstanceJSON(string fileName, DesignSpaceJSON schema)
        {
            mFileName = fileName;

            List<DesignInstanceCategory> cats = new List<DesignInstanceCategory>();
            foreach (var c in schema.categories)
            {
                DesignInstanceCategory designInstanceCategory = new DesignInstanceCategory();
                designInstanceCategory.name = c.name;

                List<DesignInstanceDimensions> dims = new List<DesignInstanceDimensions>();
                foreach (var d in c.dimensions)
                {
                    DesignInstanceDimensions dimensions = new DesignInstanceDimensions();
                    dimensions.name = d.name;
                    dimensions.singular = d.singular;
                    dimensions.options = (string[])d.options.Clone();
                    dimensions.choices = new bool[dimensions.options.Length];
                    for (int i = 0; i < dimensions.choices.Length; i++)
                    {
                        if (i == 0)
                        {
                            dimensions.choices[i] = true;
                        }
                        else
                        {
                            dimensions.choices[i] = false;
                        }
                    }

                    dims.Add(dimensions);
                }
                designInstanceCategory.dimensions = dims.ToArray();

                cats.Add(designInstanceCategory);
            }

            categories = cats.ToArray();
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

        public bool Query(string category, string dimension, string option)
        {
            DesignInstanceCategory c = categories.FirstOrDefault(f => f.name == category);
            if (c != null)
            {
                DesignInstanceDimensions d = c.dimensions.FirstOrDefault(f => f.name == dimension);
                if (d != null)
                {
                    int index = Array.IndexOf(d.options, option);
                    if (index >= 0)
                    {
                        return d.choices[index];
                    }
                }
            }

            return false;
        }

        public void Set(string category, string dimension, string option, bool setValue)
        {
            DesignInstanceCategory c = categories.FirstOrDefault(f => f.name == category);
            if (c != null)
            {
                DesignInstanceDimensions d = c.dimensions.FirstOrDefault(f => f.name == dimension);
                if (d != null)
                {
                    if (!d.singular)
                    {
                        for (int i = 0; i < d.choices.Length; i++)
                        {
                            d.choices[i] = false;
                        }
                    }

                    int index = Array.IndexOf(d.options, option);
                    if (index >= 0)
                    {
                        d.choices[index] = setValue;

                        Save();
                    }
                }
            }
        }
    }

}