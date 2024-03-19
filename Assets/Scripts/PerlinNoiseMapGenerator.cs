using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerlinNoiseMapGenerator
{
    public static void GenerateNoiseMap(float[,] noiseMap, int seed, int widht, int height, float scale, int layerCount, float lacunarity, float layerLacunarity, float layerAmplitude, Vector2Int offset)
    {
        System.Random random = new System.Random(seed);

        Vector2[] layerOffsets = new Vector2[layerCount];
        float maxPossibleNoiseValue = 0;    // 理论上的最大噪声值
        float amplitude = 1;
        // 确定层数数据
        for (int i = 0; i < layerCount; i++)
        {
            layerOffsets[i] = new Vector2(random.Next(0, 100000), random.Next(0, 100000));
            maxPossibleNoiseValue += amplitude;
            amplitude *= layerAmplitude;
        }

        for (int x = 0; x < widht; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float noiseHeight = 0;

                int indexX = x + offset.x;
                int indexY = y + offset.y;
                // 处理接缝问题：就是把开头放到前一个line的结尾
                if (x == 0) indexX -= 1;
                if (y == 0) indexY -= 1;

                float currlayerLacunarity = 1;
                float currlayerAmplitude = 1;
                for (int i = 0; i < layerCount; i++)
                {
                    float layerX = ((indexX * lacunarity * currlayerLacunarity) + layerOffsets[i].x) * scale;
                    float layerY = ((indexY * lacunarity * currlayerLacunarity) + layerOffsets[i].y) * scale;
                    noiseHeight += (Mathf.PerlinNoise(layerX, layerY) * 2 - 1) * currlayerAmplitude;
                    currlayerLacunarity *= layerLacunarity;
                    currlayerAmplitude *= layerAmplitude;
                }

                noiseHeight = (noiseHeight + 1) / maxPossibleNoiseValue; // 约束在0~1
                noiseMap[x, y] = noiseHeight;
            }
        }


    }
}
