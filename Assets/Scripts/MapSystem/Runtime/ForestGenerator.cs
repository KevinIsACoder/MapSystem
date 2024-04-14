using UnityEngine;

namespace MapSystem.Runtime
{
    public class ForestGenerator : IMapGenerator
    {
        public Vector2 MapSize { get; set; }
        public Vector3 Coord { get; set; }
        
        public TerrainChunk Generate()
        {
            return null;
        }
    }
}