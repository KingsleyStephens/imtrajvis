using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KSimulation;

public class MoveableObject : MonoBehaviour
{
    public float MaxSpeed = 10.0f;
    public float MaxAcceleration = 10.0f;

    public Vector3 Position { get; protected set; }
    public Quaternion Orientation { get; protected set; }
    public Vector3 Velocity { get; protected set; }

    void Start()
    {
        InitMoveable();
    }

    private void OnDestroy()
    {
        SimulationRecorder.Instance.RemoveMovingObject(this);
    }

    void Update()
    {

    }

    public void InitMoveableObject(Vector3 position,  Quaternion orientation)
    {
        Position = position;
        Orientation = orientation;

        SimulationRecorder.Instance.AddMovingObject(this);
    }

    protected virtual void InitMoveable() { }

    public virtual void ApplyForce(float dt, Vector3 force)
    { }
}
