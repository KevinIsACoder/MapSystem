using UnityEngine;

namespace MapSystem.Runtime
{
    //地块
    public class TerrainChunk
    {
        public GameObject trunkObject;
        private MeshData meshData;
        private Material terrainMaterial;

        public TerrainChunk(Vector3 position, int chunkSize, float mapWidth, Transform parent, Material material)
        {
            trunkObject= new GameObject("TerrainTrunk")
            {
                isStatic = true
            };
            trunkObject.transform.SetParent(parent, false);
            trunkObject.transform.position = position;
            terrainMaterial = material;
            meshData = new MeshData(chunkSize, chunkSize, mapWidth, position.x, position.z);
            GenerateMesh();
        }

        private void GenerateMesh()
        {
            var mesh = meshData.GenerateNoiseMesh();
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