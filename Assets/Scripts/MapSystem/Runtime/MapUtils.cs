using System.Collections.Generic;
using UnityEngine;

namespace MapSystem.Runtime
{
    public static class MapUtils
    {
        public static int[,] voronoiMap = null;
        /// <summary>
        /// 生成perling噪声值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="octaves"></param>
        /// <returns></returns>
        public static float FBM(float x, float y, int octaves)
        {
            float total = 0;
            float frequency = 1;
            for (int i = 0; i < octaves; i++)
            {
                total += Mathf.PerlinNoise(x * frequency, y * frequency);
                frequency *= 2;
            }

            return total / (float)octaves;
        }
        
        // Catmull-Rom splines are Hermite curves with special tangent values.
        // Hermite curve formula:
        // (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
        // For points p0 and p1 passing through points m0 and m1 interpolated over t = [0, 1]
        // Tangent M[k] = (P[k+1] - P[k-1]) / 2
        // With [] indicating subscript
        public static Vector3 Interpolate(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
        {
            var position = (2.0f * t * t * t - 3.0f * t * t + 1.0f) * start
                               + (t * t * t - 2.0f * t * t + t) * tanPoint1
                               + (-2.0f * t * t * t + 3.0f * t * t) * end
                               + (t * t * t - t * t) * tanPoint2;

            return position;
        }
        
        /// <summary>
        /// 生成诺伊多边形区域
        /// </summary>
        /// <param name="districtNum"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void GenerateVoronoiMap(int districtNum, int width, int height)
        {
            voronoiMap = new int[width, height];
            var locations = new Dictionary<Vector2Int, int>();
            var index = 0;
            while (index < districtNum)
            {
                var x = Random.Range(0, width);
                var y = Random.Range(0, height);
                var vectorInt = new Vector2Int(x, y);
                if (!locations.ContainsKey(vectorInt))
                {
                    locations.Add(vectorInt, index);
                    index++;
                }
            }
            
            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var distance = Mathf.Infinity;
                    foreach (var valuePair in locations)
                    {
                        var distTo = Vector2Int.Distance(valuePair.Key, new Vector2Int(x, y));
                        if (distTo < distance)
                        {
                            distance = distTo;
                            voronoiMap[x, y] = valuePair.Value;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapWidth">地图宽度</param>
        /// <param name="mapHeight">地图高度</param>
        /// <param name="scale"></param>
        /// <param name="octaves">叠加层数</param>
        /// <param name="peristance">持续度</param>
        /// <param name="lacunarity">孔隙度</param>
        /// <returns></returns>
        public static float[,] GeneratePerlinValue(int mapWidth, int mapHeight, float scale, int octaves, float peristance, float lacunarity, int seed)
        {
            float maxNoiseValue = float.MinValue;
            float minNoiseValue = float.MaxValue;

            var random = new System.Random(seed);
            var octaveOffsets = new Vector2[octaves];
            for (var i = 0; i < octaves; i++)
            {
                var offsetX = random.Next(0, mapWidth);
                var offsetY = random.Next(0, mapWidth);
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }
            
            var noiseMap = new float[mapWidth, mapHeight];
            for (var x = 0; x < mapWidth; x++)
            {
                for (var y = 0; y < mapHeight; y++)
                {
                    float altitude = 1; //高度
                    float frequency = 1; //频率
                    float noiseHeight = 0;
                    for (var i = 0; i < octaves; i++)
                    {
                        var sampleX = (x +  octaveOffsets[i].x) / scale * frequency;
                        var sampleY = (y + octaveOffsets[i].y) / scale * frequency;
                        var perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * altitude;
                        if (noiseHeight > maxNoiseValue)
                        {
                            maxNoiseValue = noiseHeight;
                        }
                        else if(noiseHeight < minNoiseValue)
                        {
                            minNoiseValue = noiseHeight;
                        }
                        altitude *= peristance;
                        frequency *= lacunarity;
                    }
                    
                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (var i = 0; i < mapWidth; i++)
            {
                for (var j = 0; j < mapHeight; j++)
                {
                    noiseMap[i, j] = Mathf.InverseLerp(minNoiseValue, maxNoiseValue, noiseMap[i, j]);
                }
            }
            
            return noiseMap;
        }
    }   
}
