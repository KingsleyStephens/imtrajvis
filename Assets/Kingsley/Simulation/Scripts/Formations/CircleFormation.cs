using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

[CreateAssetMenu]
public class CircleFormation : FormationPattern
{
    public int count = 10;
    public float radius = 10.0f;

    public new float Radius { get { return radius; } }

    public override int GetSize() { return count; }

    public override Vector3 GetFormationPosition(int index)
    {
        if (index < 0 || index >= GetSize())
        {
            return Vector3.zero;
        }

        return Quaternion.Euler(0.0f, 360.0f / count * index, 0.0f) * Vector3.forward * radius;
    }

}
