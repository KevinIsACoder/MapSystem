using System;
using UnityEngine;
using MapSystem;

namespace CityMaps
{
    [ExecuteInEditMode]
    public class PerlingNoiseTest : MonoBehaviour
    {
        [Range(0f, 1f)] public float cutoffRed = 0.3f;
        [Range(0f, 1f)] public float cutoffBlue = 0.5f;

        [Range(0f, 0.1f)] public float xScale = 0.1f;
        [Range(0f, 0.1f)] public float yScale = 0.1f;

        [Range(0, 1000)] public float xOffset = 10;

        [Range(0, 1000)] public float yOffset = 10;
        [Range(0, 8)] public int octaves = 2;
        private void OnValidate()
        {
            Texture2D texture = new Texture2D(1024, 1024);
            GetComponent<Renderer>().sharedMaterial.mainTexture = texture;

            Color colour = Color.black;
            float perlin;
            for (int i = 0; i < texture.height; i++)
            {
                for (int j = 0; j < texture.width; j++)
                {
                    perlin = MapUtils.FBM((i + xOffset) * xScale , (j + yOffset) * yScale, octaves);
                    if(perlin < cutoffRed) colour = Color.red;
                    else if (perlin < cutoffBlue) colour = Color.blue;
                    texture.SetPixel(i, j, colour);        
                }
            }
            
            texture.Apply();
        }
    }
}