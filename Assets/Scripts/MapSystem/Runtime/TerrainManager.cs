using System.Collections.Generic;
using UnityEngine;

namespace MapSystem.Runtime
{
    public class TerrainManager : Singleton<TerrainManager>
    {
        private Dictionary<Vector2, TerrainChunk> m_TerrainLoaded = new Dictionary<Vector2, TerrainChunk>();
        
        public void AddTerrainTrunk(Vector2 position, TerrainChunk terrainChunk)
        {
            m_TerrainLoaded[position] = terrainChunk;
        }

        public TerrainChunk GetTerrainTrunk(Vector2 position)
        {
            if (m_TerrainLoaded.TryGetValue(position, out TerrainChunk mTerrainChunk))
            {
                return mTerrainChunk;
            }
            return default;
        }
    }
}