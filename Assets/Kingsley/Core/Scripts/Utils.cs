using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static void LimitVector(ref Vector3 v, float dist)
    {
        float length = v.magnitude;
        if (length > dist)
        {
            v = v.normalized * dist;
        }
    }

}
