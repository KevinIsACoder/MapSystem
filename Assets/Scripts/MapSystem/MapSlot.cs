using System.Collections.Generic;
using System.IO.Pipes;
using UnityEngine;

namespace MapSystem
{
    public class MapSlot
    {
        public float x, y;
        public SlotType slotType; //地块类型
        public int chunkSize; //地块大小
        public GameObject slotObject;
    }
    
    //地块
    public class TerrainChunk
    {
        public List<MapSlot> mapSlots;
        public GameObject trunkObject;
        private MeshData meshData;
        public Vector2 position;
        private Material terrainMaterial;
        private SlotType slotType;
        
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
    
    public class SlotType
    {
        public static int Road = 0; //路
        public static int House = 1; //房子
        public static int Tree = 2; //树
        public static int Grass = 3; //草
    }
}