using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlotVoronoi : MonoBehaviour
{
    [Range(0, 10)] 
    public int locationCount;
    private void OnValidate()
    {
        Texture2D texture = new Texture2D(1024, 1024);
        GetComponent<Renderer>().sharedMaterial.mainTexture = texture;

        Dictionary<Vector2Int, Color> locations = new Dictionary<Vector2Int, Color>();
        while (locations.Count < locationCount)
        {
            var x = UnityEngine.Random.Range(0, texture.width);
            var y = UnityEngine.Random.Range(0, texture.height);
            var color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f),
                UnityEngine.Random.Range(0f, 1f));
            if (!locations.ContainsKey(new Vector2Int(x, y)))
            {
                locations.Add(new Vector2Int(x, y), color);
            }
        }

        for (int i = 0; i < texture.height; i++)
        {
            for (int j = 0; j < texture.width; j++)
            {
                var color = Color.white;
                var closestDistance = Mathf.Infinity;
                foreach (KeyValuePair<Vector2Int, Color> valuePair in locations)
                {
                    var distance = Vector2Int.Distance(valuePair.Key, new Vector2Int(i, j));
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        color = valuePair.Value;
                    }
                }
                texture.SetPixel(i, j, color);
            }
        }
        texture.Apply();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
