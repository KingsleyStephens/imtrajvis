using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SATestbed
{
    public struct TimeSnapshot
    {
        public float time;
        public float deltaTime;

        public TimeSnapshot(float time, float deltaTime)
        {
            this.time = time;
            this.deltaTime = deltaTime;
        }
    }
}