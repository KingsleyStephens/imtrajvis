using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrive : SteeringBehaviour
{
    [SerializeField]
    private float targetRadius = 0.5f;
    [SerializeField]
    private float slowRadius = 5.0f;
    [SerializeField]
    private float timeToTarget = 1.0f;

    protected MoveableObject mMoveableObject;
    protected Target mTarget;

    protected override void InitSteeringBehaviour() 
    {
        mMoveableObject = GetComponent<MoveableObject>();
        mTarget = GetComponent<Target>();
    }

    protected override void UpdateSteeringBehaviour() 
    {
    }


    public override Vector3 Calculate()
    {
        if (mMoveableObject != null && mTarget != null)
        {
            float targetSpeed = 0.0f;

            Vector3 direction = mTarget.Position - mMoveableObject.Position;
            float distance = direction.magnitude;
            if (distance < targetRadius)
            {
                return Vector3.zero;
            }

            if (distance > slowRadius) 
            {
                targetSpeed = mMoveableObject.MaxSpeed;
            }
            else
            {
                targetSpeed = mMoveableObject.MaxSpeed * distance / slowRadius;
            }

            Vector3 targetVelocity = direction.normalized;
            targetVelocity *= targetSpeed;

            Vector3 force = targetVelocity - mMoveableObject.Velocity;
            force /= timeToTarget;

            Utils.LimitVector(ref force, mMoveableObject.MaxAcceleration);

            return force;
        }

        return Vector3.zero;
    }
}
