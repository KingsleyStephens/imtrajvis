using UnityEngine;
using KingsleyStephens.Core;

namespace KingsleyStephens
{
    namespace CoreInternal
    {
        /**
         * A mock class for Updatable
         */
        public class MockUpdateable : Updateable
        {

            // PUBLIC

            public MockUpdateable()
            {
                UpdateInterval = 1.0f;
            }

            /**
             * Initialise
             */
            public override void Init() 
            {
                Debug.Log("Init()");
            }

            /**
             * Initialise
             */
            public override void Shutdown() 
            {
                Debug.Log("Shutdown()");
            }

            protected override void DoUpdate(TimeSnapshot time) 
            {
                Debug.Log("Update()");
            }

        }
    }
}
