using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSighted : FSMTransition
{
    private float sightRange = 100.0f;
    private Agent thisAgent;
    private List<Formation> excludeFormations = new List<Formation>();
    private Formation[] targetFormations;

    public TargetSighted(Agent thisAgent, float sightRange)
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
        targetFormations = AgentManager.Instance?.QueryFormations(excludeFormations.ToArray(), thisAgent.transform.position, sightRange);
        return (targetFormations != null && targetFormations.Length > 0);
    }

    public override void OnTrigger() 
    {
        if (thisAgent != null)
        {
            Target target = thisAgent.GetComponent<Target>();
            if (target != null)
            {
                GameObject closestAgent = null;
                float closestDist = float.MaxValue;
                for (int i = 0; i < targetFormations.Length; i++)
                {
                    if (targetFormations[i].gameObject.activeSelf)
                    {
                        float dist = (targetFormations[i].transform.position - thisAgent.transform.position).magnitude;
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestAgent = targetFormations[i].gameObject;
                        }
                    }
                }

                target.TargetGameObject = closestAgent;
            }
        }
    
    }
}
