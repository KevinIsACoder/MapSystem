using System.Collections.Generic;
using UnityEngine;

namespace MapSystem.Runtime
{
    public class TerrainManager : Singleton<TerrainManager>
    {
        private Dictionary<Vector3, TerrainChunk> m_TerrainLoaded = new Dictionary<Vector3, TerrainChunk>();
        
        public void AddTerrainTrunk(Vector3 position, TerrainChunk terrainChunk)
        {
            m_TerrainLoaded[position] = terrainChunk;
        }

        public TerrainChunk GetTerrainTrunk(Vector3 position)
        {
            if (m_TerrainLoaded.TryGetValue(position, out TerrainChunk mTerrainChunk))
            {
                return mTerrainChunk;
            }
            return default;
        }
    }
}