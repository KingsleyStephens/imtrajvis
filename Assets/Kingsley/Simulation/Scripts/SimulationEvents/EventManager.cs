using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public GameObject eventIconPrototype;
    public GameObject waypointPrototype;

    public GameObject marker;
    public GameObject fire;
    public GameObject sink;
    public GameObject current;

    public bool Visible {get; set;} = true;


    public enum EventType
    {
        OperationLifeCycle,
        Navigation,
        Conflict,
        Destroyed,
        Movement
    }

    public enum EventSubType
    {
        Begin,
        Milestone,
        End,
        Change,
        Waypoint,
        Engage,
        Disengage
    }

    public class Event
    {
        public int agentId;
        public EventType type;
        public EventSubType subType;
        public string info;
        public Vector3 position;
        public Vector3 direction;
        public float extraValue;
        public float time;

        public Event(int agentId, EventType type, EventSubType subType, string info, Vector3 position, Vector3 direction, float extraValue = 0.0f)
        {
            this.agentId = agentId;
            this.type = type;
            this.subType = subType;
            this.info = info;
            this.position = position;
            this.time = 0.0f;
            this.direction = direction;
            this.extraValue = extraValue;
        }
    }

    public NarrativeManager narrativeManager;

    private List<Event> mEvents = new List<Event>();

    private Dictionary<EventType, Color> eventTypeColours = new Dictionary<EventType, Color>();
    private Dictionary<EventType, int> eventSignificance = new Dictionary<EventType, int>();

    private float mStartTime;

    private static EventManager mInstance;

    public static EventManager Instance { get { return mInstance; } }

    public delegate void EventHandler(Event e);
    public event EventHandler OnMovementEvent;

    private void Awake()
    {
        mInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        eventTypeColours.Add(EventType.OperationLifeCycle, Color.white);
        eventTypeColours.Add(EventType.Navigation, Color.blue);
        eventTypeColours.Add(EventType.Conflict, Color.red);
        eventTypeColours.Add(EventType.Destroyed, Color.gray);
        eventTypeColours.Add(EventType.Movement, Color.gray);

        eventSignificance.Add(EventType.Navigation, 1);
        eventSignificance.Add(EventType.OperationLifeCycle, 5);
        eventSignificance.Add(EventType.Conflict, 10);
        eventSignificance.Add(EventType.Destroyed, 20);
        eventSignificance.Add(EventType.Movement, 0);

        if (narrativeManager != null)
        {
            narrativeManager.onTellEvent += NarrativeManager_onTellEvent;
        }

        mStartTime = Time.time;
    }

    private void NarrativeManager_onTellEvent(float time, string eventType, string[] actors, string[] eventData)
    {
        Debug.Log("Tell event");

        EventType eventTypeEnum;
        EventSubType eventSubTypeEnum;
        string[] split = eventType.Split('-');
        if (Enum.TryParse(split[0], out eventTypeEnum) && Enum.TryParse(split[1], out eventSubTypeEnum))
        {
            Debug.Log(eventTypeEnum.ToString() + eventSubTypeEnum.ToString());

            GameObject spawnIcon = null;
            bool billboarded = false;

            string[] posStrings = eventData[0].Substring(1, eventData[0].Length - 2).Split(',');
            Vector3 pos = new Vector3(float.Parse(posStrings[0]), float.Parse(posStrings[1]), float.Parse(posStrings[2]));

            string[] dirStrings = eventData[1].Substring(1, eventData[1].Length - 2).Split(',');
            Vector3 direction = new Vector3(float.Parse(dirStrings[0]), float.Parse(dirStrings[1]), float.Parse(dirStrings[2]));


            switch (eventTypeEnum)
            {
                case EventType.OperationLifeCycle:
                    {
                        spawnIcon = eventIconPrototype;
                        billboarded = true;
                        break;
                    }
                case EventType.Navigation:
                    {
                        switch (eventSubTypeEnum)
                        {
                            case EventSubType.Waypoint:
                                {
                                    spawnIcon = waypointPrototype;
                                    break;
                                }
                            default:
                                {
                                    spawnIcon = eventIconPrototype;
                                    billboarded = true;
                                    break;
                                }
                        }
                        break;
                    }
                case EventType.Conflict:
                    spawnIcon = fire;
                    break;
                case EventType.Destroyed:
                    spawnIcon = sink;
                    break;
                default:
                    break;
            }

            if (spawnIcon != null)
            {
                Debug.Log("spawnIcon");


                GameObject eObj = Instantiate(spawnIcon, pos, Quaternion.LookRotation(direction), transform);
                EventIcon icon = eObj.GetComponent<EventIcon>();
                if (icon != null)
                {
                    icon.SetColour(eventTypeColours[eventTypeEnum]);
                    icon.SetLabel(eventTypeEnum.ToString() + "-" + eventSubTypeEnum.ToString());
                    icon.SetSubLabel("[" + time + "][ " + actors[0] + "]");
                    icon.billboarded = billboarded;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddEvent(Event e)
    {
        e.time = Time.time - mStartTime;

        mEvents.Add(e);


        if (e.type == EventType.Movement)
        {
            OnMovementEvent?.Invoke(e);
        }
        else
        {
            // Transparent marker
            if (Visible)
            {
                Instantiate(current, e.position, Quaternion.identity);
            }

            if (narrativeManager != null)
            {
                narrativeManager.AddEvent(e.time,
                    e.type.ToString() + "-" + e.subType.ToString(),
                    new string[] { e.agentId.ToString() },
                    new string[] { e.position.ToString(), e.direction.ToString() },
                    eventSignificance[e.type]);
            }
        }
    }
}

