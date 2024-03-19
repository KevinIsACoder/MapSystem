using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Threading;

public class MapChunkController
{
    public static Queue<MapChunkController> pool = new Queue<MapChunkController>();
    public static MapChunkController Create(MapConfig mapConfig, Vector2Int coord, Transform parent)
    {
        MapChunkController mapChunk;
        if (pool.Count > 0)
        {
            mapChunk = pool.Dequeue();
            mapChunk.Init(coord, parent);
        }
        else
        {
            mapChunk = new MapChunkController();
            mapChunk.Init(coord, parent);
            mapChunk.OnCreate(mapConfig);
        }
        // 生成地图
        mapChunk.StartGenerateMap();
        return mapChunk;
    }

    private MapConfig mapConfig;
    private Transform parent;
    private Terrain terrain;
    public Vector2Int coord;

    private float[,] mapData;
    private float[,] heightData;
    private float[,,] alphaData;
    private List<int[,]> detailDataList;
    private List<TreeInstance> treeInstances;

    public AnimationCurve curve;

    private void Init(Vector2Int coord, Transform parent)
    {
        this.coord = coord;
        this.parent = parent;
    }

    private void UnInit()
    {
        treeInstances.Clear();
    }

    // 显示
    public void Enable()
    {
        if (destroyCoroutine != null) // 当前正在执行销毁协程
        {
            // 关闭协程
            MapManager.Instance.StopCoroutine(destroyCoroutine);
            destroyCoroutine = null;
        }

        if (terrain != null) // 因为多线程加载，也许还没有生成地图
        {
            terrain.gameObject.SetActive(true);
        }
    }

    // 隐藏
    public void Disable()
    {
        // 隐藏自己，然后一段时间后进行销毁
        if (terrain != null)
        {
            terrain.gameObject.SetActive(false);
            destroyCoroutine = MapManager.Instance.StartCoroutine(DestroyChunk());
        }
    }

    private Coroutine destroyCoroutine;
    private IEnumerator DestroyChunk()
    {
        yield return new WaitForSeconds(mapConfig.terrainDestroyTime);
        GameObject.Destroy(terrain.terrainData);
        GameObject.Destroy(terrain.gameObject);
        UnInit();
        pool.Enqueue(this);
        MapManager.Instance.DestroyChunk(coord);
        destroyCoroutine = null;
    }

    private bool isSucced = false;
    private void StartGenerateMap()
    {
        isSucced = false;

        // 开启一个协程在主线中一直检测数据是否成功
        MapManager.Instance.StartCoroutine(DoCheckDataSucced());

        // 用多线程生成数据
        Thread thread = new Thread(new ThreadStart(
            () =>
            {
                CreateMapData();
            }
        ));
        thread.Start();
    }

    private IEnumerator DoCheckDataSucced()
    {
        while (true)
        {
            if (isSucced)
            {
                OnDataSucced();
                isSucced = false;
                yield break;
            }
            yield return null;
        }
    }

    private void OnCreate(MapConfig mapConfig)
    {
        this.mapConfig = mapConfig;
        this.curve = new AnimationCurve(mapConfig.curve.keys);
        mapData = new float[mapConfig.heightmapResolution, mapConfig.heightmapResolution];
        heightData = new float[mapConfig.heightmapResolution, mapConfig.heightmapResolution];
        alphaData = new float[mapConfig.heightmapResolution - 1, mapConfig.heightmapResolution - 1, mapConfig.template.alphamapLayers];
        detailDataList = new List<int[,]>(mapConfig.template.detailPrototypes.Length);
        treeInstances = new List<TreeInstance>(1500); // 1500是人工估算出来的
        for (int i = 0; i < mapConfig.template.detailPrototypes.Length; i++)
        {
            detailDataList.Add(new int[mapConfig.heightmapResolution - 1, mapConfig.heightmapResolution - 1]);
        }
    }

    public void GenerateMapOnEditor(MapConfig mapConfig, Terrain editorTerrin, Vector2Int coord)
    {
        this.terrain = editorTerrin;
        this.coord = coord;
        OnCreate(mapConfig);
        CreateMapData();
        OnDataSucced();
    }
    private void CreateMapData()
    {
        Vector2Int dataOffset = new Vector2Int(coord.y, coord.x) * mapConfig.heightmapResolution; // 柏林噪声的数据方向和Unity的世界方向xy颠倒
        PerlinNoiseMapGenerator.GenerateNoiseMap(mapData, mapConfig.seed, mapConfig.heightmapResolution, mapConfig.heightmapResolution, mapConfig.scale, mapConfig.layerCount, mapConfig.lacunarity, mapConfig.layerLacunarity, mapConfig.layerAmplitude, dataOffset);
        System.Random random = new System.Random(mapConfig.seed + dataOffset.GetHashCode());
        for (int x = 0; x < mapConfig.heightmapResolution; x++)
        {
            for (int z = 0; z < mapConfig.heightmapResolution; z++)
            {
                // 地形数据
                float height = curve.Evaluate(mapData[x, z]);
                heightData[x, z] = height;
                // 检查区域
                if (x != 0 && z != 0)
                {
                    CheckRegion(x - 1, z - 1, random);
                }
            }
        }
        isSucced = true;
    }

    private void CheckRegion(int x, int z, System.Random random)
    {
        float currentHeight = mapData[x, z];

        for (int i = 0; i < mapConfig.regions.Count; i++)
        {
            RegionConfig regionConfig = mapConfig.regions[i];
            float initialHeight = regionConfig.initialHegiht;
            if (currentHeight >= initialHeight) // 确定区域
            {
                // 设置贴图
                TextureConfig textureConfig = regionConfig.textureConfig;
                float lerpMaxHeight = textureConfig.lerpMaxHeight;
                bool haveNext = i != mapConfig.regions.Count - 1;
                bool wantLerp = haveNext && currentHeight < lerpMaxHeight;
                float alphaValue = 1;
                if (wantLerp && textureConfig.layerIndex != textureConfig.nextLayerIndex)
                {
                    alphaValue = (lerpMaxHeight - currentHeight) / (lerpMaxHeight - initialHeight);
                    alphaData[x, z, textureConfig.layerIndex] = 1 - alphaValue;
                    alphaData[x, z, textureConfig.nextLayerIndex] = alphaValue;
                }
                else
                {
                    alphaData[x, z, textureConfig.layerIndex] = alphaValue;
                }

                // 建筑区域
                if (regionConfig.buildingConfig.probability != 0 && regionConfig.buildingConfig.prefabs.Count != 0)
                {
                    if (x % mapConfig.buildingSpawnStep == 0 && z % mapConfig.buildingSpawnStep == 0)
                    {
                        float buildingRandomvalue = (float)random.NextDouble();
                        if (buildingRandomvalue < regionConfig.buildingConfig.probability)
                        {
                            int index = random.Next(regionConfig.buildingConfig.prefabs.Count);
                            int treeIndex = regionConfig.buildingConfig.prefabsID[index];
                            TreeInstance treeInstance = new TreeInstance();
                            treeInstance.position = new Vector3((float)z / mapConfig.detailWidth, heightData[x, z], (float)x / mapConfig.detailWidth);
                            treeInstance.prototypeIndex = treeIndex;
                            treeInstance.widthScale = 1;
                            treeInstance.heightScale = 1;
                            treeInstances.Add(treeInstance);
                        }
                    }
                    return;
                }
                // 生成树木
                if (x % mapConfig.treeSpawnStep == 0 && z % mapConfig.treeSpawnStep == 0)
                {
                    float radnomValue = (float)random.NextDouble();
                    if (radnomValue < regionConfig.treeConfig.probability && regionConfig.treeConfig.prefabs.Count != 0)
                    {
                        int index = random.Next(regionConfig.treeConfig.prefabs.Count);
                        int treeIndex = regionConfig.treeConfig.prefabsID[index];
                        TreeInstance treeInstance = new TreeInstance();
                        treeInstance.position = new Vector3((float)z / mapConfig.detailWidth, heightData[x, z], (float)x / mapConfig.detailWidth);
                        treeInstance.prototypeIndex = treeIndex;
                        treeInstance.rotation = random.Next(0, 360);
                        treeInstance.widthScale = random.Next(regionConfig.treeConfig.scaleRange.x, regionConfig.treeConfig.scaleRange.y) / 1000f;
                        treeInstance.heightScale = treeInstance.widthScale;
                        treeInstances.Add(treeInstance);
                    }
                    else // 设置石头
                    {
                        if (radnomValue < regionConfig.stoneConfig.probability && regionConfig.stoneConfig.prefabs.Count != 0)
                        {
                            int index = random.Next(regionConfig.stoneConfig.prefabs.Count);
                            int treeIndex = regionConfig.stoneConfig.prefabsID[index];
                            TreeInstance treeInstance = new TreeInstance();
                            treeInstance.prototypeIndex = treeIndex;
                            treeInstance.position = new Vector3((float)z / mapConfig.detailWidth, heightData[x, z], (float)x / mapConfig.detailWidth);
                            treeInstance.rotation = random.Next(0, 360);
                            treeInstance.widthScale = random.Next(regionConfig.stoneConfig.scaleRange.x, regionConfig.stoneConfig.scaleRange.y) / 1000f;
                            treeInstance.heightScale = treeInstance.widthScale;
                            treeInstances.Add(treeInstance);
                        }
                    }
                }
                else
                {
                    // 设置花草
                    float detailRandomValue = (float)random.NextDouble();
                    if (detailRandomValue < regionConfig.detailConfig.probability)
                    {
                        int index = random.Next(regionConfig.detailConfig.prefabs.Count);
                        int detailIndex = regionConfig.detailConfig.prefabsID[index];
                        detailDataList[detailIndex][x, z] = 1;
                    }
                }

                break;
            }
        }
    }

    private void OnDataSucced()
    {
        if (terrain == null)
        {
            terrain = GameObject.Instantiate(mapConfig.terrainPrefab).GetComponent<Terrain>();
            terrain.transform.SetParent(parent);
            terrain.transform.position = new Vector3(coord.x, 0, coord.y) * mapConfig.template.size.x;
        }

        terrain.terrainData = TerrainData.Instantiate(mapConfig.template); // 基于模版进行实例化
        terrain.terrainData.SetHeights(0, 0, heightData);
        terrain.terrainData.SetAlphamaps(0, 0, alphaData);
        for (int i = 0; i < detailDataList.Count; i++)
        {
            terrain.terrainData.SetDetailLayer(0, 0, i, detailDataList[i]);
        }

        terrain.terrainData.SetTreeInstances(treeInstances.ToArray(), false);
        terrain.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;
    }
}
