using UnityEngine;

namespace KVisualisation
{
    public interface ITimeUpdater
    {
        public void SetTime(float time);

        public void Reset();
    }

}