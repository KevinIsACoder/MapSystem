using UnityEngine;

namespace MapSystem
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
        
    }
}