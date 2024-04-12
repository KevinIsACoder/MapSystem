using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapSystem.Runtime.Building
{
    public enum ZoneType
    {
        R,
        C,
        I
    };

    public enum PieceType
    {
        HOUSE,
        COMMERCIAL,
        INDUSTRY,
        NONE
    };

    public class BuildingGenerator : MonoBehaviour
    {
        public GameObject[] Residential;
        public GameObject[] Commercials;
        public GameObject[] Industry;
        private Dictionary<Vector3Int, PieceType> m_BuildMap = new Dictionary<Vector3Int, PieceType>();
        
        private float extentWidth = 10;

        List<List<int>> zones = new List<List<int>>();
        public Dictionary<Vector3Int, PieceType> cityMap = new Dictionary<Vector3Int, PieceType>();

        private List<Segment> m_segments = new List<Segment>();
        public void BuildHouse(List<Segment> roads)
        {
            m_segments = roads;
            //StartCoroutine(CoroutineBuildHouse(roads));
        }
        
        IEnumerator CoroutineBuildHouse(List<Segment> roads)
        {
            yield return null;
            zones.Add(new List<int> { 0 }); //residential
            zones.Add(new List<int> { 1 }); //commercials
            zones.Add(new List<int> { 2 }); //industry
            
            foreach (var segment in roads)
            {
                if (segment.IsHorizontal())
                {
                    var startX = segment.startPoint.x < segment.endPoint.x
                        ? Mathf.CeilToInt(segment.startPoint.x)
                        : Mathf.CeilToInt(segment.endPoint.x);
                    var endX = segment.endPoint.x > segment.startPoint.x
                        ? Mathf.FloorToInt(segment.endPoint.x)
                        : Mathf.FloorToInt(segment.startPoint.x);
                    for (var i = startX; i < endX; i = i + 5)
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

                        GenerateBuilding(new Vector3(i, 0, posZ));
                    }
                }
                else
                {
                    var startY = segment.startPoint.y < segment.endPoint.y
                        ? Mathf.CeilToInt(segment.startPoint.y)
                        : Mathf.CeilToInt(segment.endPoint.y);
                    var endY = segment.endPoint.y > segment.startPoint.y
                        ? Mathf.FloorToInt(segment.endPoint.y)
                        : Mathf.FloorToInt(segment.startPoint.y);
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

                        GenerateBuilding(new Vector3(posX, 0, i));
                    }
                }
            }
        }
        
        void GenerateBuilding(Vector3 location)
        {
            var x = (int)location.x;
            var z = (int)location.z;
            if (x <= 0 || x >= MapConsts.mapSize || z <= 0 || z >= MapConsts.mapSize)
                return;
            
            var pt = PieceType.NONE;
            GameObject go = null;
            
            if (IsVoronoiType(x, z, ZoneType.R))
            {
                var index = Random.Range(0, Residential.Length);
                go = Instantiate(Residential[index], location, Quaternion.identity);
                pt = PieceType.HOUSE;
            }

            if (IsVoronoiType(x, z, ZoneType.C))
            {
                var index = Random.Range(0, Commercials.Length);
                go = Instantiate(Commercials[index], location, Quaternion.identity);
                pt = PieceType.COMMERCIAL;
            }

            if (IsVoronoiType(x, z, ZoneType.I))
            {
                var index = Random.Range(0, Industry.Length);
                go = Instantiate(Industry[index], location, Quaternion.identity);
                pt = PieceType.INDUSTRY;
            }

            if (go != null)
            {
                go.transform.localScale = new Vector3(6, 6, 6);
                
                BoxCollider box = go.GetComponent<BoxCollider>();
                bool found = false;

                for (int j = (int)(-box.size.z / 2.0f); j < box.size.z / 2.0f; j++)
                {
                    for (int i = (int)(-box.size.x / 2.0f); i < box.size.x / 2.0f; i++)
                    {
                        Vector3Int mapKey = Vector3Int.RoundToInt(go.transform.position + new Vector3Int(i, 0, j));
                        if (cityMap.ContainsKey(mapKey))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (found) break;
                }
                

                if (found)
                {
                    DestroyImmediate(go);
                    go = null;
                }
                
                //判断建筑和路是否相交
                if (go != null)
                {
                    foreach (var segment in m_segments)
                    {
                        var segBound = segment.GetBounds();
                        var buildBounds = new Bounds(go.transform.position, box.bounds.size * (go.transform.localScale.x));

                        if (buildBounds.Intersects(segBound))
                        {
                            DestroyImmediate(go);
                            go = null;
                            break;
                        }
                    }
                }

                if (go != null)
                {
                    var pos = go.transform.position;
                    pos.y += 1;
                    go.transform.position = pos;
                    
                    for (int j = (int)(-box.size.z / 2.0f); j < box.size.z / 2.0f; j++)
                    {
                        for (int i = (int)(-box.size.x / 2.0f); i < box.size.x / 2.0f; i++)
                        {
                            Vector3Int mapKey = Vector3Int.RoundToInt(go.transform.position + new Vector3Int(i, 0, j));
                            cityMap.TryAdd(mapKey, pt);
                        }
                    }
                }
            }
        }

        bool IsVoronoiType(int x, int z, ZoneType type)
        {
            x = Mathf.FloorToInt(x * 1.0f / MapConsts.terrainSize);
            z = Mathf.FloorToInt(z * 1.0f / MapConsts.terrainSize);
            foreach (int t in zones[(int)type])
            {
                if (MapUtils.voronoiMap[x, z] == t)
                    return true;
            }
            
            return false;
        }
    }
}