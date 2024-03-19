using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapConfig
{
    public int seed;
    public TerrainData template;
    public GameObject terrainPrefab;
    public float scale;
    public int layerCount;
    public float lacunarity;
    public float layerLacunarity;
    public float layerAmplitude;
    public AnimationCurve curve;

    public float detailScale;
    public int treeSpawnStep;
    public int buildingSpawnStep;
    public float terrainDestroyTime;
    public List<RegionConfig> regions;

    #region 共享参数-不需要序列化
    [HideInInspector] public int heightmapResolution;
    [HideInInspector] public int detailWidth;
    #endregion
}
[Serializable]
public class RegionConfig
{
    // 起始高度
    public float initialHegiht;
    public TextureConfig textureConfig;
    public DetailConfig detailConfig;
    public TreeConfig treeConfig;
    public StoneConfig stoneConfig;
    public BuildingConfig buildingConfig;
}

[Serializable]
public class TextureConfig
{
    public TerrainLayer terrainLayer;
    public float lerpMaxHeight;
    [HideInInspector] public int layerIndex;
    [HideInInspector] public int nextLayerIndex;
}

[Serializable]
public class DetailConfig
{
    [Range(0f, 1f)] public float probability; // 0~1
    public List<GameObject> prefabs;
    [HideInInspector] public List<int> prefabsID;
}

[Serializable]
public class TreeConfig
{
    [Range(0f, 1f)] public float probability; // 0~1
    public List<GameObject> prefabs;
    [HideInInspector] public List<int> prefabsID;
    public Vector2Int scaleRange = new Vector2Int(500, 700);
}

[Serializable]
public class StoneConfig
{
    [Range(0f, 1f)] public float probability; // 0~1
    public List<GameObject> prefabs;
    [HideInInspector] public List<int> prefabsID;
    public Vector2Int scaleRange = new Vector2Int(500, 700);
}

[Serializable]
public class BuildingConfig
{
    [Range(0f, 1f)] public float probability; // 0~1
    public List<GameObject> prefabs;
    [HideInInspector] public List<int> prefabsID;
}