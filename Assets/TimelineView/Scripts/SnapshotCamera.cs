using UnityEngine;
using System.IO;

namespace KingsleyPhD
{
    public class SnapshotCamera : MonoBehaviour
    {
        public Camera cam;              // Assign your camera in the Inspector
        public int width = 1920;
        public int height = 1080;

        public void TakeSnapshot(string fileName)
        {
            // Create a temporary RenderTexture
            RenderTexture rt = new RenderTexture(width, height, 24);
            cam.targetTexture = rt;
            Texture2D snapshot = new Texture2D(width, height, TextureFormat.RGB24, false);

            // Render the camera to the RenderTexture
            cam.Render();

            // Set active and read pixels
            RenderTexture.active = rt;
            snapshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            snapshot.Apply();

            // Cleanup
            cam.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);

            // Encode to PNG (or JPG)
            byte[] bytes = snapshot.EncodeToPNG();
            File.WriteAllBytes(fileName, bytes);
        }
    }

}