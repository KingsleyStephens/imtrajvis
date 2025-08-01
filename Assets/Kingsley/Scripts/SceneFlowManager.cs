using System;
using System.Collections.Generic;
using System.Linq;
using KController;
using KVisualisation;
using SATestbed;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Interaction.Locomotion;

public class SceneFlowManager : MonoBehaviour
{
    public enum RunType
    {
        Record,
        Play
    }

    [SerializeField]
    private RunType runType = RunType.Play;

    [SerializeField]
    private GameObject recorder;
    [SerializeField]
    private GameObject player;

    [SerializeField]
    private Transform visRoot;

    [SerializeField]
    private GameObject visPrefab;

    [SerializeField]
    private Vector3 visDimensions = new Vector3(1000.0f, 1000.0f, 1000.0f);
    [SerializeField]
    private float smallMultipleScale = 0.25f;

    [SerializeField]
    private GameObject launchUI;

    [SerializeField]
    private GameObject trajConfigUI;

    [SerializeField]
    private GameObject cameraRig;

    [SerializeField]
    private Camera mainCamera;

    public VisSimController Controller { get; private set; }

    [Serializable]
    private class ViewConfig
    {
        public Vector3 position;
        public Vector3 scale;
        public GameObject interaction;
    }

    [SerializeField]
    private ViewConfig tableTopViewConfig;
    [SerializeField]
    private ViewConfig roomViewConfig;

    public enum ViewConfigType
    {
        Tabletop,
        Room
    }

    private ViewConfigType mViewConfigType = ViewConfigType.Room;

    public bool TableTopView { get { return mViewConfigType == ViewConfigType.Tabletop; } set { SetCurrentViewConfig(ViewConfigType.Tabletop, value); } }
    public bool RoomView { get { return mViewConfigType == ViewConfigType.Room; } set { SetCurrentViewConfig(ViewConfigType.Room, value); } }

    public void SetCurrentViewConfig(ViewConfigType configType, bool on)
    {
        if (on)
        {
            ApplyViewConfig(configType);
        }
    }

    private class SmallMultipleConfig
    {
        public SmallMultipleConfig(Vector3 scale, Vector3 position)
        {
            this.scale = scale;
            this.position = position;
        }

        public Vector3 scale;
        public Vector3 position;
    }

    private class SmallMultipleGroup
    {
        public List<SmallMultipleConfig> config = new List<SmallMultipleConfig>();
        public Vector3 startPos = Vector3.zero;
    }


    private Dictionary<int, SmallMultipleGroup> mSmallMultiples = new Dictionary<int, SmallMultipleGroup>();

    private List<GameObject> visClients = new List<GameObject>();


    private int mNumClients = 1;

    public int NumClientsUIOption { get { return mNumClients - 1; } set { mNumClients = value + 1; } }

    private List<VisClient.MovingObjectSettings> clientConfigs = new List<VisClient.MovingObjectSettings>();

    public float PlaybackSpeed { get { return Controller.Player.PlaybackSpeed; } set { Controller.Player.PlaybackSpeed = value; } }
    public float SimTime { get { return Controller.Player.SimTime; } set { Controller.Player.SimTime = value; } }

    [SerializeField]
    private float speedChangeDelta = 0.01f;

    private bool mTimeSpeedControl = true;

    private float mPrevPlaybackSpeed = 1.0f;

    public bool IncreaseTime { get; set; } = false;
    public bool DecreaseTime { get; set; } = false;


    private void ApplyViewConfig(ViewConfigType config)
    {
        if (visRoot == null)
        {
            return;
        }

        ViewConfig currentConfig = null;
        switch (mViewConfigType)
        {
            case ViewConfigType.Tabletop:
                {
                    currentConfig = tableTopViewConfig;
                    break;
                }
            case ViewConfigType.Room:
                {
                    currentConfig = roomViewConfig;
                    break;
                }
            default:
                {
                    break;
                }
        }
        if (currentConfig != null)
        {
            currentConfig.interaction.SetActive(false);
        }

        mViewConfigType = config;

        ActivateCurrentViewConfig();
    }

    private void ActivateCurrentViewConfig()
    {
        if (visRoot == null)
        {
            return;
        }

        ViewConfig currentConfig = null;
        switch (mViewConfigType)
        {
            case ViewConfigType.Tabletop:
                {
                    currentConfig = tableTopViewConfig;

                    if (cameraRig != null)
                    {
                        VRFlyCamera oldCam = cameraRig.GetComponentInChildren<VRFlyCamera>();
                        if (oldCam != null)
                        {
                            oldCam.enabled = false;
                        }

                        VROrbitCamera newCam = cameraRig.GetComponentInChildren<VROrbitCamera>();
                        if (newCam != null)
                        {
                            newCam.SetPivotPos(Vector3.zero, visRoot);
                            newCam.enabled = true;
                        }
                    }

                    break;
                }
            case ViewConfigType.Room:
                {
                    currentConfig = roomViewConfig;


                    if (cameraRig != null)
                    {
                        VROrbitCamera oldCam = cameraRig.GetComponentInChildren<VROrbitCamera>();
                        if (oldCam != null)
                        {
                            oldCam.enabled = false;
                        }

                        VRFlyCamera newCam = cameraRig.GetComponentInChildren<VRFlyCamera>();
                        if (newCam != null)
                        {
                            newCam.enabled = true;
                        }
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
        if (currentConfig != null)
        {
            currentConfig.interaction.SetActive(true);
            visRoot.position = currentConfig.position;
            visRoot.localScale = currentConfig.scale;

            VisRoot root = visRoot.GetComponent<VisRoot>();
            if (root != null)
            {
                root.OnResize();
            }
        }
    }

    private void ResetTimeControls()
    {
        mTimeSpeedControl = true;
        mPrevPlaybackSpeed = 1.0f;
        IncreaseTime = false;
        DecreaseTime = false;
    }

    public void TimeControlToggle()
    {
        mTimeSpeedControl = !mTimeSpeedControl;

        if (mTimeSpeedControl)
        {
            PlaybackSpeed = mPrevPlaybackSpeed;
        }
        else
        {
            mPrevPlaybackSpeed = PlaybackSpeed;
            PlaybackSpeed = 0.0f;
        }
    }

    public void TimeIncrease()
    {
        if (mTimeSpeedControl)
        {
            IncreasePlaybackSpeed();
        }
        else
        {
            IncreaseSimTime();
        }
    }

    public void TimeDecrease()
    {
        if (mTimeSpeedControl)
        {
            DecreasePlaybackSpeed();
        }
        else
        {
            DecreaseSimTime();
        }
    }

    public void IncreasePlaybackSpeed()
    {
        ChangePlaybackSpeed(speedChangeDelta);
    }
    public void DecreasePlaybackSpeed()
    {
        ChangePlaybackSpeed(-speedChangeDelta);
    }

    public void ChangePlaybackSpeed(float delta)
    {
        PlaybackSpeed += delta;
    }


    public void IncreaseSimTime()
    {
        ChangeSimTime(mPrevPlaybackSpeed);
    }
    public void DecreaseSimTime()
    {
        ChangeSimTime(-mPrevPlaybackSpeed);
    }
    public void ChangeSimTime(float delta)
    {
        SimTime += delta;
    }


    private void Awake()
    {
        float height = 40.0f;

        List<SmallMultipleConfig> config1 = new List<SmallMultipleConfig>();
        config1.Add(new SmallMultipleConfig(Vector3.one, Vector3.zero));
        mSmallMultiples.Add(1, new SmallMultipleGroup() { config = config1, startPos = new Vector3(0.0f, height, 0.0f) });

        height *= smallMultipleScale;

        List<SmallMultipleConfig> config2 = new List<SmallMultipleConfig>();
        config2.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(-visDimensions.x * smallMultipleScale * 0.5f, 0.0f, 0.0f)));
        config2.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(visDimensions.x * smallMultipleScale * 0.5f, 0.0f, 0.0f)));
        mSmallMultiples.Add(2, new SmallMultipleGroup() { config = config2, startPos = new Vector3(0.0f, height, 0.0f) });

        List<SmallMultipleConfig> config3 = new List<SmallMultipleConfig>();
        config3.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(-visDimensions.x * smallMultipleScale * 0.5f, 0.0f, -visDimensions.z * smallMultipleScale * 0.5f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(visDimensions.x * smallMultipleScale * 0.5f, 0.0f, -visDimensions.z * smallMultipleScale * 0.5f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(0.0f, 0.0f, visDimensions.z * smallMultipleScale * 0.5f)));
        mSmallMultiples.Add(3, new SmallMultipleGroup() { config = config3, startPos = new Vector3(0.0f, height, 0.0f) });

        List<SmallMultipleConfig> config4 = new List<SmallMultipleConfig>();
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(-visDimensions.x * smallMultipleScale * 0.5f, 0.0f, -visDimensions.z * smallMultipleScale * 0.5f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(visDimensions.x * smallMultipleScale * 0.5f, 0.0f, -visDimensions.z * smallMultipleScale * 0.5f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(-visDimensions.x * smallMultipleScale * 0.5f, 0.0f, visDimensions.z * smallMultipleScale * 0.5f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScale, new Vector3(visDimensions.x * smallMultipleScale * 0.5f, 0.0f, visDimensions.z * smallMultipleScale * 0.5f)));
        mSmallMultiples.Add(4, new SmallMultipleGroup() { config = config4, startPos = new Vector3(0.0f, height, 0.0f) });
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (runType)
        {
            case RunType.Record:
                {
                    if (recorder != null)
                    {
                        recorder.SetActive(true);
                    }
                    break;
                }
            case RunType.Play:
                {
                    if (launchUI != null)
                    {
                        ActivateLaunchMenu();
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    public void MoveToNextClient()
    {
        MoveToClient(1);
    }

    public void MoveToPrevClient()
    {
        MoveToClient(-1);
    }


    private void MoveToClient(int direction)
    {
        int currentClient = -1;

        Collider[] overlaps = Physics.OverlapSphere(mainCamera.transform.position, 10.0f, 1 << LayerMask.NameToLayer("VisInstance"));
        if (overlaps.Length > 0)
        {
            Visualisation vis = overlaps[0].gameObject.GetComponent<Visualisation>();
            if (vis != null)
            {
                currentClient = vis.instanceId;
            }
        }
        else
        {
            RaycastHit hit;
            if (Physics.Raycast(mainCamera.transform.position, Vector3.down, out hit, 10000.0f, 1 << LayerMask.NameToLayer("VisInstance")))
            {
                Visualisation vis = hit.transform.gameObject.GetComponent<Visualisation>();
                if (vis != null)
                {
                    currentClient = vis.instanceId;
                }
            }
        }

        if (currentClient >= 0)
        {
            Vector3 realtivePos = cameraRig.transform.position - visClients[currentClient].transform.position;

            int nextClient = currentClient + direction;
            if (nextClient < 0)
            {
                nextClient = visClients.Count - 1;
            }
            else if (nextClient >= visClients.Count)
            {
                nextClient = 0;
            }
            cameraRig.transform.position = visClients[nextClient].transform.position + realtivePos;
        }
    }

    public void ActivateLaunchMenu()
    {
        if (trajConfigUI != null && trajConfigUI.activeSelf)
        {
            trajConfigUI.SetActive(false);

            mViewConfigType = ViewConfigType.Room;
        }

        if (player != null)
        {
            Controller = player.GetComponent<VisSimController>();

            if (Controller != null)
            {
                Controller.StopPlay();
            }
            player.SetActive(false);
        }

        foreach (var vis in visClients)
        {
            Destroy(vis);
        }
        visClients.Clear();
        clientConfigs.Clear();
        mCurrentClient = 0;

        if (launchUI != null)
        {
            launchUI.SetActive(true);
        }
    }

    public void Launch()
    {
        LaunchSim(mNumClients);
        launchUI.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
        if (IncreaseTime)
        {
            TimeIncrease();
        }
        if (DecreaseTime)
        {
            TimeDecrease();
        }

        /*        if (NumClients > 0 && trajConfigUI != null && !trajConfigUI.activeSelf && Mouse.current.rightButton.wasPressedThisFrame)
                {
                    if (mainCamera != null)
                    {
                        Vector2 screenPos = new Vector2(Mouse.current.position.x.value, Mouse.current.position.y.value);

                        Ray ray = mainCamera.ScreenPointToRay(screenPos);

                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 10000.0f, 1 << LayerMask.NameToLayer("VisInstance")))
                        {
                            Visualisation vis = hit.transform.gameObject.GetComponent<Visualisation>();
                            if (vis != null)
                            {
                                ShowConfigScreen(vis.instanceId);
                            }
                        }

                    }
                }*/
    }

    private int mCurrentClient = 0;

    public int CurrentClientConfig { get { return mCurrentClient; } set { mCurrentClient = value; SetClientConfigUI(mCurrentClient);  } }


    public void ShowConfigScreen()
    {
        if (trajConfigUI.activeSelf)
        {
            return;
        }

        trajConfigUI.SetActive(true);

        TMP_Dropdown[] dropDowns = trajConfigUI.GetComponentsInChildren<TMP_Dropdown>();
        if (dropDowns.Length > 0)
        {
            var dropDownSearch = dropDowns.First((t) => t.name == "ClientInstanceDropdown");
            if (dropDownSearch != null)
            {
                dropDownSearch.options.Clear();

                for (int i = 0; i < mNumClients; i++)
                {
                    dropDownSearch.options.Add(new TMP_Dropdown.OptionData((i + 1).ToString()));
                }

                dropDownSearch.value = mCurrentClient;
                SetClientConfigUI(mCurrentClient);
            }
        }
        else
        {
            Debug.LogWarning("No Dropdowns found in trajConfigUI");
        }
    }

    private void SetClientConfigUI(int client)
    {
        TMP_Dropdown[] dropDowns = trajConfigUI.GetComponentsInChildren<TMP_Dropdown>();
        if (dropDowns.Length > 0)
        {
            var dropDownSearch = dropDowns.First((t) => t.name == "ClientInstanceDropdown");
            if (dropDownSearch != null)
            {
                dropDownSearch.value = mCurrentClient;
            }

            dropDownSearch = dropDowns.First((t) => t.name == "TimeDropdown");
            if (dropDownSearch != null)
            {
                dropDownSearch.value = TimeType;
            }
        }
        else
        {
            Debug.LogWarning("No Dropdowns found in trajConfigUI");
        }

        Toggle[] toggles = trajConfigUI.GetComponentsInChildren<Toggle>();
        if (toggles.Length > 0)
        {
            var toggleSearch = toggles.First((t) => t.name == "MeshToggle");
            if (toggleSearch != null)
            {
                toggleSearch.isOn = Mesh;
            }
            toggleSearch = toggles.First((t) => t.name == "TrailsToggle");
            if (toggleSearch != null)
            {
                toggleSearch.isOn = Trails;
            }
            toggleSearch = toggles.First((t) => t.name == "GhostingToggle");
            if (toggleSearch != null)
            {
                toggleSearch.isOn = Ghosting;
            }
            toggleSearch = toggles.First((t) => t.name == "DensityToggle");
            if (toggleSearch != null)
            {
                toggleSearch.isOn = Density;
            }
        }
        else
        {
            Debug.LogWarning("No Toggles found in trajConfigUI");
        }
    }


    public bool Mesh { get { return clientConfigs[mCurrentClient].mesh; } set { clientConfigs[mCurrentClient].mesh = value; } }
    public bool Trails { get { return clientConfigs[mCurrentClient].trail; } set { clientConfigs[mCurrentClient].trail = value; } }
    public bool Ghosting { get { return clientConfigs[mCurrentClient].ghosting; } set { clientConfigs[mCurrentClient].ghosting = value; } }
    public bool Density { get { return clientConfigs[mCurrentClient].density; } set { clientConfigs[mCurrentClient].density = value; } }
    public int TimeType { get { return (int)clientConfigs[mCurrentClient].timeType; } set { clientConfigs[mCurrentClient].timeType = (VisClient.MovingObjectSettings.TimeType)value; } }


    public void ApplyCurrentConfig()
    {
        ApplyClientConfig(visClients[mCurrentClient].GetComponent<Visualisation>(), clientConfigs[mCurrentClient]);
    }

    public void CloseConfigScreen()
    {
        trajConfigUI.SetActive(false);
    }

    public void LaunchSim(int clientCount)
    {
        ResetTimeControls();

        ActivateCurrentViewConfig();
        SpawnClients(clientCount);

        Vector3 startPos = mSmallMultiples[clientCount].startPos;
        if (cameraRig != null)
        {
            cameraRig.transform.position = startPos;
            cameraRig.transform.rotation = Quaternion.identity;
        }

        if (player != null)
        {
            player.SetActive(true);

            if (Controller != null)
            {
                foreach (var client in visClients)
                {
                    Visualisation vis = client.GetComponent<Visualisation>();
                    if (vis != null)
                    {
                        vis.controller = Controller;
                    }
                    client.SetActive(true);
                }

                Controller.StartPlay();
            }
        }
    }

    private void SpawnClients(int clientCount)
    {
        if (visPrefab == null)
        {
            return; 
        }

        if (mSmallMultiples.ContainsKey(clientCount))
        {
            List<SmallMultipleConfig> smallMultipleConfigs = mSmallMultiples[clientCount].config;

            for (int i = 0; i < clientCount; i++)
            {
                if (i < smallMultipleConfigs.Count)
                {
                    SmallMultipleConfig smConfig = smallMultipleConfigs[i];

                    GameObject vis = Instantiate(visPrefab, visRoot);
                    vis.transform.localPosition = smConfig.position;
                    vis.transform.localRotation = Quaternion.identity;
                    vis.transform.localScale = smConfig.scale;
                    visClients.Add(vis);

                    VisClient.MovingObjectSettings config = new VisClient.MovingObjectSettings();
                    clientConfigs.Add(config);
                    Visualisation visComponent = vis.GetComponent<Visualisation>();
                    if (visComponent != null)
                    {
                        visComponent.instanceId = i;
                        ApplyClientConfig(visComponent, config);
                    }

/*                    InteractableUnityEventWrapper interaction = vis.GetComponent<InteractableUnityEventWrapper>();
                    if (interaction != null)
                    {
                        interaction.WhenSelect.AddListener(() => ShowConfigScreen(interaction.gameObject.GetComponent<Visualisation>().instanceId));
                    }*/
                }
            }
        }
    }

    private void ApplyClientConfig(Visualisation vis, VisClient.MovingObjectSettings config)
    {
        if (vis != null)
        {
            vis.ApplyConfig(config);
        }
    }
}
