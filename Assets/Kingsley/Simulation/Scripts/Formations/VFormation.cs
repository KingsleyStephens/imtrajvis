using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class VFormation : FormationPattern
{
    public int WingSize = 4;
    public float SpreadDistance = 3.0f;
    public float angle = 90.0f;

    public new float Radius { get { return WingSize * SpreadDistance; } }

    public override int GetSize() { return WingSize * 2 + 1; }

    public override Vector3 GetFormationPosition(int index)
    {
        if (index <= 0 || index >= GetSize())
        {
            return Vector3.zero;
        }

        // -1 = left, 1 = right
        int side = index % 2;
        if (side == 0)
        {
            side = -1;
        }
        int depth = (index + 1) / 2;

        Vector3 direction = Quaternion.Euler(0.0f, 180.0f + side * angle * 0.5f, 0.0f) * Vector3.forward;
        direction *= SpreadDistance * depth;

        return direction;
    }

}
