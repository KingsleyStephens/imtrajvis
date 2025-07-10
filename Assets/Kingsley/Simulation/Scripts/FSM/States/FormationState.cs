using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationState<E> : FSMState<E> where E : Enum
{
    private float weight = 1.0f;

    public FormationState(float weight)
    {
        this.weight = weight;
    }

    public override void OnEnter(Agent agent)
    {
        base.OnEnter(agent);

        FormationFollower formationFollow = mAgent.AddSteeringBehaviour<FormationFollower>(weight);

        AgentStatus s = mAgent.gameObject.GetComponent<AgentStatus>();
        if (s != null)
        {
            s.Status = AgentStatus.StatusType.Moving;
        }
    }

    public override void OnExit()
    {
        mAgent.RemoveSteeringBehaviour<FormationFollower>();
    }

    public override void Update(float dt)
    {
    }
}
