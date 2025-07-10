using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvoidBounds : SteeringBehaviour
{
    [SerializeField]
    private float collisionTestDistance = 20.0f;

    [SerializeField]
    private int numWhiskers = 3;
    [SerializeField]
    private float whiskerAngle = 30.0f;

    protected MoveableObject mMoveableObject;
    protected Agent mAgent;

    private List<Vector3> mWhiskers = new List<Vector3>();

    private enum State { None, Avoiding }
    private State mState = State.None;

    protected override void InitSteeringBehaviour()
    {
        mAgent = GetComponent<Agent>();
        mMoveableObject = GetComponent<MoveableObject>();

        for (int i = 0; i < numWhiskers; ++i)
        {
            Vector3 w = Vector3.forward;
            Quaternion wRot = Quaternion.Euler(0.0f, 0.0f, 360.0f / numWhiskers * i);
            Quaternion wRot2 = Quaternion.Euler(0.0f, whiskerAngle, 0.0f);
            mWhiskers.Add(wRot * wRot2 * w);
        }
    }

    protected override void UpdateSteeringBehaviour()
    {
        for (int i = 0; i < mWhiskers.Count; ++i)
        {
            Debug.DrawLine(transform.position, transform.position + transform.TransformDirection(mWhiskers[i]) * collisionTestDistance, Color.yellow);
        }
    }

    public override Vector3 Calculate()
    {
        if (mMoveableObject != null)
        {
            Vector3 avoidForce = Vector3.zero;
            for (int i = 0; i < mWhiskers.Count; ++i)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(mWhiskers[i]), out hit, collisionTestDistance, 1 << LayerMask.NameToLayer("BOUNDS")))
                {
                    Vector3 collisionVector = hit.point - transform.position;
                    float collisionUrgency = 1.0f - (collisionVector.magnitude / collisionTestDistance);
                    avoidForce += hit.normal * (collisionUrgency * mMoveableObject.MaxAcceleration);
                }
            }

            if (avoidForce.magnitude > 0.0f)
            {
                Debug.DrawLine(transform.position, transform.position + avoidForce, Color.blue);

                if (mState == State.None)
                {
                    mState = State.Avoiding;

                    EventManager.Instance?.AddEvent(new EventManager.Event(
                        mAgent?.Id ?? -1,
                        EventManager.EventType.Navigation,
                        EventManager.EventSubType.Begin,
                        "Begin course correction " + transform.position.ToString(),
                        transform.position, transform.forward));
                }
            } 
            else
            {
                if (mState == State.Avoiding)
                {
                    mState = State.None;

                    EventManager.Instance?.AddEvent(new EventManager.Event(
                         mAgent?.Id ?? -1,
                        EventManager.EventType.Navigation,
                        EventManager.EventSubType.End,
                        "End course correction " + transform.position.ToString(),
                        transform.position, transform.forward));
                }
            }

            return avoidForce;
        }

        return Vector3.zero;
    }
}
