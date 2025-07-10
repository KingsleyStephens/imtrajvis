using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    [SerializeField]
    private bool leaveWaypoints = false;
    [SerializeField]
    private float waypointTiming = 5.0f;

    private float waypointTimer = 0.0f;

    private int mId;
    private Steering steering = new Steering();

    private static int mNextId = 0;

    public int Id { get { return mId; } }

    private bool isVisible = true;
    public bool Visible { get { return isVisible;} set { isVisible = value; SetVisible(isVisible);}}

    private Energy energy;

    // Start is called before the first frame update
    void Start()
    {
        mId = mNextId++;

        energy = GetComponent<Energy>();
        if (energy == null )
        {
            energy = gameObject.AddComponent<Energy>();
        }

        if (steering != null)
        {
            MoveableObject moveableObject = GetComponent<MoveableObject>();
            if (moveableObject != null)
            {
                SteeringBehaviour[] behaviours = GetComponents<SteeringBehaviour>();
                steering.Init(moveableObject, new List<SteeringBehaviour>(behaviours));
            }
        }

        AgentManager.Instance?.AddAgent(this);

        OnAgentStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (steering != null)
        {
            steering.Update(Time.deltaTime);
            transform.position = steering.Position;
            transform.rotation = steering.Orientation;

            EventManager.Instance?.AddEvent(new EventManager.Event(
                Id,
                EventManager.EventType.Movement,
                EventManager.EventSubType.Change,
                transform.position.ToString(),
                transform.position, transform.forward, energy.EnergyReserve));
        }

        OnAgentUpdate(Time.deltaTime);

        if (leaveWaypoints)
        {
            waypointTimer += Time.deltaTime;
            if (waypointTimer > waypointTiming)
            {
                EventManager.Instance?.AddEvent(new EventManager.Event(
                    Id,
                    EventManager.EventType.Navigation,
                    EventManager.EventSubType.Waypoint,
                    transform.position.ToString(),
                    transform.position, transform.forward));

                waypointTimer = 0.0f;
            }
        }
    }

    protected virtual void OnAgentStart() { }
    protected virtual void OnAgentUpdate(float dt) { }
    protected virtual void OnAgentStop() { }

    public T AddSteeringBehaviour<T>(float weight = 1.0f) where T : SteeringBehaviour
    {
        T sb = gameObject.AddComponent<T>();
        sb.Weight = weight;
        steering.Add(sb);
        return sb;
    }

    public void RemoveSteeringBehaviour<T>() where T : SteeringBehaviour
    {
        SteeringBehaviour sb = gameObject.GetComponent<T>();
        steering.Remove(sb);
    }

    private void SetVisible(bool visible)
    {
        MeshRenderer mr = gameObject.GetComponent<MeshRenderer>();
        if (mr) 
        {
            mr.enabled = visible;
        }
    }
}
