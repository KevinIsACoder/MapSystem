using System.Collections.Generic;
using UnityEngine;

namespace MapSystem.Runtime
{
    public class TerrainManager : Singleton<TerrainManager>
    {
        private Dictionary<Vector2, MeshData> m_TerrainLoaded;
    }
}