using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Steering
{
    private MoveableObject mMoveable;

    public Vector3 Position { get { return mMoveable.Position; } }
    public Quaternion Orientation { get { return mMoveable.Orientation; } }

    private List<SteeringBehaviour> mSteeringBehaviours;

    public Steering() { }


    public void Init(MoveableObject moveable, List<SteeringBehaviour> behaviours)
    {
        mMoveable = moveable;
        mSteeringBehaviours = behaviours;
    }

    public void Add(SteeringBehaviour behaviour)
    {
        mSteeringBehaviours.Add(behaviour);
    }

    public void Remove(SteeringBehaviour behaviour)
    {
        mSteeringBehaviours.Remove(behaviour);
    }

    public void Update(float dt) 
    {
        Vector3 force = Vector3.zero;

        foreach (var b in mSteeringBehaviours)
        {
            Vector3 steeringForce = b.Calculate() * b.Weight;
            Debug.DrawLine(mMoveable.Position, mMoveable.Position + steeringForce, Color.green);

            force += steeringForce;
        }

        if (mMoveable != null)
        {
            mMoveable.ApplyForce(dt, force);
        }
    }
}
