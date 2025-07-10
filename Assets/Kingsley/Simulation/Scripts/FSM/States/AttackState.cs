using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState<E> : FSMState<E> where E : Enum
{
    private float weight = 1.0f;
    private float maxForce = 50.0f;
    private float maxSpeed = 50.0f;
    private float attackDist = 30.0f;
    private float loseDist = 100.0f;

    private float timeBetweenFiring = 5.0f;

    private float fireTime = 0.0f;

    private float prevMaxForce;
    private float prevMaxSpeed;

    private Target target;

    public AttackState(float weight)
    {
        this.weight = weight;
    }

    public override void OnEnter(Agent agent)
    {
        base.OnEnter(agent);

        Seek seek = mAgent.AddSteeringBehaviour<Seek>(weight);

        MoveableObject moveable = mAgent.GetComponent<MoveableObject>();
        if (moveable != null)
        {
            prevMaxForce = moveable.MaxAcceleration;
            prevMaxSpeed = moveable.MaxSpeed;

            moveable.MaxAcceleration = maxForce;
            moveable.MaxSpeed = maxSpeed;
        }

        target = mAgent.GetComponent<Target>();

        AgentStatus s = mAgent.gameObject.GetComponent<AgentStatus>();
        if (s != null)
        {
            s.Status = AgentStatus.StatusType.Attacking;
        }
    }

    public override void OnExit()
    {
        mAgent.RemoveSteeringBehaviour<Seek>();

        MoveableObject moveable = mAgent.GetComponent<MoveableObject>();
        if (moveable != null)
        {
            moveable.MaxAcceleration = prevMaxForce;
            moveable.MaxSpeed = prevMaxSpeed;
        }
    }

    public override void Update(float dt)
    {
        GameObject targGO = target?.TargetGameObject;
        if (targGO != null)
        {
            float dist = (targGO.transform.position - mAgent.transform.position).magnitude;
            if (dist < attackDist)
            {
                WeaponSystem ws = mAgent.GetComponent<WeaponSystem>();
                if (ws != null)
                {
                    if (ws.HasAmmo && (Time.time - fireTime > timeBetweenFiring))
                    {
                        ws.Fire(targGO);
                        fireTime = Time.time;
                    }
                }
            }
            else if (dist > loseDist)
            {
                SquadFSMAgent sfa = mAgent as SquadFSMAgent;
                if (sfa != null)
                {
                    sfa.mState.QueueEvent(SquadFSMEvents.TargetLost);
                }
            }
        } 
        else
        {
            SquadFSMAgent sfa = mAgent as SquadFSMAgent;
            if (sfa != null)
            {
                sfa.mState.QueueEvent(SquadFSMEvents.TargetDestroyed);
            }
        }
    }
}
