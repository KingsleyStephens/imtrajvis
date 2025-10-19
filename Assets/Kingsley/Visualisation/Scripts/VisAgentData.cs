using System.Collections.Generic;
using UnityEngine;

namespace KVisualisation
{
    public class VisAgent : MonoBehaviour, ITimeUpdater
    {
        public enum AgentClass
        {
            Formation,
            MovingObject,
            Weapon
        }

        public AgentClass agentClass = AgentClass.Formation;

        public int playerId = -1;
        public int id = -1;
        public int parentId = -1;

        public VisClient3D VisClient3D { get; set; }

        private List<ITimeUpdater> updatables = new List<ITimeUpdater>();

        public void AddUpdatable(ITimeUpdater updatable)
        {
            updatables.Add(updatable);
        }

        public void ClearUpdatables()
        {
            updatables.Clear();
        }

        public void SetTime(float time)
        {
            foreach(var updatable in updatables)
            {
                updatable.SetTime(time);
            }
        }

        public void Reset()
        {
            
        }

    }

}
