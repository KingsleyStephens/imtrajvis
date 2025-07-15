using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringBehaviour : MonoBehaviour 
{
    public float Weight = 1.0f;

    void Start()
    {
        InitSteeringBehaviour();
    }

    void Update()
    {
        UpdateSteeringBehaviour();
    }

    protected virtual void InitSteeringBehaviour() { }
    protected virtual void UpdateSteeringBehaviour() {
    }

    public virtual Vector3 Calculate()
    {
        return Vector3.zero;
    }
}
