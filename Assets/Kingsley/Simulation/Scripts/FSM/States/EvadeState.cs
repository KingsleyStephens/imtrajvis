using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeState<E> : FSMState<E> where E : Enum
{
    private float weight = 1.0f;
    private float maxForce = 50.0f;
    private float maxSpeed = 50.0f;
    private float prevMaxForce;
    private float prevMaxSpeed;
    private float evadeTime = 4.0f;
    private float evadeTimer = 4.0f;


    public EvadeState(float weight)
    {
        this.weight = weight;
    }

    public override void OnEnter(Agent agent)
    {
        base.OnEnter(agent);

        Avoid formationFollow = mAgent.AddSteeringBehaviour<Avoid>(weight);

        FormationFollower follower = mAgent.GetComponent<FormationFollower>();
        if (follower != null)
        {
            Target target = mAgent.GetComponent<Target>();
            if (target != null && follower.mFormation != null)
            {
                target.TargetGameObject = follower.mFormation.gameObject;
            }
        }

        MoveableObject moveable = mAgent.GetComponent<MoveableObject>();
        if (moveable != null)
        {
            prevMaxForce = moveable.MaxAcceleration;
            prevMaxSpeed = moveable.MaxSpeed;

            moveable.MaxAcceleration = maxForce;
            moveable.MaxSpeed = maxSpeed;
        }

        AgentStatus s = mAgent.gameObject.GetComponent<AgentStatus>();
        if (s != null)
        {
            s.Status = AgentStatus.StatusType.Moving;
        }

        evadeTimer = 0.0f;
    }

    public override void OnExit()
    {
        mAgent.RemoveSteeringBehaviour<Avoid>();

        MoveableObject moveable = mAgent.GetComponent<MoveableObject>();
        if (moveable != null)
        {
            moveable.MaxAcceleration = prevMaxForce;
            moveable.MaxSpeed = prevMaxSpeed;
        }

    }

    public override void Update(float dt)
    {
        evadeTimer += dt;
        if (evadeTimer > evadeTime)
        {
            SquadFSMAgent sfa = mAgent as SquadFSMAgent;
            if (sfa != null)
            {
                sfa.mState.QueueEvent(SquadFSMEvents.Formation);
            }
        }
    }
}
