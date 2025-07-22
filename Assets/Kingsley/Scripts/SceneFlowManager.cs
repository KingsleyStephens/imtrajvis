using System.Collections.Generic;
using KController;
using KVisualisation;
using SATestbed;
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
    private GameObject launchUI;

    [SerializeField]
    private GameObject trajConfigUI;

    private List<GameObject> visClients = new List<GameObject>();

    private int numClients = 0;

    private List<VisClient.MovingObjectSettings> clientConfigs = new List<VisClient.MovingObjectSettings>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (launchUI != null)
        {
            launchUI.SetActive(true);

            UIDocument doc = launchUI.GetComponent<UIDocument>();
            if (doc != null) {
                var root = doc.rootVisualElement;
                Button launchButton = root.Q<Button>("launchButton");
                if (launchButton != null)
                {
                    launchButton.clicked += () => { LaunchSim(numClients); launchUI.SetActive(false); };
                }

                DropdownField clientCountDropdown = root.Q<DropdownField>("clientCountDropdown");
                if (clientCountDropdown != null)
                {
                    numClients = int.Parse(clientCountDropdown.value);

                    clientCountDropdown.RegisterValueChangedCallback(e =>
                    {
                        numClients = int.Parse(e.newValue);
                    });
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (numClients > 0 && trajConfigUI != null && !trajConfigUI.activeSelf && Mouse.current.rightButton.wasPressedThisFrame)
        {
            int currentClient = 0;

            var currentConfig = clientConfigs[currentClient];

            trajConfigUI.SetActive(true);
            UIDocument doc = trajConfigUI.GetComponent<UIDocument>();
            if (doc != null)
            {
                var root = doc.rootVisualElement;

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
                }
            }
        }
    }

    public void LaunchSim(int clientCount)
    {
        SpawnClients(clientCount);

        VisSimController controller = null;
        switch (runType)
        {
            case RunType.Record:
                {
                    if (recorder != null)
                    {
                        recorder.SetActive(true);
                        controller = recorder.GetComponent<VisSimController>();
                    }
                    break;
                }
            case RunType.Play:
                {
                    if (player != null)
                    {
                        player.SetActive(true);
                        controller = player.GetComponent<VisSimController>();
                    }
                    break;
                }
            default:
                {
                    break;
                }
        }

        foreach (var client in visClients)
        {
            Visualisation vis = client.GetComponent<Visualisation>();
            if (vis != null)
            {
                vis.controller = controller;
            }
            client.SetActive(true);
        }

    }

    private void SpawnClients(int clientCount)
    {
        if (visPrefab == null)
        {
            return; 
        }

        for (int i = 0; i < clientCount; i++)
        {
            GameObject vis = Instantiate(visPrefab);
            vis.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
            vis.transform.rotation = Quaternion.identity;
            vis.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            visClients.Add(vis);

            VisClient.MovingObjectSettings config = new VisClient.MovingObjectSettings();
            clientConfigs.Add(config);
            ApplyClientConfig(vis.GetComponent<Visualisation>(), config);
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
