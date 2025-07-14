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
         * Sample the spline over a list of control points
         * 
         * @param controlPoints The points to sample from
         * @param segmentsPerCurve Resolution
         */
        public static List<Vector3> Sample(List<Vector3> controlPoints, int segmentsPerCurve)
        {
            List<Vector3> result = new List<Vector3>();

            // Test if enough points for spline
            if (controlPoints.Count >= 4)
            {
                for (int i = 0; i < controlPoints.Count - 3; i++)
                {
                    for (int j = 0; j <= segmentsPerCurve; j++)
                    {
                        float t = (float)j / (float)segmentsPerCurve;
                        Vector3 point = Interpolate(controlPoints[i], controlPoints[i + 1], controlPoints[i + 2], controlPoints[i + 3], t);
                        result.Add(point);
                    }
                }
            }

            return result;
        }


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
