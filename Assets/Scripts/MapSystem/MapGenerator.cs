using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapSystem
{
    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MapGenerator : MonoBehaviour
    {
        public int seed = 100; //随机种子
        public Material terrainMaterial; //地形材质
        public int terrainHeight = 10; //地形高度
        
        private int terrainSize = MapConsts.terrainSize;
        
        public int terrainChunkWidth = 50;
        public Vector3 centerPosistion = Vector3.zero; //中心点
        public Dictionary<Vector3, TerrainChunk> TerrainChunk = new Dictionary<Vector3, TerrainChunk>();
        private int m_terrainTrunckNum;
        
        private RoadGenerator m_roadGenerator;     
        private void OnEnable()
        {
            gameObject.GetComponent<MeshRenderer>().material = terrainMaterial;
            m_roadGenerator = gameObject.GetComponent<RoadGenerator>();
        }
        
        private void Start()
        {
            Clear();
            GenerateMap();
        }
        
        public void GenerateMap()
        {
            //生成地形
            GenerateTerrain();

            StartCoroutine(GenerateRoad());
        }

        private IEnumerator GenerateRoad()
        {
            yield return null;
            //生成路线
            m_roadGenerator.Generate(terrainChunkWidth, false);
        }

        private void GenerateTerrain()
        {
            m_terrainTrunckNum = Mathf.RoundToInt(terrainChunkWidth / terrainSize);
            for (var x = 0; x < m_terrainTrunckNum; x++)
            {
                for (var y = 0; y < m_terrainTrunckNum; y++)
                {
                    var pos = new Vector3(x * terrainSize, 0, y * terrainSize);
                    
                    if (TerrainChunk.TryGetValue(pos, out var trunk))
                    {
                        trunk.Destroy();
                    }
                    else
                    {
                        var trunck = new TerrainChunk(pos, MapConsts.terrainSize, m_terrainTrunckNum * MapConsts.terrainSize, gameObject.transform, terrainMaterial);
                        TerrainChunk.Add(pos, trunck);
                    }
                }
            }
        }
        
        public void Clear()
        {
        }
        
        //生成柏林噪声值
        public float GeneratePerlinValue(int x, int y, float offsetX, float offsetZ)
        {
            var xCoord = (float)(x + offsetX) / (m_terrainTrunckNum * terrainSize)  * MapConsts.scaleFatter + MapConsts.offsetFatter;
            var zCoord = (float)(y + offsetZ) / (m_terrainTrunckNum * terrainSize ) * MapConsts.scaleFatter + MapConsts.offsetFatter;
            return Mathf.PerlinNoise(xCoord, zCoord);
        }
    }
}