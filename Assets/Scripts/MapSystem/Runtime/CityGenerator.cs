using System.Collections.Generic;
using UnityEngine;

namespace MapSystem.Runtime
{
    public class CityGenerator : IMapGenerator
    {
        public Vector3 Coord { get; set; }
        public Vector2 MapSize { get; set; }
        
        private int m_districtNum;
        private Material cityMaterial;
        public Dictionary<MapGenerator.EDistrict, TerrainChunk> m_terrainTrunks = new Dictionary<MapGenerator.EDistrict, TerrainChunk>();

        public List<Vector3> trunkLocation = new List<Vector3>()
        {
             new Vector3(0, 0, 0),
             new Vector3(0, 0, 1),
             new Vector3(1, 0, 0),
             new Vector3(1, 0,1)
        };
            
        public CityGenerator(Vector3 coord, Vector2 mapSize, int mDistrictNum, Material material)
        {
            Coord = coord;
            MapSize = mapSize;
            m_districtNum = mDistrictNum;
            cityMaterial = material;
        }
        
        public TerrainChunk Generate()
        {
            m_terrainTrunks.Clear();
            MapUtils.GenerateVoronoiMap(m_districtNum, (int)MapSize.x, (int)MapSize.y);
            var cityParent = new GameObject("CityMap");
            var trunkSize = (int) (MapSize.x / (m_districtNum * 0.5));
            var coord = Coord;
            for (int i = 0; i < m_districtNum; i++)
            {
                coord = trunkSize * trunkLocation[i];
                TerrainChunk trunk = null;
                if (i == (int)MapGenerator.EDistrict.Commercial)
                {
                    var noiseMap = MapUtils.GeneratePerlinValue(trunkSize,
                        trunkSize, MapConsts.scaleFatter, 4, 200, 2, 100);
                    trunk = new TerrainChunk(coord, trunkSize, 10, cityParent.transform, cityMaterial, noiseMap);
                }
                else if (i == (int)MapGenerator.EDistrict.Residential)
                {
                    trunk = new TerrainChunk(coord, trunkSize, 10, cityParent.transform, cityMaterial);
                }
                else if (i == (int)MapGenerator.EDistrict.Industrial)
                {
                    trunk = new TerrainChunk(coord, trunkSize, 10, cityParent.transform, cityMaterial);
                }
                else if (i == (int)MapGenerator.EDistrict.Park)
                {
                    var noiseMap = MapUtils.GeneratePerlinValue(trunkSize,
                        trunkSize, MapConsts.scaleFatter, 4, 200, 2, 100);
                    trunk = new TerrainChunk(coord, trunkSize, 10, cityParent.transform, cityMaterial, noiseMap);
                }

                if (trunk != null)
                {
                    m_terrainTrunks.Add((MapGenerator.EDistrict)i, trunk);
                    TerrainManager.Instance.AddTerrainTrunk(coord, trunk);
                }
            }

            foreach (var trunk in m_terrainTrunks)
            {
                trunk.Value.GenerateMesh();
            }

            return null;
        }
    }
}