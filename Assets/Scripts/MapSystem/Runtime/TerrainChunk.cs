using UnityEngine;

namespace MapSystem.Runtime
{
    //地块
    public class TerrainChunk
    {
        public GameObject trunkObject;
        private MeshData meshData;
        public MeshData MeshData
        {
            get
            {
                return meshData;
            }
        }
        
        private Material terrainMaterial;
        private int m_chunkSize;
        private Transform m_parent;
        private Vector3 m_position;
        public Vector3 Position => m_position;

        private MapGenerator.EDistrict m_districtType;
        public MapGenerator.EDistrict DistrictType => m_districtType;
        public TerrainChunk(Vector3 position, int chunkSize, int meshSize, Transform parent, Material material = null, float[,] noiseMap = null, MapGenerator.EDistrict districtType = MapGenerator.EDistrict.Sand)
        {
            terrainMaterial = material;
            m_chunkSize = chunkSize;
            m_parent = parent;
            m_position = position;
            m_districtType = districtType;
            meshData = new MeshData(m_chunkSize, m_chunkSize, meshSize, m_position.x, m_position.z, noiseMap, m_districtType);
        }

        public void GenerateMesh(float[,] noiseMap = null)
        {
            trunkObject= new GameObject("TerrainTrunk")
            {
                isStatic = true
            };
            trunkObject.transform.SetParent(m_parent, false);
            trunkObject.transform.position = m_position;
            var mesh = meshData.GenerateNoiseMesh(noiseMap);
            var terrain = new GameObject("Terrain");
            terrain.AddComponent<MeshFilter>().mesh = mesh;
            terrain.AddComponent<MeshRenderer>().sharedMaterial = terrainMaterial;
            terrain.AddComponent<MeshCollider>();
            terrain.transform.SetParent(trunkObject.transform, false);
        }
        
        public void Destroy()
        {
            Object.DestroyImmediate(trunkObject);
        }
    }
}