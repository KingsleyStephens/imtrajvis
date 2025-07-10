using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class NarrativeManager : MonoBehaviour
{
    private class QueueData
    {
        public bool busy = false;
        public bool ready = false;
        public float cost;
        public List<PlotEvent> plotEvents;

        public QueueData(float cost, List<PlotEvent> plotEvents)
        {
            this.cost = cost;
            this.plotEvents = plotEvents;
        }
    }

    [SerializeField]
    private bool isRealTime = false;

    [SerializeField]
    private string eventFile = "/Data/testNarrative.json";

    [SerializeField]
    private float narrativeSpeed = 1.0f;

    public bool Visible {get; set;} = true;

    private NarrativeData fileNarrativeData;
    private NarrativeData narrativeData = new NarrativeData();
    private List<PlotEvent> liveEvents = new List<PlotEvent>();

    private float lastPlotPointTime = 0.0f;

    private bool isPlaying = false;

    private Dictionary<string, QueueData> plotEventQueues = new Dictionary<string, QueueData>();
    private PlotEvent lastEvent;

    [SerializeField]
    private float temporalTensionWeight = 1.0f;
    [SerializeField]
    private float continuityWeight = 1.0f;
    [SerializeField]
    private float centralityWeight = 1.0f;

    [SerializeField]
    private float momentum = 5.0f;
    [SerializeField]
    private float timeCorrection = 3.0f;

    private float startTime = 0.0f;
    private int currentPlaybackEvent = 0;
    private int timeTick = 0;
    
    private int currentTime = 0;

    private Label timeLabel;
    private ListView narrativeList;
    private List<string> narrativeListItems = new List<string>();
    public VisualTreeAsset listItemView;

    // Start is called before the first frame update
    void Start()
    {
        var uiDocument = GetComponent<UIDocument>();
        timeLabel = uiDocument.rootVisualElement.Q("currentTime") as Label;
        narrativeList = uiDocument.rootVisualElement.Q("narrative") as ListView;

        Func<VisualElement> makeItem = () => { return listItemView.CloneTree(); };
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            e.Q<Label>("label").text = narrativeListItems[i];
        };

        narrativeList.makeItem = makeItem;
        narrativeList.bindItem = bindItem;
        narrativeList.itemsSource = narrativeListItems;

        //if (Load(Application.dataPath + eventFile))
        {
            Play();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            if (isRealTime) 
            { 
                //if (currentPlaybackEvent < fileNarrativeData.plotEvents.Length)
                //{
                    float playTime = Time.time - startTime;

                    if ((int)playTime > timeTick)
                    {
                        currentTime = (int)playTime;
                        timeLabel.text = currentTime.ToString();

                        Debug.Log("Time: " + currentTime);
                        timeTick = currentTime;
                    }

                    //if (playTime > fileNarrativeData.plotEvents[currentPlaybackEvent].time)
                    //{
                    //    PlotEvent pe = fileNarrativeData.plotEvents[currentPlaybackEvent];

                    //    //string[] posStrings = pe.eventData[0].Substring(1, pe.eventData[0].Length - 2).Split(',');
                    //    //Instantiate(current, new Vector3(float.Parse(posStrings[0]), float.Parse(posStrings[1]), float.Parse(posStrings[2])), Quaternion.identity);

                    //    AddEventToQueues(pe);
                    //    currentPlaybackEvent++;
                    //}
                //}
                //else
                //{
                //    isPlaying = false;
                //}
            }

            if (Visible)
            {
                TellNarrative();
            }
        }
    }

    public float time;
    public string eventType;
    public string[] plotActors;
    public string[] eventData;
    public int significance;

    public delegate void OnTellEvent(float time, string eventType, string[] eventActors, string[] eventData);

    public event OnTellEvent onTellEvent;

    public void AddEvent(float time, string eventType, string[] eventActors, string[] eventData, int significance)
    {
        PlotEvent pe = new PlotEvent(time, eventType, eventActors, eventData, significance);
        liveEvents.Add(pe);

        //string[] posStrings = pe.eventData[0].Substring(1, pe.eventData[0].Length - 2).Split(',');
        //Instantiate(current, new Vector3(float.Parse(posStrings[0]), float.Parse(posStrings[1]), float.Parse(posStrings[2])), Quaternion.identity);

        AddEventToQueues(pe);
    }


    private void TellNarrative()
    {
        if (narrativeData != null)
        {
            if (EventsRemaining() > 0 && (Time.time - lastPlotPointTime) > narrativeSpeed)
            {
                TimeCompensation(currentTime);

                //Debug.Log("CalculateQueueCosts");
                CalculateQueueCosts();
                //Debug.Log("CalculateReadiness");
                CalculateReadiness();

                float lowestCost = float.MaxValue;
                string lowestCostKey = "";
                foreach (var k in plotEventQueues.Keys)
                {
                    if (plotEventQueues[k].cost < lowestCost)
                    {
                        if (plotEventQueues[k].ready && plotEventQueues[k].plotEvents.Count > 0)
                        {
                            lowestCost = plotEventQueues[k].cost;
                            lowestCostKey = k;
                        }
                        else
                        {
                            plotEventQueues[k].busy = true;
                        }
                    }
                }

                //Debug.Log("lowestCostKey: " + lowestCostKey);

                if (lowestCostKey != "")
                {
                    bool continueNarrative = true;
                    if (isRealTime && lastEvent != null && lowestCostKey != lastEvent.plotActors[0])
                    {
                        float timePast = Time.time - lastEvent.time;
                        float m = timePast - momentum;
                        if (m < lowestCost)
                        {
                            continueNarrative = false;
                        }
                    }
                    
                    if (continueNarrative)
                    { 
                        PlotEvent pe = plotEventQueues[lowestCostKey].plotEvents[0];
                        lastEvent = pe;
                        // OUTPUT TO CONSOLE
                        Debug.Log(pe.ToString());

                        narrativeListItems.Add(pe.ToString());
                        if (narrativeListItems.Count > 7)
                        {
                            narrativeListItems.RemoveAt(0);
                        }
                        narrativeList.Rebuild();

                        onTellEvent(pe.time, pe.eventType, pe.plotActors, pe.eventData);

                        //string[] posStrings = pe.eventData[0].Substring(1, pe.eventData[0].Length - 2).Split(',');
                        //GameObject prefab = marker;
                        //if (pe.eventType == "start" || pe.eventType == "Waypoint")
                        //{
                        //    prefab = marker;
                        //}
                        //else if (pe.eventType == "contact" || pe.eventType == "fire")
                        //{
                        //    prefab = fire;
                        //}
                        //else if (pe.eventType == "sink")
                        //{
                        //    prefab = sink;
                        //}

                        //Instantiate(prefab, new Vector3(float.Parse(posStrings[0]), float.Parse(posStrings[1]), float.Parse(posStrings[2])), Quaternion.identity);

                        foreach (var k in plotEventQueues.Keys)
                        {
                            if (plotEventQueues[k].plotEvents.Count > 0 && plotEventQueues[k].plotEvents[0] == pe)
                            {
                                plotEventQueues[k].plotEvents.Remove(pe);
                                plotEventQueues[k].busy = false;
                            }
                        }
                    }
                }


                lastPlotPointTime = Time.time;
            }
        }
    }

    private int EventsRemaining()
    {
        int count = 0;
        foreach (var k in plotEventQueues.Keys)
        {
            count += plotEventQueues[k].plotEvents.Count;
        }
        //Debug.Log("EventsRemaining: " + count);
        return count;
    }

    private bool Load(string jsonEventFile)
    {
        if (File.Exists(jsonEventFile))
        {
            string fileData = File.ReadAllText(jsonEventFile);
            if (fileData != null)
            {
                fileNarrativeData = JsonUtility.FromJson<NarrativeData>(fileData);
                if (!isRealTime)
                {
                    narrativeData = fileNarrativeData;
                }
                else
                {
                    fileNarrativeData.SortByTime();
                    narrativeData = new NarrativeData();
                }

                if (!isRealTime && narrativeData != null)
                {
                    //Debug.Log("SortNarrativeIntoQueues");
                    SortNarrativeIntoQueues();
                }
                return true;
            }
        }
        return false;
    }

    private void Play()
    {
        isPlaying = true;
        startTime = Time.time;
    }

    private void SortNarrativeIntoQueues()
    {
        foreach (var pe in narrativeData.plotEvents)
        {
            foreach (var a in pe.plotActors)
            {
                if (!plotEventQueues.ContainsKey(a))
                {
                    plotEventQueues[a] = new QueueData(0, new List<PlotEvent>());
                }

                plotEventQueues[a].plotEvents.Add(pe);
            }
        }

        foreach (var k  in plotEventQueues.Keys)
        {
            plotEventQueues[k].plotEvents.Sort(delegate(PlotEvent a, PlotEvent b)
            {
                return a.time.CompareTo(b.time);
            });
        }
    }

    private void AddEventToQueues(PlotEvent plotEvent)
    {
        foreach (var a in plotEvent.plotActors)
        {
            if (!plotEventQueues.ContainsKey(a))
            {
                plotEventQueues[a] = new QueueData(0, new List<PlotEvent>());
            }

            plotEventQueues[a].plotEvents.Add(plotEvent);
        }
    }

    private void CalculateReadiness()
    {
        foreach (var k in plotEventQueues.Keys)
        {
            if (plotEventQueues[k].plotEvents.Count > 0)
            {
                bool ready = true;
                PlotEvent e = plotEventQueues[k].plotEvents[0];
                foreach(var a in e.plotActors)
                {
                    if ((a != k) && (plotEventQueues[a].plotEvents.Count == 0 || plotEventQueues[a].plotEvents[0] != e))
                    {
                        ready = false;
                        break;
                    }
                }

                plotEventQueues[k].ready = ready;
            }
        }
    }

    private void CalculateQueueCosts()
    {
        foreach (var k in plotEventQueues.Keys)
        {
            if (!plotEventQueues[k].busy)
            {
                //Debug.Log("Cost of " + k);
                plotEventQueues[k].cost = CalculateQueueCost(plotEventQueues[k].plotEvents);
            }
        }
    }

    private float CalculateQueueCost(List<PlotEvent> plotEvents)
    {
        if (plotEvents.Count > 0)
        {
            float temporal = CalculateAbsoluteTemporalTension(plotEvents[0].time) * CalculateSignificanceTemporalTension(plotEvents[0].time);
            //Debug.Log(temporal);
            float relatedness = CalculateRelatedness(plotEvents);
            //Debug.Log(relatedness);
            float continuity = CalculateContinuity(plotEvents[0]);
            //Debug.Log(continuity);

            return temporalTensionWeight * temporal - centralityWeight * relatedness - continuityWeight * continuity;

        }
        return 0.0f;
    }

    private float CalculateAbsoluteTemporalTension(float time)
    {
        return time - GetEarliestQueueTime();
    }

    private float GetEarliestQueueTime()
    {
        float earliest = float.MaxValue;
        foreach (var k in plotEventQueues.Keys)
        {
            if (plotEventQueues[k].plotEvents.Count > 0 && plotEventQueues[k].plotEvents[0].time < earliest)
            {
                earliest = plotEventQueues[k].plotEvents[0].time;
            }
        }
        return earliest;
    }

    private int CalculateSignificanceTemporalTension(float time)
    {
        int sig = 0;
        foreach (var k in plotEventQueues.Keys)
        {
            if (plotEventQueues[k].plotEvents.Count > 0) {
                int i = 0;
                while (i < plotEventQueues[k].plotEvents.Count && plotEventQueues[k].plotEvents[i].time < time)
                {
                    sig += plotEventQueues[k].plotEvents[i].significance;
                    i++;
                }
            }
        }

        return sig;
    }

    private float CalculateRelatedness(List<PlotEvent> plotEvents)
    {
        float relatedness = 0.0f;
        string mainActor = plotEvents[0].plotActors[0];
        for (int i = 1; i < plotEvents[0].plotActors.Length; ++i)
        {
            int count = 0;
            for (int j = 0; j < plotEvents.Count; j++)
            {
                if (plotEvents[j].plotActors.Contains(mainActor)) {
                    count++;
                }
            }
            relatedness += (1 + count) / (1 + plotEvents.Count);
        }

        return relatedness;
    }

    private float CalculateContinuity(PlotEvent plotEvent)
    {
        if (lastEvent != null)
        {
            foreach (var a in plotEvent.plotActors)
            {
                if (lastEvent.plotActors.Contains(a))
                {
                    return 1.0f;
                }
            }
        }

        return 0.0f;
    }

    private void TimeCompensation(float currentTime)
    {
        foreach (var k in plotEventQueues.Keys)
        {
            if (plotEventQueues[k].plotEvents.Count > 0 && (currentTime - plotEventQueues[k].plotEvents[0].time) > timeCorrection)
            {
                while (plotEventQueues[k].plotEvents.Count > 0 && (currentTime - plotEventQueues[k].plotEvents[0].time) > timeCorrection)
                {
                    PlotEvent pe = plotEventQueues[k].plotEvents[0];
                    if (pe.significance >= 5)
                    {
                        break;
                    }
                    
                    // Cull it for insignificance
                    plotEventQueues[k].plotEvents.Remove(pe);
                }
            }
        }
    }

}
