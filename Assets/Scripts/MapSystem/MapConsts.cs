using UnityEngine;

namespace MapSystem
{
    public static class MapConsts
    {
        [Range(0, 1000)] 
        public static int terrainSize = 50; //地图size
        [Range(0, 100)] 
        public static int scaleFatter = 30; //地形缩放，用于柏林噪声
        [Range(0, 100)] 
        public static int offsetFatter = 5; //地形偏移
        
        [Range(0, 100)] 
        public static int terrainHeight = 5; //地形高度
        
        public static int roadWidth = 5; //路宽度
    }
}