using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    public GameObject[] cameras;
    public float[] fog;
    private int currentCamera = 0;

    public GameObject CurrentCamera { get { return cameras[currentCamera]; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetCameraView(currentCamera);
    }

    // Update is called once per frame
    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.tabKey.wasPressedThisFrame)
        {
            SetCameraView((currentCamera + 1) % cameras.Length);
        }
    }

    private void SetCameraView(int cameraIndex)
    {
        cameras[currentCamera].gameObject.SetActive(false);
        currentCamera = cameraIndex;
        cameras[currentCamera].gameObject.SetActive(true);

        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
    }
}
