using System;
using System.Collections;
using System.Collections.Generic;
using FCG;
using MapSystem.Runtime.Building;
using UnityEngine;

namespace MapSystem.Runtime
{
    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MapGenerator : MonoBehaviour
    {
        public Material terrainMaterial; //地形材质
        
        private int terrainSize = MapConsts.terrainSize;
        
        public int terrainChunkWidth = 50;
        public Dictionary<Vector3, TerrainChunk> TerrainChunk = new Dictionary<Vector3, TerrainChunk>();
        private int m_terrainTrunckNum;
        
        private RoadGenerator m_roadGenerator;
        private BuildingGenerator m_buildingGenerator;
        public int districtNum = 5; //区域数量

        public Material industrial;
        public Material commercial;
        public Material residential;
        public Material park;
        public Material sand;
        
        public enum EDistrict
        {
            Commercial,
            Residential,
            Industrial,
            Park,
            Sand
        }

        public List<TerrainChunk> m_IndustrialChunks = new List<TerrainChunk>();
        public List<TerrainChunk> m_CommercialChunks = new List<TerrainChunk>();
        public List<TerrainChunk> m_ResidentialChunks = new List<TerrainChunk>();
        public List<TerrainChunk> m_ParkChunks = new List<TerrainChunk>();
        public List<TerrainChunk> m_SandChunks = new List<TerrainChunk>();

        private CityGenerator m_cityGenerator;
        private void OnEnable()
        {
            gameObject.GetComponent<MeshRenderer>().material = terrainMaterial;
            m_roadGenerator = gameObject.GetComponent<RoadGenerator>();
            m_buildingGenerator = gameObject.GetComponent<BuildingGenerator>();
        }
        
        private void Start()
        {
            Clear();
            GenerateMap();
        }
        
        public void GenerateMap()
        {
            m_roadGenerator.OnGenerateComplete = OnRoadGenerateComplete;
            //生成地形
            GenerateTerrain();
           // m_cityGenerator = new CityGenerator(Vector3.zero, new Vector2(MapConsts.mapSize, MapConsts.mapSize), 4, terrainMaterial);
           // m_cityGenerator.Generate();
            //StartCoroutine(GenerateRoad());
        }
        
        private void OnRoadGenerateComplete()
        {
            m_buildingGenerator.BuildHouse(m_roadGenerator.GetGenerateSegments());
        }

        private IEnumerator GenerateRoad()
        {
            yield return null;
            //生成路网
            m_roadGenerator.GenerateRoad();
        }

        private void GenerateTerrain()
        {
            // m_terrainTrunckNum = Mathf.RoundToInt(terrainChunkWidth / terrainSize);
            // MapUtils.GenerateVoronoiMap(districtNum, m_terrainTrunckNum, m_terrainTrunckNum);
            // var noiseMap = MapUtils.GeneratePerlinValue(m_terrainTrunckNum * terrainSize,
            //     m_terrainTrunckNum * terrainSize, MapConsts.scaleFatter, 4, 200, 2, 100);
            //
            // for (var x = 0; x < m_terrainTrunckNum; x++)
            // {
            //     for (var y = 0; y < m_terrainTrunckNum; y++)
            //     {
            //         var pos = new Vector3(x * terrainSize, 0, y * terrainSize);
            //         var districtType = MapUtils.voronoiMap[x, y];
            //         var trunck = new TerrainChunk(pos, MapConsts.terrainSize, gameObject.transform, GetDistrictMaterial(districtType), noiseMap, (EDistrict)districtType);
            //         
            //         TerrainManager.Instance.AddTerrainTrunk(pos, trunck);
            //         switch (districtType)
            //         {
            //             case (int)EDistrict.Commercial:
            //                 m_CommercialChunks.Add(trunck);
            //                 break;
            //             case (int)EDistrict.Industrial:
            //                 m_IndustrialChunks.Add(trunck);
            //                 break;                   
            //             case (int)EDistrict.Residential:
            //                 m_ResidentialChunks.Add(trunck);
            //                 break;
            //             case (int)EDistrict.Park:
            //                 m_ParkChunks.Add(trunck);
            //                 break;
            //             case (int)EDistrict.Sand:
            //                 m_SandChunks.Add(trunck);
            //                 break;
            //         }
            //     }
            //}
            
            // foreach (var chunk in m_IndustrialChunks)
            // {
            //      chunk.GenerateMesh();
            // }
            //
            // foreach (var chunk in m_ParkChunks)
            // {
            //     chunk.GenerateMesh(noiseMap);
            // }
            //
            // foreach (var chunk in m_SandChunks)
            // {
            //     chunk.GenerateMesh(noiseMap);
            // }
            //
            // foreach (var chunk in m_CommercialChunks)
            // {
            //     chunk.GenerateMesh();
            // }
            //
            // foreach (var chunk in m_ResidentialChunks)
            // {
            //     chunk.GenerateMesh();
            // }
        }

        public void CheckTerrainTrunck()
        {
            
        }
        
        public Material GetDistrictMaterial(int districtType)
        {
            switch (districtType)
            {
                case (int)EDistrict.Industrial:
                    return industrial; //工业
                case (int)EDistrict.Commercial:
                    return commercial; //商业
                case (int)EDistrict.Residential:
                    return residential; //居民
                case (int)EDistrict.Park:
                    return park; //公园
                case (int)EDistrict.Sand:
                    return sand; //沙地
            }

            return default;
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