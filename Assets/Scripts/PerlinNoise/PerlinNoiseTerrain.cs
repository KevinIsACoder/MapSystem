using System;
using UnityEngine;
using Random = System.Random;

namespace PerlinNoise
{
    //用柏林噪声函数生成不同地形
    [Serializable]
    public class PerlinNoiseTerrain : MonoBehaviour
    {
        private Terrain m_Terrain;
        private TerrainData m_TerrainData;

        public LineRenderer lineRenderer;
        public int positionCount = 5;
        public AnimationCurve animCurve;
        public float lacunarity = 0.01f; //采样间隙
        public float layerCount = 3f; //层数
        public float layerLacunarity = 0.01f; //层采样间隙
        public float layerAmplitude = 0.02f; //采样幅度
        public float terrainHeight = 50;
        
        private Vector3 m_heightMapScale = Vector3.one;

        
        [ContextMenu("GenerateLine")]
        private void GenerateLine()
        {
            Vector3[] positions = new Vector3[positionCount];
            float[] layeroffset = new float[positionCount];
            lineRenderer.positionCount = positionCount;

            for (int i = 0; i < layerCount; i++)
            {
                layeroffset[i] = UnityEngine.Random.Range(0, 1000f);
            }
            
            float sampleHeight = 0;

            float maxNosiHeight = float.MinValue;
            for (int i = 0; i < positionCount; i++)
            {
                float curLayerLacunarity = 1;
                float curLayerAmplitude = 1;
                for (int j = 0; j < layerCount; j++)
                {
                    sampleHeight += Mathf.PerlinNoise(i * lacunarity * curLayerLacunarity + layeroffset[j], 0);
                    curLayerLacunarity *= layerLacunarity;
                    curLayerAmplitude *= layerAmplitude;
                }

                if (maxNosiHeight < sampleHeight) maxNosiHeight = sampleHeight;
                positions[i] = new Vector3(0, sampleHeight, i * 0.1f);
            }   

            for (int i = 0; i < positionCount; i++)
            {
                float normalLise = Mathf.Clamp01(positions[i].y / maxNosiHeight);
                positions[i].y = animCurve.Evaluate(normalLise) * terrainHeight;
                positions[i] = positions[i];
            }

            lineRenderer.SetPositions(positions);
        }
    }
}