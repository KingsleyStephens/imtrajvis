using UnityEngine;

public class AgentId : MonoBehaviour
{
    public enum AgentClass
    {
        Formation,
        MovingObject,
        Weapon
    }

    public AgentClass agentClass = AgentClass.Formation;
    
    public int id = -1;
    public int parentId = -1;

    private static int currentId = 0;

    public static int NextId { get { return currentId++; } }

    private void Awake()
    {
        id = NextId;
    }
}
