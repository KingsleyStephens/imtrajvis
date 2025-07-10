using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    private List<Agent> agents = new List<Agent>();
    private List<Formation> formations = new List<Formation>();

    private static AgentManager mInstance;

    public static AgentManager Instance { get { return mInstance; } }

    public List<Agent> Agents { get { return agents; }}
    public List<Formation> Formations { get { return formations; } }

    void Awake()
    {
        mInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void AddAgent(Agent agent)
    { 
        agents.Add(agent);

        Formation formation = agent.GetComponent<Formation>();
        if (formation != null)
        {
            formations.Add(formation);
        }
    }

    public void RemoveAgent(Agent agent)
    {
        int index = agents.FindIndex(a => a.Id == agent.Id);
        if (index >= 0 && index < agents.Count)
        {
            agents.RemoveAt(index);
        }

        Formation formation = agent.GetComponent<Formation>();
        if (formation != null)
        {
            formations.Remove(formation);
        }
    }

    public Formation[] QueryFormations(Formation[] exludeFormations, Vector3 position, float radius)
    {
        List<Formation> formationQuery = new List<Formation>();

        foreach (Formation f in formations)
        {
            if (exludeFormations != null && exludeFormations.Contains(f))
            {
                continue;
            }

            if ((f.transform.position - position).magnitude < radius + f.CollisionRadius)
            {
                formationQuery.Add(f);
            }
        }

        return formationQuery.ToArray();
    }



}
