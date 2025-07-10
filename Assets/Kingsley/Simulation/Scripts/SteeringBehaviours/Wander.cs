using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wander : SteeringBehaviour
{
    public float bigCircleDistance = 1.0f;
    public float bigCircleRadius = 0.5f;
    public float jitterCircleRadius = 0.1f;

    private Vector3 bigCirclePos = Vector3.zero;
    protected MoveableObject mMoveableObject;

    protected override void InitSteeringBehaviour() 
    {
        mMoveableObject = GetComponent<MoveableObject>();
        bigCirclePos = Vector3.forward * bigCircleRadius;
    }

    public override Vector3 Calculate()
    {
        if (mMoveableObject != null)
        {
            bigCirclePos += Random.onUnitSphere * jitterCircleRadius;
            Utils.LimitVector(ref bigCirclePos, bigCircleRadius);

            return transform.TransformDirection((Vector3.forward * bigCircleDistance + bigCirclePos).normalized * mMoveableObject.MaxAcceleration);
        }

        return Vector3.zero;
    }
}
