using System;
using System.Collections.Generic;
using System.Linq;
using KController;
using KVisualisation;
using Oculus.Interaction;
using SATestbed;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

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
    private GameObject visPrefab;

    [SerializeField]
    private Vector3 visDimensions = new Vector3(1000.0f, 1000.0f, 1000.0f);
    [SerializeField]
    private float smallMultipleScaleStep = 0.25f;

    [SerializeField]
    private GameObject launchUI;

    [SerializeField]
    private GameObject trajConfigUI;

    [SerializeField]
    private Camera mainCamera;


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

    private Dictionary<int, List<SmallMultipleConfig>> mSmallMultiples = new Dictionary<int, List<SmallMultipleConfig>>();

    private List<GameObject> visClients = new List<GameObject>();

    public int NumClients { get; set; } = 0;

    private List<VisClient.MovingObjectSettings> clientConfigs = new List<VisClient.MovingObjectSettings>();

    private void Awake()
    {
        List<SmallMultipleConfig> config1 = new List<SmallMultipleConfig>();
        config1.Add(new SmallMultipleConfig(Vector3.one, Vector3.zero));
        mSmallMultiples.Add(1, config1);

        List<SmallMultipleConfig> config2 = new List<SmallMultipleConfig>();
        config2.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep, new Vector3(-visDimensions.x * smallMultipleScaleStep * 0.5f, 0.0f, visDimensions.z * smallMultipleScaleStep * 0.5f)));
        config2.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep, new Vector3(visDimensions.x * smallMultipleScaleStep * 0.5f, 0.0f, visDimensions.z * smallMultipleScaleStep * 0.5f)));
        mSmallMultiples.Add(2, config2);

        List<SmallMultipleConfig> config3 = new List<SmallMultipleConfig>();
        config3.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep * 0.5f, new Vector3(-visDimensions.x * smallMultipleScaleStep * 0.25f, 0.0f, visDimensions.z * smallMultipleScaleStep * 0.25f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep * 0.5f, new Vector3(visDimensions.x * smallMultipleScaleStep * 0.25f, 0.0f, visDimensions.z * smallMultipleScaleStep * 0.25f)));
        config3.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep * 0.5f, new Vector3(0.0f, 0.0f, 3.0f * visDimensions.z * smallMultipleScaleStep * 0.25f)));
        mSmallMultiples.Add(3, config3);

        List<SmallMultipleConfig> config4 = new List<SmallMultipleConfig>();
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep * 0.5f, new Vector3(-visDimensions.x * smallMultipleScaleStep * 0.25f, 0.0f, visDimensions.z * smallMultipleScaleStep * 0.25f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep * 0.5f, new Vector3(visDimensions.x * smallMultipleScaleStep * 0.25f, 0.0f, visDimensions.z * smallMultipleScaleStep * 0.25f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep * 0.5f, new Vector3(-visDimensions.x * smallMultipleScaleStep * 0.25f, 0.0f, 3.0f * visDimensions.z * smallMultipleScaleStep * 0.25f)));
        config4.Add(new SmallMultipleConfig(Vector3.one * smallMultipleScaleStep * 0.5f, new Vector3(visDimensions.x * smallMultipleScaleStep * 0.25f, 0.0f, 3.0f * visDimensions.z * smallMultipleScaleStep * 0.25f)));
        mSmallMultiples.Add(4, config4);
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
                        launchUI.SetActive(true);
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    public void Launch()
    {
        LaunchSim(NumClients + 1);
        launchUI.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
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

    private int mCurrentClient = -1;

    public void ShowConfigScreen(int currentClient)
    {
        Debug.Log("ShowConfigScreen " + currentClient.ToString());

        if (currentClient >= clientConfigs.Count)
        {
            return;
        }

        mCurrentClient = currentClient;

        var currentConfig = clientConfigs[currentClient];

        trajConfigUI.SetActive(true);

        TextMeshProUGUI[] texts = trajConfigUI.GetComponentsInChildren<TextMeshProUGUI>();
        var search = texts.First((t) => t.name == "ClientNumber");
        if (search != null)
        {
            search.text = (currentClient + 1).ToString();
        }

        Toggle[] toggles = trajConfigUI.GetComponentsInChildren<Toggle>();
        var toggleSearch = toggles.First((t) => t.name == "MeshToggle");
        if (toggleSearch != null)
        {
            toggleSearch.value = currentConfig.mesh;
        }
        toggleSearch = toggles.First((t) => t.name == "TrailsToggle");
        if (toggleSearch != null)
        {
            toggleSearch.value = currentConfig.trail;
        }
        toggleSearch = toggles.First((t) => t.name == "GhostingToggle");
        if (toggleSearch != null)
        {
            toggleSearch.value = currentConfig.ghosting;
        }
        toggleSearch = toggles.First((t) => t.name == "DensityToggle");
        if (toggleSearch != null)
        {
            toggleSearch.value = currentConfig.density;
        }

        /*        UIDocument doc = trajConfigUI.GetComponent<UIDocument>();
                if (doc != null)
                {
                    var root = doc.rootVisualElement;

                    Label clientIdLabel = root.Q<Label>("clientIdLabel");
                    if (clientIdLabel != null)
                    {
                        clientIdLabel.text = currentClient.ToString();
                    }

                    Toggle meshToggle = root.Q<Toggle>("meshToggle");
                    if (meshToggle != null)
                    {
                        meshToggle.value = currentConfig.mesh;
                        meshToggle.RegisterValueChangedCallback<bool>(e =>
                        {
                            currentConfig.mesh = e.newValue;
                        });
                    }

                    Toggle trailToggle = root.Q<Toggle>("trailToggle");
                    if (trailToggle != null)
                    {
                        trailToggle.value = currentConfig.trail;
                        trailToggle.RegisterValueChangedCallback<bool>(e =>
                        {
                            currentConfig.trail = e.newValue;
                        });
                    }

                    Toggle ghostingToggle = root.Q<Toggle>("ghostingToggle");
                    if (ghostingToggle != null)
                    {
                        ghostingToggle.value = currentConfig.ghosting;
                        ghostingToggle.RegisterValueChangedCallback<bool>(e =>
                        {
                            currentConfig.ghosting = e.newValue;
                        });
                    }

                    Toggle densityToggle = root.Q<Toggle>("densityToggle");
                    if (densityToggle != null)
                    {
                        densityToggle.value = currentConfig.density;
                        densityToggle.RegisterValueChangedCallback<bool>(e =>
                        {
                            currentConfig.density = e.newValue;
                        });
                    }

                    Button applyButton = root.Q<Button>("applyButton");
                    if (applyButton != null)
                    {
                        applyButton.clicked += () => {
                            ApplyClientConfig(visClients[currentClient].GetComponent<Visualisation>(), currentConfig);
                            trajConfigUI.SetActive(false);
                        };
                    }*/
        // }
    }


    public bool Mesh { get { return clientConfigs[mCurrentClient].mesh; } set { clientConfigs[mCurrentClient].mesh = value; } }
    public bool Trails { get { return clientConfigs[mCurrentClient].trail; } set { clientConfigs[mCurrentClient].trail = value; } }
    public bool Ghosting { get { return clientConfigs[mCurrentClient].ghosting; } set { clientConfigs[mCurrentClient].ghosting = value; } }
    public bool Density { get { return clientConfigs[mCurrentClient].density; } set { clientConfigs[mCurrentClient].density = value; } }

    public void CloseConfigScreen()
    {
        ApplyClientConfig(visClients[mCurrentClient].GetComponent<Visualisation>(), clientConfigs[mCurrentClient]);
        trajConfigUI.SetActive(false);
    }

    public void LaunchSim(int clientCount)
    {
        SpawnClients(clientCount);

        if (player != null)
        {
            player.SetActive(true);

            foreach (var client in visClients)
            {
                Visualisation vis = client.GetComponent<Visualisation>();
                if (vis != null)
                {
                    vis.controller = player.GetComponent<VisSimController>();
                }
                client.SetActive(true);
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
            List<SmallMultipleConfig> smallMultipleConfigs = mSmallMultiples[clientCount];

            for (int i = 0; i < clientCount; i++)
            {
                if (i < smallMultipleConfigs.Count)
                {
                    SmallMultipleConfig smConfig = smallMultipleConfigs[i];

                    GameObject vis = Instantiate(visPrefab);
                    vis.transform.position = smConfig.position;
                    vis.transform.rotation = Quaternion.identity;
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

                    InteractableUnityEventWrapper interaction = vis.GetComponent<InteractableUnityEventWrapper>();
                    if (interaction != null)
                    {
                        interaction.WhenSelect.AddListener(() => ShowConfigScreen(interaction.gameObject.GetComponent<Visualisation>().instanceId));
                    }
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
