using KController;
using SATestbed;
using UnityEngine;

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
    private Visualisation vis;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        switch (runType)
        {
            case RunType.Record:
                {
                    recorder.SetActive(true);
                    vis.controller = recorder.GetComponent<VisSimController>();
                    break;
                }
            case RunType.Play:
                {
                    player.SetActive(true);
                    vis.controller = player.GetComponent<VisSimController>();
                    break;
                }
            default:
                {
                    break;
                }
        }

        vis.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
