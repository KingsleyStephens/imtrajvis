using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seek : SteeringBehaviour
{
    [SerializeField]
    private float targetRadius = 0.5f;

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
            Vector3 direction = mTarget.Position - mMoveableObject.Position;
            float distance = direction.magnitude;
            if (distance < targetRadius)
            {
                return Vector3.zero;
            }

            return direction.normalized * mMoveableObject.MaxAcceleration;
        }

        return Vector3.zero;
    }
}
