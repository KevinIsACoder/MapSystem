using UnityEngine;

namespace MapSystem.Runtime
{
    public static class MapConsts
    {
        [Range(0, 1000)] 
        public static int terrainSize = 50; //地图size
        [Range(0, 1000)] 
        public static int mapSize = 500;
        [Range(0, 100)] 
        public static int scaleFatter = 5; //地形缩放，用于柏林噪声
        [Range(0, 100)] 
        public static int offsetFatter = 5; //地形偏移
        
        [Range(0, 100)] 
        public static int terrainHeight = 5; //地形高度
        
        public static int roadWidth = 5; //路宽度

        public static int normalStreetLength = 10; //普通路段宽度

        public static int HighWayStreeetLength = terrainSize; //高速长度

        /** number of possible new segments to search for maximum population */
        public static int HIGHWAY_POPULATION_SAMPLE_SIZE = 1;

        public static float HIGHWAY_POPULATION_THRESOLD = 50f; //正常街的长度

        /** probability of branching from highways */
        public static float HIGHWAY_BRANCH_PROBABILITY = 0.02f;

        public static float NORMAL_BRANCH_POPULATION_THRESHOLD = 0.04f;
        public static float DEFAULT_BRANCH_PROBABILITY = 0.4f;

        public static bool onlyHighWay = false;

        public static Bound QUADTREE_PARAMS = new Bound(0, 0, mapSize, mapSize);
        public static int QUADTREE_MAX_OBJECTS = 10;
        public static int QUADTREE_MAX_LEVELS = 10;
    }
}