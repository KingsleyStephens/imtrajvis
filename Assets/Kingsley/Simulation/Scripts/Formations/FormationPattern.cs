using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormationPattern : ScriptableObject
{
    public virtual int GetSize() { return 0; }
    public virtual Vector3 GetFormationPosition(int index) {  return Vector3.zero; }

    public float Radius { get { return 0; } }

}
