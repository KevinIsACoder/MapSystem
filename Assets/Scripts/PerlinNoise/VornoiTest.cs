using System;
using System.Collections.Generic;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using Random = UnityEngine.Random;

public class VornoiTest : MonoBehaviour
{
    private Texture2D m_texture;
    public int m_locations;
    
    private void OnValidate()
    {
        m_texture = new Texture2D(1024, 1024);
        GetComponent<Renderer>().sharedMaterial.mainTexture = m_texture;
        
        var locations = new Dictionary<Vector2Int, Color>();
        
        while (m_locations < locations.Count)
        {
            var x = Random.Range(0, m_texture.width);
            var y = Random.Range(0, m_texture.height);
            var vectorInt = new Vector2Int(x, y);
            var color = new Color(Random.Range(0, 1), Random.Range(0, 1), Random.Range(0, 1));
            if (!locations.ContainsKey(vectorInt))
            {
                locations.Add(vectorInt, color);
            }
        }
            
        for (var y = 0; y < m_texture.height; y++)
        {
            for (var x = 0; x < m_texture.width; x++)
            {
                var distance = Mathf.Infinity;
                var color = Color.white;
                foreach (var valuePair in locations)
                {
                    var distTo = Vector2Int.Distance(valuePair.Key, new Vector2Int(y, x));
                    if (distTo < distance)
                    {
                        color = valuePair.Value;
                        distance = distTo;
                    }
                }
                m_texture.SetPixel(y, x, color);
            }
        }
        
        m_texture.Apply();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
