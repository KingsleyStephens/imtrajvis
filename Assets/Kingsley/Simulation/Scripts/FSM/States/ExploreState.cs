using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreState<E> : FSMState<E> where E : Enum
{
    private float bigCircleDistance = 1.0f;
    private float bigCircleRadius = 0.5f;
    private float jitterCircleRadius = 0.1f;
    private float weight = 1.0f;

    public ExploreState(float weight, float bigCircleDistance, float bigCircleRadius, float jitterCircleRadius)
    {
        this.weight = weight;
        this.bigCircleDistance = bigCircleDistance;
        this.bigCircleRadius = bigCircleRadius;
        this.jitterCircleRadius = jitterCircleRadius;
    }

    public override void OnEnter(Agent agent)
    {
        base.OnEnter(agent);

        Wander wander = mAgent.AddSteeringBehaviour<Wander>(weight);
        wander.bigCircleDistance = bigCircleDistance;
        wander.bigCircleRadius = bigCircleRadius;
        wander.jitterCircleRadius = jitterCircleRadius;
    }

    public override void OnExit()
    {
        mAgent.RemoveSteeringBehaviour<Wander>();
    }

    public override void Update(float dt)
    {
    }
}
