using System.Collections.Generic;
using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * Aggregate Input Interfaces
         */
        public class AggregateInputInterface : InputInterfaceMonoBehaviour
        {
            [SerializeField]
            private InputInterfaceMonoBehaviour[] inputInterfaces;

            private List<Ray> mPrevRayList = new List<Ray>();


            // POINTING

            /**
             * Get the current pointing ray
             */
            public override Ray[] GetPointerRays()
            {
                List<Ray> rayList = new List<Ray>();

                foreach (var inputInterface in inputInterfaces)
                {
                    Ray[] rays = inputInterface.GetPointerRays();
                    foreach (var ray in rays)
                    {
                        rayList.Add(ray);
                    }
                }

                int rayIndex = 0;
                if (mPrevRayList.Count > 0)
                {
                    foreach (var r in rayList)
                    {
                        if (Vector3.Distance(r.origin, mPrevRayList[rayIndex].origin) < 0.001f)
                        {
                            rayIndex++;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (rayIndex >= mPrevRayList.Count)
                    {
                        rayIndex = 0;
                    }
                }
                else
                {
                    foreach (var r in rayList)
                    {
                        mPrevRayList.Add(r);
                    }
                }

                return new Ray[1] { rayList[rayIndex] };
            }

            // SELECTION

            /**
             * Has a select been triggered
             */
            public override bool GetSelectDown()
            {
                foreach (var inputInterface in inputInterfaces)
                {
                    if (inputInterface.GetSelectDown())
                    {
                        return true;
                    }
                }

                return false;
            }

            /**
             * Has a select been released
             */
            public override bool GetSelectUp()
            {
                foreach (var inputInterface in inputInterfaces)
                {
                    if (inputInterface.GetSelectUp())
                    {
                        return true;
                    }
                }

                return false;
            }

            // SCROLLING

            /**
             * Get the current scroll delta
             */
            public override float GetScroll()
            {
                foreach (var inputInterface in inputInterfaces)
                {
                    float s = inputInterface.GetScroll();
                    if (s != 0.0f)
                    {
                        return s;
                    }
                }

                return 0.0f;
            }
        }


    }
}