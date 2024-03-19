using Unity.VisualScripting;
using UnityEngine;

namespace MapSystem
{
    public static class MapUtils
    {
        /// <summary>
        /// 生成perling噪声值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="octaves"></param>
        /// <returns></returns>
        public static float FBM(float x, float y, int octaves)
        {
            float total = 0;
            float frequency = 1;
            for (int i = 0; i < octaves; i++)
            {
                total += Mathf.PerlinNoise(x * frequency, y * frequency);
                frequency *= 2;
            }

            return total / (float)octaves;
        }
        
        // Catmull-Rom splines are Hermite curves with special tangent values.
        // Hermite curve formula:
        // (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
        // For points p0 and p1 passing through points m0 and m1 interpolated over t = [0, 1]
        // Tangent M[k] = (P[k+1] - P[k-1]) / 2
        // With [] indicating subscript
        public static Vector3 Interpolate(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
        {
            var position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * start
                               + (t * t * t - 2.0f * t * t + t) * tanPoint1
                               + (-2.0f * t * t * t + 3.0f * t * t) * end
                               + (t * t * t - t * t) * tanPoint2;

            return position;
        }
        
        
    }   
}
