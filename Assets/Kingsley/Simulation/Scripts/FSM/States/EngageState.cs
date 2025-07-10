using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngageState<E> : FSMState<E> where E : Enum
{
    private float weight = 1.0f;
    private float attackDist = 25.0f;
    private float attackDelay = 1.0f;

    private Formation target;
    private float attackTimer = 0.0f;

    public EngageState(float weight)
    {
        this.weight = weight;
    }

    public override void OnEnter(Agent agent)
    {
        base.OnEnter(agent);

        GameObject targetGameObject = mAgent.gameObject.GetComponent<Target>()?.TargetGameObject;
        if (targetGameObject != null) {
            target = targetGameObject.GetComponent<Formation>();
        }

        mAgent.AddSteeringBehaviour<Seek>(weight);

        EventManager.Instance?.AddEvent(new EventManager.Event(
            mAgent.Id,
            EventManager.EventType.Conflict,
            EventManager.EventSubType.Engage,
            mAgent.transform.position.ToString(),
            mAgent.transform.position, mAgent.transform.forward));

    }

    public override void OnExit()
    {
        mAgent.RemoveSteeringBehaviour<Seek>();

        EventManager.Instance?.AddEvent(new EventManager.Event(
            mAgent.Id,
            EventManager.EventType.Conflict,
            EventManager.EventSubType.Disengage,
            mAgent.transform.position.ToString(),
            mAgent.transform.position, mAgent.transform.forward));
    }

    public override void Update(float dt)
    {
        if (attackTimer > 0.0f)
        {
            attackTimer -= dt;
            return;
        }

        if (target != null && (target.transform.position - mAgent.transform.position).magnitude < attackDist + target.CollisionRadius)
        {
            GameObject closestAgent = null;
            float closestDist = float.MaxValue;
            for (int i = 0; i < target.Squad.Count; ++i)
            {
                float dist = (target.Squad[i].transform.position - mAgent.transform.position).magnitude;
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestAgent = target.Squad[i];
                }
            }

            if (closestAgent != null)
            {
                Formation f = mAgent.gameObject.GetComponent<Formation>();
                if (f != null)
                {
                    foreach (var s in f.Squad)
                    {
                        AgentStatus status = s.GetComponent<AgentStatus>();
                        if (status != null && status.Status != AgentStatus.StatusType.Attacking)
                        {
                            Agent a = s.GetComponent<Agent>();
                            if (a != null)
                            {
                                SquadFSMAgent sfa = a as SquadFSMAgent;
                                if (sfa != null)
                                {
                                    Target t = sfa.gameObject.GetComponent<Target>();
                                    if (t != null)
                                    {
                                        t.TargetGameObject = closestAgent;
                                    }
                                    sfa.mState.QueueEvent(SquadFSMEvents.Attack);
                                    attackTimer = attackDelay;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
}
