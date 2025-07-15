using System.Collections.Generic;
using UnityEngine;

namespace KCore
{

    /**
     * Catmull-Rom spline interpolation class
     */
    public static class CatmullRomSpline
    {
        /**
         * Returns a point between p1 and p2 using Catmull-Rom interpolation
         * 
         * @param p0 First point
         * @param p1 Second point
         * @param p2 Third point
         * @param p3 Fourth point
         * @param t Interpolator between 0 and 1
         */
        public static Vector3 Interpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            // Catmull-Rom spline formula
            return 0.5f * (2.0f * p1 + (-p0 + p2) * t + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t * t + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t * t * t);
        }
    }

}
