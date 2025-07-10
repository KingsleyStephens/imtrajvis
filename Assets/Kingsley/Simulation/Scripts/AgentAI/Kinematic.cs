using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kinematic : MoveableObject
{
    [SerializeField]
    private float Mass = 10.0f;

    [SerializeField]
    private float Drag = -1.0f;

    private float mInverseMass;

    protected override void InitMoveable()
    {
        mInverseMass = 1.0f / Mass;

        //Vector3 forward = Orientation * Vector3.forward;
        //Utils.LimitVector(ref forward, MaxSpeed);
        //Velocity = forward;
    }

    public override void ApplyForce(float dt, Vector3 force)
    {
        Vector3 velocity = Velocity;

        if (force.sqrMagnitude > 0.0f)
        {
            Utils.LimitVector(ref force, MaxAcceleration);

            velocity += force * mInverseMass * dt;
            Utils.LimitVector(ref velocity, MaxSpeed);
        }

        velocity = velocity.normalized * Mathf.Clamp(velocity.magnitude + Drag * dt, 0.0f, MaxSpeed);
        Velocity = velocity;

        if (Velocity.magnitude > 0.0f)
        {
            Position += Velocity * dt;

            Vector3 forward = Velocity.normalized;
            Vector3 up = Vector3.Cross(forward, transform.right);
            Orientation = Quaternion.LookRotation(forward, up);
        }
    }
}
