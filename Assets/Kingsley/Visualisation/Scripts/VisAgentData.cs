using UnityEngine;

namespace KVisualisation
{
    public class VisAgentData : MonoBehaviour
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
    }

}
