using KingsleyStephens.SpatioTemporalDatabase;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LOTUSimCSVToJSONUtility : EditorWindow
{
    private TextAsset csvFile;

    // AUVSim reference centre — trajectories spawn here in the room
    // private static readonly Vector3 AUVSimCentre = new Vector3(-200f, 25f, -207f);
    private static readonly Vector3 AUVSimCentre = new Vector3(69f, 32f, 76f);
    // AUVSim approximate half-extent (fits comfortably in the room)
    // private const float TargetHalfExtent = 60f;
    private const float TargetHalfExtent = 350f;

    [MenuItem("Tools/LOTUSim CSV to JSON Converter")]
    public static void ShowWindow()
    {
        GetWindow<LOTUSimCSVToJSONUtility>("LOTUSim CSV to JSON Converter");
    }

    void OnGUI()
    {
        GUILayout.Label("LOTUSim CSV to JSON Converter", EditorStyles.boldLabel);
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

            string[] headers = lines[0].Trim().Split(',');

            int iName = System.Array.IndexOf(headers, "vessel_name");
            int iTime = System.Array.IndexOf(headers, "sim_time_s");
            int iPosX = System.Array.IndexOf(headers, "pos_x");
            int iPosY = System.Array.IndexOf(headers, "pos_y");
            int iPosZ = System.Array.IndexOf(headers, "pos_z");
            int iOriX = System.Array.IndexOf(headers, "orient_x");
            int iOriY = System.Array.IndexOf(headers, "orient_y");
            int iOriZ = System.Array.IndexOf(headers, "orient_z");
            int iOriW = System.Array.IndexOf(headers, "orient_w");

            if (iName < 0 || iTime < 0 || iPosX < 0 || iPosY < 0 || iPosZ < 0 ||
                iOriX < 0 || iOriY < 0 || iOriZ < 0 || iOriW < 0)
            {
                Debug.LogError("CSV is missing required columns. Check headers.");
                return;
            }

            // --- Pass 1: collect raw data ---
            Dictionary<string, List<(float time, Vector3 pos, Quaternion ori)>> vesselData
                = new Dictionary<string, List<(float, Vector3, Quaternion)>>();
            List<string> vesselOrder = new List<string>();

            Vector3 minPos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] v = line.Split(',');
                if (v.Length <= Mathf.Max(iName, iTime, iPosX, iPosY, iPosZ, iOriX, iOriY, iOriZ, iOriW))
                    continue;

                string name = v[iName].Trim();
                float  time = float.Parse(v[iTime]);
                Vector3 pos = new Vector3(
                    float.Parse(v[iPosX]),
                    float.Parse(v[iPosY]),
                    float.Parse(v[iPosZ]));
                Quaternion ori = new Quaternion(
                    float.Parse(v[iOriX]),
                    float.Parse(v[iOriY]),
                    float.Parse(v[iOriZ]),
                    float.Parse(v[iOriW]));

                if (!vesselData.ContainsKey(name))
                {
                    vesselData[name] = new List<(float, Vector3, Quaternion)>();
                    vesselOrder.Add(name);
                }
                vesselData[name].Add((time, pos, ori));

                // Track bounding box
                minPos = Vector3.Min(minPos, pos);
                maxPos = Vector3.Max(maxPos, pos);
            }

            // --- Compute normalisation ---
            Vector3 dataCentre = (minPos + maxPos) * 0.5f;
            Vector3 extents    = maxPos - minPos;
            float   maxExtent  = Mathf.Max(extents.x, extents.y, extents.z);

            // Avoid division by zero if all points are the same
            float scaleFactor = maxExtent > 0.001f ? TargetHalfExtent / (maxExtent * 0.5f) : 1f;

            Debug.Log($"LOTUSim: dataCentre={dataCentre}, maxExtent={maxExtent}, scaleFactor={scaleFactor}");

            // --- Build JSON ---
            string outputPath = Path.Combine(
                Application.persistentDataPath,
                "TrajectoryData",
                csv.name + ".json");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(outputPath);
            db.CreateOrOpen();

            string tableName = "session";
            if (!db.TableExsits(tableName))
                db.CreateTable(tableName);

            int nextId = 1;
            foreach (string vesselName in vesselOrder)
            {
                int id = nextId++;
                var points = vesselData[vesselName];

                // Calculate trajectoryDelta as average time gap
                float delta = 1.0f;
                if (points.Count > 1)
                {
                    float totalGap = 0f;
                    for (int i = 1; i < points.Count; i++)
                        totalGap += points[i].time - points[i - 1].time;
                    delta = totalGap / (points.Count - 1);
                }

                db.UpdateTable(tableName, id,
                    new SpatioTemporalDB.MovingObjectData(id, 0, "MovingObject", vesselName));
                db.UpdateTable(tableName, id, delta);

                foreach (var pt in points)
                {
                    // Centre, scale, then shift to AUVSim reference centre
                    Vector3 normPos = (pt.pos - dataCentre) * scaleFactor + AUVSimCentre;

                    db.UpdateTable(tableName, id,
                        new SpatioTemporalDB.MovingObjectPositionUpdate(
                            normPos,
                            pt.ori,
                            0.0f,
                            pt.time));
                }
            }

            db.Save();

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Success",
                $"{csv.name}.json saved to:\n{Path.GetDirectoryName(outputPath)}\n\n" +
                $"Scale factor applied: {scaleFactor:F4}\n" +
                $"Data centre: {dataCentre}", "OK");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LOTUSim conversion error: {ex.Message}\n{ex.StackTrace}");
        }
    }
}




// OLD CODE WITHOUT SCALLING


// using KingsleyStephens.SpatioTemporalDatabase;
// using System.Collections.Generic;
// using System.IO;
// using UnityEditor;
// using UnityEngine;

// public class LOTUSimCSVToJSONUtility : EditorWindow
// {
//     private TextAsset csvFile;

//     [MenuItem("Tools/LOTUSim CSV to JSON Converter")]
//     public static void ShowWindow()
//     {
//         GetWindow<LOTUSimCSVToJSONUtility>("LOTUSim CSV to JSON Converter");
//     }

//     void OnGUI()
//     {
//         GUILayout.Label("LOTUSim CSV to JSON Converter", EditorStyles.boldLabel);
//         GUILayout.Space(10);

//         csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);

//         GUILayout.Space(10);
//         if (GUILayout.Button("Convert CSV to JSON"))
//         {
//             if (csvFile == null)
//             {
//                 EditorUtility.DisplayDialog("Error", "Please assign a CSV file.", "OK");
//                 return;
//             }
//             ConvertCSVToJSON(csvFile);
//         }
//     }

//     private void ConvertCSVToJSON(TextAsset csv)
//     {
//         try
//         {
//             string[] lines = csv.text.Split('\n');
//             if (lines.Length <= 1)
//             {
//                 Debug.LogError("CSV file is empty or invalid.");
//                 return;
//             }

//             // Parse headers
//             string[] headers = lines[0].Trim().Split(',');

//             // Find column indices by name for robustness
//             int iName    = System.Array.IndexOf(headers, "vessel_name");
//             int iTime    = System.Array.IndexOf(headers, "sim_time_s");
//             int iPosX    = System.Array.IndexOf(headers, "pos_x");
//             int iPosY    = System.Array.IndexOf(headers, "pos_y");
//             int iPosZ    = System.Array.IndexOf(headers, "pos_z");
//             int iOriX    = System.Array.IndexOf(headers, "orient_x");
//             int iOriY    = System.Array.IndexOf(headers, "orient_y");
//             int iOriZ    = System.Array.IndexOf(headers, "orient_z");
//             int iOriW    = System.Array.IndexOf(headers, "orient_w");

//             if (iName < 0 || iTime < 0 || iPosX < 0 || iPosY < 0 || iPosZ < 0 ||
//                 iOriX < 0 || iOriY < 0 || iOriZ < 0 || iOriW < 0)
//             {
//                 Debug.LogError("CSV is missing required columns. Check headers.");
//                 return;
//             }

//             // Collect trajectory points per vessel, preserving insertion order
//             Dictionary<string, List<(float time, Vector3 pos, Quaternion ori)>> vesselData
//                 = new Dictionary<string, List<(float, Vector3, Quaternion)>>();
//             List<string> vesselOrder = new List<string>();

//             for (int i = 1; i < lines.Length; i++)
//             {
//                 string line = lines[i].Trim();
//                 if (string.IsNullOrWhiteSpace(line)) continue;

//                 string[] v = line.Split(',');
//                 if (v.Length <= Mathf.Max(iName, iTime, iPosX, iPosY, iPosZ, iOriX, iOriY, iOriZ, iOriW))
//                     continue;

//                 string name = v[iName].Trim();
//                 float  time = float.Parse(v[iTime]);
//                 Vector3 pos = new Vector3(
//                     float.Parse(v[iPosX]),
//                     float.Parse(v[iPosY]),
//                     float.Parse(v[iPosZ]));
//                 Quaternion ori = new Quaternion(
//                     float.Parse(v[iOriX]),
//                     float.Parse(v[iOriY]),
//                     float.Parse(v[iOriZ]),
//                     float.Parse(v[iOriW]));

//                 if (!vesselData.ContainsKey(name))
//                 {
//                     vesselData[name] = new List<(float, Vector3, Quaternion)>();
//                     vesselOrder.Add(name);
//                 }
//                 vesselData[name].Add((time, pos, ori));
//             }

//             // Build the JSON using JsonSpatioTemporalDB
//             string outputPath = Path.Combine(
//                 Application.persistentDataPath,
//                 "TrajectoryData",
//                 "LOTUSim.json");

//             Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

//             JsonSpatioTemporalDB db = new JsonSpatioTemporalDB(outputPath);
//             db.CreateOrOpen();

//             string tableName = "session";
//             if (!db.TableExsits(tableName))
//                 db.CreateTable(tableName);

//             // Assign stable integer IDs to each vessel
//             int nextId = 1;
//             foreach (string vesselName in vesselOrder)
//             {
//                 int id = nextId++;
//                 var points = vesselData[vesselName];

//                 // Calculate trajectoryDelta as average time gap for this vessel
//                 float delta = 1.0f; // fallback
//                 if (points.Count > 1)
//                 {
//                     float totalGap = 0f;
//                     for (int i = 1; i < points.Count; i++)
//                         totalGap += points[i].time - points[i - 1].time;
//                     delta = totalGap / (points.Count - 1);
//                 }

//                 db.UpdateTable(tableName, id,
//                     new SpatioTemporalDB.MovingObjectData(id, 0, "MovingObject", vesselName));
//                 db.UpdateTable(tableName, id, delta);

//                 foreach (var pt in points)
//                 {
//                     db.UpdateTable(tableName, id,
//                         new SpatioTemporalDB.MovingObjectPositionUpdate(
//                             pt.pos,
//                             pt.ori,
//                             0.0f,
//                             pt.time));
//                 }
//             }

//             db.Save();

//             AssetDatabase.Refresh();
//             EditorUtility.DisplayDialog("Success",
//                 $"LOTUSim.json saved to:\n{Path.GetDirectoryName(outputPath)}", "OK");
//         }
//         catch (System.Exception ex)
//         {
//             Debug.LogError($"LOTUSim conversion error: {ex.Message}\n{ex.StackTrace}");
//         }
//     }
// }