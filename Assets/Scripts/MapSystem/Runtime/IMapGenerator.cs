using UnityEngine;

namespace MapSystem.Runtime
{
    public interface IMapGenerator
    {
        Vector3 Coord
        {
            get;
            set;
        }

        Vector2 MapSize
        {
            get;
            set;
        }

        TerrainChunk Generate();
    }
}