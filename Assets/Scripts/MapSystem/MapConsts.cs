using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace MapSystem
{
    public static class MapConsts
    {
        [Range(0, 1000)] 
        public static int terrainSize = 50; //地图size
        [Range(0, 100)] 
        public static int scaleFatter = 0; //地形缩放，用于柏林噪声
        [Range(0, 100)] 
        public static int offsetFatter = 5; //地形偏移
        
        [Range(0, 100)] 
        public static int terrainHeight = 5; //地形高度
        
        public static int roadWidth = 10; //路宽度

        public static int normalStreetLength = 5; //普通路段宽度

        public static int HighWayStreeetLength = 10; //高速长度

        public static int normalStreetMaxPopulationNum = 10;
    }
}