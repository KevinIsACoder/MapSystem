using UnityEngine;

namespace MapSystem.Runtime
{
    public static class MathUtil
    {
        public static bool EqualV(Vector2 a, Vector2 b)
        {
            var x = a.x - b.x;
            var y = a.y - b.y;
            return 1e-8 > LengthV2(new Vector2(x, y));
        }

        public static float LengthV2(Vector2 point)
        {
            return point.x * point.x + point.y * point.y;
        }

        public static Vector2 SubtractPoints(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
        
        //叉乘
        public static float CrossProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
        
        //点与线的位置关系
        /// <summary>
        /// 返回 0 在线上  <0 在线的左侧, >0 在线的右侧
        /// </summary>
        /// <param name="point"></param>
        /// <param name="lineStart"></param>
        /// <param name="lineEnd"></param>
        /// <returns></returns>
        public static int GetPointOnlinePosition(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            var crossValue = CrossProduct(point - lineStart, lineEnd - lineStart);
            if (crossValue > 0) return 1;
            if (crossValue < 0) return -1;
            return 0;
        }
    }
}