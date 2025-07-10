using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Straight : SteeringBehaviour
{
    [SerializeField]
    private Vector3 force = Vector3.zero;

    public override Vector3 Calculate()
    {
        return force;
    }
}
