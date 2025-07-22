using UnityEngine;

[CreateAssetMenu(fileName = "TrajViewConfig", menuName = "ImTrajVis/Visualisation/TrajViewConfig", order = 1)]
public class TrajViewConfig : ScriptableObject
{
    public bool mesh = true;
    public bool trail = false;
    public bool ghosting = false;
    public bool density = false;
}
