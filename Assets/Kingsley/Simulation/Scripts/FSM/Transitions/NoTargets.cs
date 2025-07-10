using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoTargets : FSMTransition
{
    private float sightRange = 100.0f;
    private Agent thisAgent;
    private List<Formation> excludeFormations = new List<Formation>();

    public NoTargets(Agent thisAgent, float sightRange)
    {
        this.thisAgent = thisAgent;
        this.sightRange = sightRange;

        Formation formation = thisAgent.GetComponent<Formation>();
        if (formation != null)
        {
            excludeFormations.Add(formation);
        }
    }

    public override bool Test()
    {
        Formation[] targetFormations = AgentManager.Instance?.QueryFormations(excludeFormations.ToArray(), thisAgent.transform.position, sightRange);
        return (targetFormations.Length == 0);
    }

    public override void OnTrigger() 
    {
    }
}
