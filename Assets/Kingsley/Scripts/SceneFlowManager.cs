using KController;
using KVisualisation;
using Oculus.Interaction.Locomotion;
using SATestbed;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

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
    private GameObject launchUI;

    [SerializeField]
    private GameObject trajConfigUI;

    [SerializeField]
    private GameObject leftControllerUI;

    [SerializeField]
    private GameObject cameraRig;

    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private float positionCheckPeriod = 0.5f;
    private float lastPositionCheckTime = 0.0f;

    public VisSimController Controller { get; private set; }

    [Serializable]
    private class ViewConfig
    {
        public Vector3 position;
        public Vector3 scale;
    }

    
    public GameObject tableTopInteraction;

    public GameObject roomViewInteraction;

    [SerializeField]
    private ViewConfig[] tableTopViewConfig;
    [SerializeField]
    private ViewConfig[] roomViewConfig;

    private int mCurrentClient = 0;

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
        public float shadowDist = 400.0f;
    }


    private Dictionary<int, SmallMultipleGroup> mSmallMultiplesFreeFly = new Dictionary<int, SmallMultipleGroup>();
    private Dictionary<int, SmallMultipleGroup> mSmallMultiplesTabletop = new Dictionary<int, SmallMultipleGroup>();

    private List<GameObject> visClients = new List<GameObject>();


    private int mNumClients = 1;

    public int NumClientsUIOption { get { return mNumClients - 1; } set { mNumClients = value + 1; } }

    private List<VisClient.MovingObjectSettings> clientConfigs = new List<VisClient.MovingObjectSettings>();

    public float PlaybackSpeed { get { return Controller.Players[mCurrentClient].PlaybackSpeed; } set { Controller.Players[mCurrentClient].PlaybackSpeed = value; } }
    public float SimTime { get { return Controller.Players[mCurrentClient].SimTime; } set { Controller.Players[mCurrentClient].SimTime = value; } }
    public float ScrubTime { set { Controller.Players[mCurrentClient].ScrubTime(value); } }

    [SerializeField]
    private float speedChangeDelta = 0.01f;

    private bool mTimeSpeedControl = true;

    private List<float> mPrevPlaybackSpeed = new List<float>();
    private bool mIsPaused = false;

    public bool IncreaseTime { get; set; } = false;
    public bool DecreaseTime { get; set; } = false;

    public void ConfigDesignSpace(KingsleyPhD.DesignInstanceJSON designSpace)
    {
        //        public bool Mesh { get { return clientConfigs[mCurrentClient].mesh; } set { clientConfigs[mCurrentClient].mesh = value; ApplyCurrentConfig(); } }
        //public bool Trails { get { return clientConfigs[mCurrentClient].trail; } set { clientConfigs[mCurrentClient].trail = value; ApplyCurrentConfig(); } }
        //public bool Ghosting { get { return clientConfigs[mCurrentClient].ghosting; } set { clientConfigs[mCurrentClient].ghosting = value; ApplyCurrentConfig(); } }
        //public bool Density { get { return clientConfigs[mCurrentClient].density; } set { clientConfigs[mCurrentClient].density = value; ApplyCurrentConfig(); } }
        //public int TimeType { get { return (int)clientConfigs[mCurrentClient].timeType; } set { clientConfigs[mCurrentClient].timeType = (VisClient.MovingObjectSettings.TimeType)value; ApplyCurrentConfig(); } }

        foreach (var cat in designSpace.categories)
        {
            switch (cat.name)
            {
                case "Trajectories":
                    {
                        foreach (var dim in cat.dimensions)
                        {
                            switch (dim.name)
                            {
                                case "Object":
                                    {
                                        int index = 0;
                                        foreach (var opt in dim.options)
                                        {
                                            switch (opt)
                                            {
                                                case "None":
                                                    {
                                                        Mesh = !dim.choices[index];
                                                        break;
                                                    }
                                                case "Glyph":
                                                    {
                                                        Mesh = dim.choices[index];
                                                        break;
                                                    }
                                            }

                                            index++;
                                        }
                                        break;
                                    }
                                case "Path":
                                    {
                                        int index = 0;
                                        foreach (var opt in dim.options)
                                        {
                                            switch (opt)
                                            {
                                                case "Trail":
                                                    {
                                                        Trails = dim.choices[index];
                                                        break;
                                                    }
                                                case "Glyph":
                                                    {
                                                        Ghosting = dim.choices[index];
                                                        break;
                                                    }
                                                case "Density":
                                                    {
                                                        Density = dim.choices[index];
                                                        break;
                                                    }
                                            }

                                            index++;
                                        }
                                        break;
                                    }
                            }
                        }
                        break;
                    }
                case "Space/Time":
                    {
                        foreach (var dim in cat.dimensions)
                        {
                            switch (dim.name)
                            {
                                case "Time":
                                    {
                                        int index = 0;
                                        foreach (var opt in dim.options)
                                        {
                                            switch (opt)
                                            {
                                                case "Static":
                                                    {
                                                        if (dim.choices[index])
                                                        {
                                                            TimeType = (int)VisClient.MovingObjectSettings.TimeType.Static;
                                                        }
                                                        break;
                                                    }
                                                case "+ 1D position":
                                                    {
                                                        break;
                                                    }
                                                case "+ 1D animation":
                                                    {
                                                        if (dim.choices[index])
                                                        {
                                                            TimeType = (int)VisClient.MovingObjectSettings.TimeType.Dynamic;
                                                        }
                                                        break;
                                                    }
                                            }

                                            index++;
                                        }
                                        break;
                                    }
                            }
                        }

                        break;
                    }
            }
        }


    }

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
                    currentConfig = tableTopViewConfig[mNumClients - 1];
                    if (tableTopInteraction != null)
                    {
                        tableTopInteraction.SetActive(false);
                    }
                    
                    break;
                }
            case ViewConfigType.Room:
                {
                    currentConfig = roomViewConfig[mNumClients - 1];
                    if (roomViewInteraction != null)
                    {
                        roomViewInteraction.SetActive(false);
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }

        mViewConfigType = config;

        ActivateCurrentViewConfig();
    }

    private void ActivateCurrentViewConfig()
    {
        switch (mViewConfigType)
        {
            case ViewConfigType.Tabletop:
                {
                    ViewConfig currentConfig = tableTopViewConfig[mNumClients - 1];
                    ResizeVis(currentConfig.position, currentConfig.scale);

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
                            newCam.ParentPivotPos(Vector3.zero, visRoot);
                            newCam.enabled = true;
                        }
                    }

                    if (roomViewInteraction != null)
                    {
                        roomViewInteraction.SetActive(false);
                    }
                    if (tableTopInteraction != null)
                    {
                        tableTopInteraction.SetActive(true);
                    }

                    break;
                }
            case ViewConfigType.Room:
                {
                    ViewConfig currentConfig = roomViewConfig[mNumClients - 1];
                    ResizeVis(currentConfig.position, currentConfig.scale);

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

                    if (tableTopInteraction != null)
                    {
                        tableTopInteraction.SetActive(false);
                    }

                    if (roomViewInteraction != null)
                    {
                        roomViewInteraction.SetActive(true);
                    }

                    break;
                }
            default:
                {
                    break;
                }
        }

        ApplyViewChange();
        ApplyConfigAll();

        var mult = GetMultipleConfig(visClients.Count);
        if (mult != null)
        {
            SetShadowDistance(mult.shadowDist);
        }
    }

    public void Resize()
    {
        ActivateCurrentViewConfig();
    }

    private void ResizeVis(Vector3 position, Vector3 scale)
    {
        //if (visRoot != null)
        //{
        //    visRoot.position = position;
        //    visRoot.localScale = scale;

        //    VisRoot root = visRoot.GetComponent<VisRoot>();
        //    if (root != null)
        //    {
        //        root.OnResize();
        //    }
        //}

        var mult = GetMultipleConfig(mNumClients);
        if (mult != null)
        {
            List<SmallMultipleConfig> smallMultipleConfigs = mult.config;

            for (int i = 0; i < visClients.Count; i++)
            {
                if (i < smallMultipleConfigs.Count)
                {
                    SmallMultipleConfig smConfig = smallMultipleConfigs[i];

                    var vis = visClients[i];
                    vis.transform.localPosition = smConfig.position;
                    vis.transform.localRotation = Quaternion.identity;
                    vis.transform.localScale = smConfig.scale;
                }
            }
        }

        ApplyConfigAll();
    }

    private IEnumerator DeferApplyConfig()
    {
        ApplyViewChange();
        yield return new WaitForEndOfFrame();
        ApplyConfigAll();
    }

    private void ResetTimeControls()
    {
        mTimeSpeedControl = true;
        for (int i = 0; i < mPrevPlaybackSpeed.Count; i++)
        {
            mPrevPlaybackSpeed[i] = 1.0f;
        }
        IncreaseTime = false;
        DecreaseTime = false;
        mIsPaused = false;
    }

    public void TimeControlToggle()
    {
        mTimeSpeedControl = !mTimeSpeedControl;

        if (mTimeSpeedControl)
        {
            PlaybackSpeed = mPrevPlaybackSpeed[mCurrentClient];
        }
        else
        {
            mPrevPlaybackSpeed[mCurrentClient] = PlaybackSpeed;
            PlaybackSpeed = 0.0f;
        }
    }

    public void SyncTime()
    {
        for (int i = 0; i < Controller.Players.Count; ++i)
        {
            if (i != mCurrentClient)
            {
                Controller.Players[i].SimTime = Controller.Players[mCurrentClient].SimTime;
                mPrevPlaybackSpeed[i] = mPrevPlaybackSpeed[mCurrentClient];
            }
        }
    }

    private void PauseToggle(bool paused)
    {
        mIsPaused = paused;
        if (mIsPaused)
        {
            for (int i = 0; i < Controller.Players.Count; ++i)
            {
                mPrevPlaybackSpeed[i] = Controller.Players[i].PlaybackSpeed;
                Controller.Players[i].PlaybackSpeed = 0.0f;
            }
        }
        else
        {
            for (int i = 0; i < Controller.Players.Count; ++i)
            {
                Controller.Players[i].PlaybackSpeed = mPrevPlaybackSpeed[i];
            }
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
        ChangeSimTime(mPrevPlaybackSpeed[mCurrentClient]);
    }
    public void DecreaseSimTime()
    {
        ChangeSimTime(-mPrevPlaybackSpeed[mCurrentClient]);
    }
    public void ChangeSimTime(float delta)
    {
        SimTime += delta;
    }


    private void Awake()
    {
        float height = 40.0f;
        float shadowDist = 50.0f;

        // Tabletop
        List<SmallMultipleConfig> config1 = new List<SmallMultipleConfig>();
        config1.Add(new SmallMultipleConfig(Vector3.one, Vector3.zero));
        mSmallMultiplesTabletop.Add(1, new SmallMultipleGroup() { config = config1, startPos = new Vector3(0.0f, height, 0.0f), shadowDist = shadowDist });

        float scale = 0.5f;
        float h = height * scale;
        float sd = shadowDist * scale;

        List<SmallMultipleConfig> config2 = new List<SmallMultipleConfig>();
        config2.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, 0.0f, 0.0f)));
        config2.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, 0.0f, 0.0f)));
        mSmallMultiplesTabletop.Add(2, new SmallMultipleGroup() { config = config2, startPos = new Vector3(0.0f, h, 0.0f), shadowDist = sd });

        scale = 0.25f;
        h = height * scale;
        sd = shadowDist * scale;

        List<SmallMultipleConfig> config3 = new List<SmallMultipleConfig>();
        config3.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, -visDimensions.y * scale * 0.25f, 0.0f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, -visDimensions.y * scale * 0.25f, 0.0f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(0.0f, visDimensions.y * scale * 0.25f, 0.0f)));
        mSmallMultiplesTabletop.Add(3, new SmallMultipleGroup() { config = config3, startPos = new Vector3(0.0f, h, 0.0f), shadowDist = sd });

        List<SmallMultipleConfig> config4 = new List<SmallMultipleConfig>();
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, -visDimensions.y * scale * 0.25f, 0.0f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, -visDimensions.y * scale * 0.25f, 0.0f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, visDimensions.y * scale * 0.25f, 0.0f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, visDimensions.y * scale * 0.25f, 0.0f)));
        mSmallMultiplesTabletop.Add(4, new SmallMultipleGroup() { config = config4, startPos = new Vector3(0.0f, h, 0.0f), shadowDist = sd });


        // FreeFly
        height = 40.0f;
        shadowDist = 300.0f;

        config1 = new List<SmallMultipleConfig>();
        config1.Add(new SmallMultipleConfig(Vector3.one, Vector3.zero));
        mSmallMultiplesFreeFly.Add(1, new SmallMultipleGroup() { config = config1, startPos = new Vector3(0.0f, height, 0.0f), shadowDist = shadowDist });

        scale = 0.5f;
        h = height * scale;
        sd = shadowDist * scale;

        config2 = new List<SmallMultipleConfig>();
        config2.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, 0.0f, 0.0f)));
        config2.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, 0.0f, 0.0f)));
        mSmallMultiplesFreeFly.Add(2, new SmallMultipleGroup() { config = config2, startPos = new Vector3(-visDimensions.x * scale * 0.5f, h, 0.0f), shadowDist = sd });

        scale = 0.25f;
        h = height;
        sd = shadowDist * scale;

        config3 = new List<SmallMultipleConfig>();
        config3.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, 0.0f, -visDimensions.z * scale * 0.5f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, 0.0f, -visDimensions.z * scale * 0.5f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(0.0f, 0.0f, visDimensions.z * scale * 0.5f)));
        mSmallMultiplesFreeFly.Add(3, new SmallMultipleGroup() { config = config3, startPos = new Vector3(-visDimensions.x * scale * 0.5f, h, -visDimensions.z * scale * 0.5f), shadowDist = sd });

        config4 = new List<SmallMultipleConfig>();
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, 0.0f, -visDimensions.z * scale * 0.5f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, 0.0f, -visDimensions.z * scale * 0.5f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(-visDimensions.x * scale * 0.5f, 0.0f, visDimensions.z * scale * 0.5f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * scale, new Vector3(visDimensions.x * scale * 0.5f, 0.0f, visDimensions.z * scale * 0.5f)));
        mSmallMultiplesFreeFly.Add(4, new SmallMultipleGroup() { config = config4, startPos = new Vector3(-visDimensions.x * scale * 0.5f, h, -visDimensions.z * scale * 0.5f), shadowDist = sd });
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

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
                    //if (launchUI != null)
                    //{
                    //    ActivateLaunchMenu();
                    //}
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
        MoveToClientDirection(1);
    }

    public void MoveToPrevClient()
    {
        MoveToClientDirection(-1);
    }

    private void MoveToClientDirection(int direction)
    {
        mCurrentClient = GetClosestClientId();

        int nextClient = mCurrentClient + direction;
        if (nextClient < 0)
        {
            nextClient = visClients.Count - 1;
        }
        else if (nextClient >= visClients.Count)
        {
            nextClient = 0;
        }

        MoveToClient(nextClient);
    }

    private float mLaunchTime = 0.0f;
    public void UIMoveToClient(int toClient)
    {
        if (launchUI != null && launchUI.activeSelf)
        {
            return;
        }

        if (Time.time - mLaunchTime > 0.5f)
        { 
            MoveToClient(toClient - 1);
        }
    }

    public void MoveToClient(int toClient)
    {
        if (!(toClient >= 0 && toClient < mNumClients))
        {
            return;
        }
        
        Vector3 basePos = Vector3.zero;
        switch (mViewConfigType)
        {
            case ViewConfigType.Tabletop:
                {
                    VROrbitCamera orbitCam = cameraRig.GetComponentInChildren<VROrbitCamera>();
                    if (orbitCam != null)
                    {
                        basePos = orbitCam.PivotPos;
                    }

                    break;
                }
            case ViewConfigType.Room:
                {
                    basePos = cameraRig.transform.position;

                    break;
                }
            default:
                {
                    break;
                }
        }


        Vector3 realtivePos = basePos - visClients[mCurrentClient].transform.position;
        Vector3 toPos = visClients[toClient].transform.position + realtivePos;

        switch (mViewConfigType)
        {
            case ViewConfigType.Tabletop:
                {
                    VROrbitCamera orbitCam = cameraRig.GetComponentInChildren<VROrbitCamera>();
                    if (orbitCam != null)
                    {
                        orbitCam.PivotPos = toPos;
                    }

                    break;
                }
            case ViewConfigType.Room:
                {
                    cameraRig.transform.position = toPos;

                    break;
                }
            default:
                {
                    break;
                }
        }

        mCurrentClient = toClient;
        UpdateControllerUI();
    }

    public void ActivateLaunchMenu()
    {
        return; // Disabled

        if (launchUI != null && launchUI.activeSelf)
        {
            return;
        }
        
        if (trajConfigUI != null && trajConfigUI.activeSelf)
        {
            trajConfigUI.SetActive(false);

            RoomView = true;
        }

        if (player != null)
        {
            Controller = player.GetComponent<VisSimController>();

            if (Controller != null)
            {
                Controller.StopPlay();
                Controller.UnloadSim();
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

        if (leftControllerUI != null)
        {
            leftControllerUI.SetActive(false);
        }

        if (launchUI != null)
        {
            launchUI.SetActive(true);
        }
    }

    public void Launch()
    {
        LaunchSim(mNumClients);
    }

    public void Launch(Transform root)
    {
        visRoot = root;

        if (player != null)
        {
            Controller = player.GetComponent<VisSimController>();
            if (Controller != null)
            {
                Controller.StopPlay();
                Controller.UnloadSim();
            }
            player.SetActive(false);
        }

        LaunchSim(1);
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


        if (Time.time - lastPositionCheckTime > positionCheckPeriod)
        {
            int newClient = GetClosestClientId();
            if (mCurrentClient != newClient)
            {
                mCurrentClient = newClient;
                UpdateControllerUI();
            }

            lastPositionCheckTime = Time.time;
        }
    }


    public int CurrentClientConfig { get { return mCurrentClient; } set { SetClientConfigUI(value, false);  } }

    private void UpdateControllerUI()
    {
        if (leftControllerUI != null)
        { 
            leftControllerUI.SetActive(true);

            TextMeshProUGUI[] labels = leftControllerUI.GetComponentsInChildren<TextMeshProUGUI>();
            if (labels.Length > 0)
            {
                var currentConfigLabel = labels.First((t) => t.name == "InstanceLabel");
                if (currentConfigLabel != null)
                {
                    currentConfigLabel.text = (mCurrentClient + 1).ToString();
                }
            }
        }
    }

    public void ShowConfigScreen()
    {
        // Toggle
        if (trajConfigUI.activeSelf)
        {
            CloseConfigScreen();
            return;
        }

        PauseToggle(true);
        mCurrentClient = GetClosestClientId();

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

        Toggle[] toggles = trajConfigUI.GetComponentsInChildren<Toggle>();
        if (toggles.Length > 0)
        {
            var toggleSearch = toggles.First((t) => t.name == "RoomScaleToggle");
            if (toggleSearch != null)
            {
                toggleSearch.isOn = mViewConfigType == ViewConfigType.Room;
            }
        }
    }

    private int GetClosestClientId()
    {
        int id = -1;

        List<Collider> visInstances = new List<Collider>();
        foreach (var c in visClients)
        {
            if (c != null)
            {
                Collider col = c.GetComponent<Collider>();
                if (col != null)
                {
                    visInstances.Add(col);
                }
            }
        }

        float closestDist = float.MaxValue;
        int closestId = 0;
        foreach (var instance in visInstances)
        {
            if (instance.bounds.Contains(mainCamera.transform.position))
            {
                id = instance.gameObject.GetComponent<Visualisation>().instanceId;
                break;
            }
            else
            {
                float dist = Vector3.Distance(mainCamera.transform.position, instance.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestId = instance.gameObject.GetComponent<Visualisation>().instanceId;
                }
            }
        }

        if (id == -1)
        {
            id = closestId;
        }

        return id;
    }


    private void SetClientConfigUI(int client, bool setClientDropdown = true)
    {
        MoveToClient(client);
        mCurrentClient = client;

        UpdateControllerUI();

        TMP_Dropdown[] dropDowns = trajConfigUI.GetComponentsInChildren<TMP_Dropdown>();
        if (dropDowns.Length > 0)
        {
            if (setClientDropdown)
            {
                var dropDownSearch = dropDowns.First((t) => t.name == "ClientInstanceDropdown");
                if (dropDownSearch != null)
                {
                    dropDownSearch.value = mCurrentClient;
                }
            }

            var dropDownSearch2 = dropDowns.First((t) => t.name == "TimeDropdown");
            if (dropDownSearch2 != null)
            {
                dropDownSearch2.value = TimeType;
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

        Slider[] sliders = trajConfigUI.GetComponentsInChildren<Slider>();
        if (sliders.Length > 0)
        {
            var sliderSearch = sliders.First((t) => t.name == "TimeSlider");
            if (sliderSearch != null)
            {
                sliderSearch.value = Controller.Players[mCurrentClient].GetSimTimeAsRelativeTime();
            }
        }
    }


    public bool Mesh { get { return clientConfigs[mCurrentClient].mesh; } set { clientConfigs[mCurrentClient].mesh = value; ApplyCurrentConfig(); } }
    public bool Trails { get { return clientConfigs[mCurrentClient].trail; } set { clientConfigs[mCurrentClient].trail = value; ApplyCurrentConfig(); } }
    public bool Ghosting { get { return clientConfigs[mCurrentClient].ghosting; } set { clientConfigs[mCurrentClient].ghosting = value; ApplyCurrentConfig(); } }
    public bool Density { get { return clientConfigs[mCurrentClient].density; } set { clientConfigs[mCurrentClient].density = value; ApplyCurrentConfig(); } }
    public int TimeType { get { return (int)clientConfigs[mCurrentClient].timeType; } set { clientConfigs[mCurrentClient].timeType = (VisClient.MovingObjectSettings.TimeType)value; ApplyCurrentConfig(); } }


    public void ApplyFilter(int filterId)
    {
        clientConfigs[mCurrentClient].filter = (VisClient.MovingObjectSettings.FilterType)filterId; 
        ApplyCurrentConfig(); 
        clientConfigs[mCurrentClient].filter = VisClient.MovingObjectSettings.FilterType.None;
    }

    public void ApplyCurrentConfig()
    {
        if (mCurrentClient >= 0 && mCurrentClient < visClients.Count)
        {
            ApplyClientConfig(visClients[mCurrentClient].GetComponent<Visualisation>(), clientConfigs[mCurrentClient]);
        }
    }

    public void ApplyConfigAll()
    {
        for (int i = 0; i < visClients.Count; i++)
        {
            ApplyClientConfig(visClients[i].GetComponent<Visualisation>(), clientConfigs[i]);
        }
    }

    public void ApplyViewChange()
    {
        for (int i = 0; i < visClients.Count; i++)
        {
            Visualisation vis = visClients[i].GetComponent<Visualisation>();
            if (vis != null)
            {
                vis.ApplyViewChange();
            }
        }
    }

    public void CloseConfigScreen()
    {
        PauseToggle(false);
        trajConfigUI.SetActive(false);
    }

    private SmallMultipleGroup GetMultipleConfig(int count)
    {
        switch (mViewConfigType)
        {
            case ViewConfigType.Tabletop:
                return mSmallMultiplesTabletop.ContainsKey(count) ? mSmallMultiplesTabletop[count] : null;
            case ViewConfigType.Room:
                return mSmallMultiplesFreeFly.ContainsKey(count) ? mSmallMultiplesFreeFly[count] : null;
            default:
                return null;
        }
    }

    public void LaunchSim(int clientCount)
    {
        //if (!(launchUI != null && launchUI.activeSelf))
        //{
        //    return;
        //}

        mNumClients = clientCount;

        // Local parameter setting
        ResetTimeControls();

        var mult = GetMultipleConfig(clientCount);
        if (mult != null)
        {
            //Vector3 startPos = mult.startPos;
            //if (cameraRig != null)
            //{
            //    cameraRig.transform.position = startPos;
            //    cameraRig.transform.rotation = Quaternion.identity;
            //}

            SetShadowDistance(mult.shadowDist);
        }

        // Load the simulation players
        if (Controller != null)
        {
            Controller.gameObject.SetActive(true);
            Controller.LoadSim(clientCount);
        }

        // Load the visualisation clients
        SpawnClients(clientCount, Controller, visRoot);

        // Configure current view
        ActivateCurrentViewConfig();
        UpdateControllerUI();

        // Start the simulation and playback running
        if (Controller != null)
        {
            foreach (var c in visClients)
            {
                c.SetActive(true);
                VisClient vc = c.GetComponentInChildren<VisClient>();
                if (vc != null)
                {
                    vc.OnStart();
                }
            }

            Controller.StartPlay();
        }

        // Hide launch UI
        //launchUI.SetActive(false);
        mLaunchTime = Time.time;
    }

    private void SpawnClients(int clientCount, VisSimController controller, Transform root)
    {
        if (visPrefab == null)
        {
            return; 
        }

        visClients.Clear();
        clientConfigs.Clear();
        mCurrentClient = 0;

        mPrevPlaybackSpeed.Clear();

        var mult = GetMultipleConfig(clientCount);
        if (mult != null)
        {
            List<SmallMultipleConfig> smallMultipleConfigs = mult.config;

            for (int i = 0; i < clientCount; i++)
            {
                if (i < smallMultipleConfigs.Count)
                {
                    SmallMultipleConfig smConfig = smallMultipleConfigs[i];

                    GameObject vis = Instantiate(visPrefab, root);
                    vis.transform.localPosition = smConfig.position;
                    vis.transform.localRotation = Quaternion.identity;
                    vis.transform.localScale = smConfig.scale;
                    visClients.Add(vis);

                    VisClient.MovingObjectSettings config = new VisClient.MovingObjectSettings();
                    clientConfigs.Add(config);
                    Visualisation visComponent = vis.GetComponent<Visualisation>();
                    if (visComponent != null)
                    {
                        visComponent.Configure(controller, i);

                        ApplyClientConfig(visComponent, config);

                        visComponent.ActivateVis();
                    }
                    mPrevPlaybackSpeed.Add(1.0f);
                }
            }

            SetShadowDistance(mult.shadowDist);
        }
    }

    private void ApplyClientConfig(Visualisation vis, VisClient.MovingObjectSettings config)
    {
        if (vis != null)
        {
            vis.ApplyConfig(config);
        }
    }

    private void SetShadowDistance(float distance)
    {
        UniversalRenderPipelineAsset urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            urpAsset.shadowDistance = distance;
        }
    }
}
