﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapSystem.Building
{
    public enum PieceType
    {
        ROAD,
        GRASS
    }
    public class BuildingGenerator : MonoBehaviour
    {
        public GameObject[] buildingList;
        private Dictionary<Vector3Int, PieceType> m_CityMap = new Dictionary<Vector3Int, PieceType>();

        private float extentWidth = 10f;
        public void BuildingHouse(List<Segment> roads)
        {
            var location = Vector3.zero;
            foreach (var segment in roads)
            {
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
                        var building = buildingList[Random.Range(0, 4)];
                        var newbuilding = GameObject.Instantiate(building, location, Quaternion.identity);
                        
                    }
                }
                else
                {
                    
                }
            }
        }
    }
}