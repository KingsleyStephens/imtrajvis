using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Formation : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> squad;

    [SerializeField]
    private FormationPattern[] formations;

    [SerializeField]
    private int currentFormation = 0;

    private bool isVisible = true;
    public bool Visible { get { return isVisible; } set { isVisible = value; SetVisible(isVisible); } }

    public float CollisionRadius {  get; private set; }

    public int FormationSize { get {  return formations[currentFormation].GetSize(); } }
    public int SquadSize { get { return squad.Count; } }

    public Vector3 GetFormationPos(int formationIndex) 
    { 
        return transform.TransformPoint(formations[currentFormation].GetFormationPosition(formationIndex)); 
    }  

    private Dictionary<int, int> mSlots = new Dictionary<int, int>();

    private int mNextSlot = 0;

    public int CurrentFormation { get { return currentFormation; } }
    public List<GameObject> Squad { get { return squad; } }


    public void RemoveSquadMember(GameObject member)
    {
        squad.Remove(member);
        if (squad.Count == 0)
        {
            AgentManager.Instance?.RemoveAgent(gameObject.GetComponent<SquadFSMAgent>());

            gameObject.SetActive(false);
            //Destroy(gameObject);
        }
    }


    public void SetFormation(int formation, Color colour)
    {
        currentFormation = formation;
        CollisionRadius = formations[currentFormation].Radius;
        mSlots.Clear();
        mNextSlot = 0;

        TrailRenderer trailRenderer = gameObject.GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            trailRenderer.material.color = colour;
        }

        foreach (GameObject agent in squad)
        {
            FormationFollower follower = agent.GetComponent<FormationFollower>();
            follower.InitFollower(this, colour);
        }
    }

    public int RequestSlot(Vector3 position)
    {
        int slot = -1;
        if (mNextSlot < FormationSize)
        {
            int formationIndex = FindSlot(position);
            slot = mNextSlot;
            mSlots.Add(formationIndex, slot);
            mNextSlot++;
        }

        return slot;
    }

    public Vector3 GetSlotPosition(int slot)
    {
        if (slot >= 0 && slot < FormationSize)
        {
            return GetFormationPos(slot);
        }

        return transform.position;
    }

    private int FindSlot(Vector3 position)
    {
        int count = FormationSize;
        int closestFormationIndex = -1;
        float closestFormationDistance = float.MaxValue;
        for (int i = 0; i < count; ++i)
        {
            if (!mSlots.ContainsKey(i))
            {
                Vector3 formationPos = GetFormationPos(i);
                float dist = (formationPos - position).magnitude;
                if (dist < closestFormationDistance)
                {
                    closestFormationDistance = dist;
                    closestFormationIndex = i;
                }
            }
        }

        return closestFormationIndex;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void SetVisible(bool visible)
    {
        TrailRenderer trailRenderer = gameObject.GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            if (!visible)
            {
                GameObject go = new GameObject(gameObject.name + " trail snapshot");
                go.transform.position = transform.position;
                go.transform.rotation = transform.rotation;
                TrailRenderer tr = go.AddComponent<TrailRenderer>();
                tr.material = Instantiate(trailRenderer.material);
                Vector3[] positions = new Vector3[trailRenderer.positionCount];
                trailRenderer.GetPositions(positions);
                tr.AddPositions(positions);
                tr.time = trailRenderer.time;
                trailRenderer.Clear();
                trailRenderer.AddPosition(tr.GetPosition(tr.positionCount - 1));
            }

            trailRenderer.enabled = visible;
        }

    }
}
