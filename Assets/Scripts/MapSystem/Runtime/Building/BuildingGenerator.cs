using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapSystem.Runtime.Building
{
    public enum ZoneType { R, C, I };
    
    public enum PieceType { ROAD, HOUSE, SHACK, LAWN, COMMERCIAL, INDUSTRY, NONE };
    public class BuildingGenerator : MonoBehaviour
    {
        public GameObject[] Residential;
        public GameObject[] Commercials;
        public GameObject[] Industry;
        private Dictionary<Vector3Int, PieceType> m_BuildMap = new Dictionary<Vector3Int, PieceType>();

        private float extentWidth = 10f;
        
        List<List<int>> zones = new List<List<int>>();
        public void BuildingHouse(List<Segment> roads)
        {
            zones.Add(new List<int> { 0, 1 }); //residential
            zones.Add(new List<int> { 2, 3 }); //commercials
            zones.Add(new List<int> { 4, 5 }); //industry
            
            var location = Vector3.zero;
            foreach (var segment in roads)
            {
                var pt = PieceType.NONE;
                if (segment.IsHorizontal())
                {
                    var startX = segment.startPoint.x < segment.endPoint.x ? Mathf.CeilToInt(segment.startPoint.x) : Mathf.CeilToInt(segment.endPoint.x);
                    var endX = segment.endPoint.x > segment.startPoint.x ? Mathf.FloorToInt(segment.endPoint.x) : Mathf.FloorToInt(segment.startPoint.x);
                    for (var i = startX; i < endX; i = i + 10)
                    {
                        float posZ = segment.startPoint.y;
                        var randNum = Random.Range(0f, 10f);
                        if (randNum > 3)
                        {
                            posZ += MapConsts.roadWidth * 0.5f + extentWidth;
                        }
                        else
                        {
                            posZ -= MapConsts.roadWidth * 0.5f + extentWidth;
                        }
                        
                        location = new Vector3(i, 0, posZ);
                    }
                }
                else
                {
                    var startY = segment.startPoint.y < segment.endPoint.y ? Mathf.CeilToInt(segment.startPoint.y) : Mathf.CeilToInt(segment.endPoint.y);
                    var endY = segment.endPoint.y > segment.startPoint.y ? Mathf.FloorToInt(segment.endPoint.y) : Mathf.FloorToInt(segment.startPoint.y);
                    for (var i = startY; i < endY; i = i + 10)
                    {
                        float posX = segment.startPoint.x;
                        var randNum = Random.Range(0f, 10f);
                        if (randNum > 3)
                        {
                            posX += MapConsts.roadWidth * 0.5f + extentWidth;
                        }
                        else
                        {
                            posX -= MapConsts.roadWidth * 0.5f + extentWidth;
                        }
                        location = new Vector3(posX, 0, i);
                    }
                }

                var x = Mathf.RoundToInt(location.x);
                var z = Mathf.RoundToInt(location.z);
                
                if (IsVoronoiType(x, z, ZoneType.R))
                {
                    var index = Random.Range(0, Residential.Length);
                    var go = Instantiate(Residential[index], location, Quaternion.identity);
                    pt = PieceType.HOUSE;
                }

                if (IsVoronoiType(x, z, ZoneType.C))
                {
                    var index = Random.Range(0, Commercials.Length);
                    var go = Instantiate(Commercials[index], location, Quaternion.identity);
                    pt = PieceType.COMMERCIAL;
                }

                if (IsVoronoiType(x, z, ZoneType.I))
                {
                    var index = Random.Range(0, Industry.Length);
                    var go = Instantiate(Industry[index], location, Quaternion.identity);
                    pt = PieceType.INDUSTRY;
                }
            }
        }
        
        bool IsVoronoiType(int x, int z, ZoneType type)
        {
            foreach (int t in zones[(int)type])
            {
                if (MapUtils.voronoiMap[x, z] == t)
                    return true;
            }
            return false;
        }

    }
}