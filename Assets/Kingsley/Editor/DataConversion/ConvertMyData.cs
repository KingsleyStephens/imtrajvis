using KingsleyStephens.SpatioTemporalDatabase;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MyCSVToJSONUtility : EditorWindow
{
    private TextAsset csvFile;

    [MenuItem("Tools/My CSV to JSON Converter")]
    public static void ShowWindow()
    {
        GetWindow<MyCSVToJSONUtility>("My CSV to JSON Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("My CSV to JSON Converter", EditorStyles.boldLabel);
        GUILayout.Space(10);

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

        GUILayout.Space(10);
        if (GUILayout.Button("Convert CSV to JSON"))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a CSV file.", "OK");
                return;
            }
            ConvertCSVToJSON(csvFile);
        }
    }

    private void ConvertCSVToJSON(TextAsset csv)
    {
        try
        {
            string[] lines = csv.text.Split('\n');
            if (lines.Length <= 1)
            {
                Debug.LogError("CSV file is empty or invalid.");
                return;
            }

            // Extract headers
            string[] headers = lines[0].Trim().Split(',');

            List<Dictionary<string, string>> dataList = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');
                Dictionary<string, string> entry = new Dictionary<string, string>();

                for (int j = 0; j < headers.Length && j < values.Length; j++)
                {
                    entry[headers[j]] = values[j];
                }

                dataList.Add(entry);
            }

            // Write to JSON
            string movingObjectRoot = "First person view"; // Participant name
            Dictionary<string, JsonSpatioTemporalDB> dbs = new Dictionary<string, JsonSpatioTemporalDB>();
            Dictionary<string, int> ids = new Dictionary<string, int>();
            int nextId = 0;

            string taskID = Path.GetFileName(csv.name);
            if (!dbs.ContainsKey(taskID))
            {
                JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(Path.Combine(Application.persistentDataPath, taskID + ".json"));
                db.CreateOrOpen();
                dbs.Add(taskID, db);

                if (!db.TableExsits(movingObjectRoot))
                {
                    db.CreateTable(movingObjectRoot);
                }
            }

            float dt = 0.1f;

            foreach (var d in dataList)
            {
                JsonSpatioTemporalDB stDb = dbs[taskID];

                string name = d[headers[2]];
                if (!ids.ContainsKey(name))
                {
                    ids[name] = nextId++;

                    stDb.UpdateTable(movingObjectRoot, ids[name], new SpatioTemporalDB.MovingObjectData(ids[name], -1, "MovingObject", name));
                    stDb.UpdateTable(movingObjectRoot, ids[name], dt);
                }

                stDb.UpdateTable(movingObjectRoot, ids[name], new SpatioTemporalDB.MovingObjectPositionUpdate(
                    new Vector3(float.Parse(d[headers[3]]), float.Parse(d[headers[4]]), float.Parse(d[headers[5]])),
                    Quaternion.Euler(float.Parse(d[headers[6]]), float.Parse(d[headers[7]]), float.Parse(d[headers[8]])), 0.0f, float.Parse(d[headers[1]])));
            }

            foreach (var k in dbs.Keys)
            {
                dbs[k].Save();
            }
            
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success", $"JSON files saved to:\n{Application.persistentDataPath}", "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error converting CSV to JSON: {ex.Message}");
        }
    }
}
