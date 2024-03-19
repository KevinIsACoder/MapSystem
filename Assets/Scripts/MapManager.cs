using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;
    public MapConfig mapConfig;

    private void Awake()
    {
        Instance = this;
        viewChunkNum = (int)(viewDinstance / mapConfig.template.size.x);
    }
    private void Update()
    {
        UpdateChunk();
    }
    #region Editor
    public bool randomSeed;
    public Terrain editorTerrin;
    private MapChunkController editorMapChunk;
    public Vector2Int coord;
    public void GenerateMapOnEditor()
    {
        InitConfig();
        editorMapChunk = new MapChunkController();
        editorMapChunk.GenerateMapOnEditor(mapConfig, editorTerrin, coord);
    }
    #endregion

    private void InitConfig()
    {
        if (randomSeed)
        {
            mapConfig.seed = System.DateTime.Now.GetHashCode();
        }

        mapConfig.heightmapResolution = mapConfig.template.heightmapResolution;
        mapConfig.detailWidth = mapConfig.template.detailWidth;

        // 对区域的高度进行排序，倒序
        mapConfig.regions.Sort((a, b) =>
        {
            return -a.initialHegiht.CompareTo(b.initialHegiht);
        });

        // 创建TerrainLayer
        List<TerrainLayer> terrainLayers = new List<TerrainLayer>();
        Dictionary<TerrainLayer, int> terrainLayerDic = new Dictionary<TerrainLayer, int>();
        for (int i = 0; i < mapConfig.regions.Count; i++)
        {
            TerrainLayer terrainLayer = mapConfig.regions[i].textureConfig.terrainLayer;
            if (!terrainLayerDic.TryGetValue(terrainLayer, out int terrainLayerIndex))
            {
                terrainLayerIndex = terrainLayers.Count;
                terrainLayerDic.Add(terrainLayer, terrainLayerIndex);
                terrainLayers.Add(terrainLayer);
            }
            mapConfig.regions[i].textureConfig.layerIndex = terrainLayerIndex;
            // 设置上一个layer的nextIndex是我
            if (i != 0) // 避免第一个
            {
                mapConfig.regions[i - 1].textureConfig.nextLayerIndex = terrainLayerIndex;
            }
        }
        mapConfig.template.terrainLayers = terrainLayers.ToArray();

        // 创建DetailPrototype
        List<DetailPrototype> detailPrototypes = new List<DetailPrototype>();
        Dictionary<GameObject, int> detailDic = new Dictionary<GameObject, int>();
        for (int i = 0; i < mapConfig.regions.Count; i++)
        {
            mapConfig.regions[i].detailConfig.prefabsID.Clear();
            for (int d = 0; d < mapConfig.regions[i].detailConfig.prefabs.Count; d++)
            {
                GameObject prefab = mapConfig.regions[i].detailConfig.prefabs[d];
                if (!detailDic.TryGetValue(prefab, out int detailIndex))
                {
                    detailIndex = detailPrototypes.Count;
                    detailDic.Add(prefab, detailIndex);
                    DetailPrototype detailPrototype = new DetailPrototype();
                    detailPrototype.prototype = prefab;
                    detailPrototype.usePrototypeMesh = true;
                    detailPrototype.useInstancing = true;
                    detailPrototype.renderMode = DetailRenderMode.VertexLit;
                    detailPrototype.minWidth = mapConfig.detailScale / 2;
                    detailPrototype.minHeight = mapConfig.detailScale / 2;
                    detailPrototype.maxWidth = mapConfig.detailScale;
                    detailPrototype.maxHeight = mapConfig.detailScale;
                    detailPrototypes.Add(detailPrototype);
                }
                mapConfig.regions[i].detailConfig.prefabsID.Add(detailIndex);
            }
        }
        mapConfig.template.detailPrototypes = detailPrototypes.ToArray();

        // 树木
        List<TreePrototype> treePrototypes = new List<TreePrototype>();
        Dictionary<GameObject, int> treeDic = new Dictionary<GameObject, int>();
        for (int i = 0; i < mapConfig.regions.Count; i++)
        {
            mapConfig.regions[i].treeConfig.prefabsID.Clear();
            for (int d = 0; d < mapConfig.regions[i].treeConfig.prefabs.Count; d++)
            {
                GameObject prefab = mapConfig.regions[i].treeConfig.prefabs[d];
                if (!treeDic.TryGetValue(prefab, out int treeIndex))
                {
                    treeIndex = treePrototypes.Count;
                    treeDic.Add(prefab, treeIndex);
                    TreePrototype treePrototype = new TreePrototype();
                    treePrototype.prefab = prefab;
                    treePrototype.navMeshLod = prefab.GetComponent<LODGroup>().lodCount - 1;
                    treePrototypes.Add(treePrototype);
                }
                mapConfig.regions[i].treeConfig.prefabsID.Add(treeIndex);
            }
        }

        // 石头
        for (int i = 0; i < mapConfig.regions.Count; i++)
        {
            mapConfig.regions[i].stoneConfig.prefabsID.Clear();
            for (int d = 0; d < mapConfig.regions[i].stoneConfig.prefabs.Count; d++)
            {
                GameObject prefab = mapConfig.regions[i].stoneConfig.prefabs[d];
                if (!treeDic.TryGetValue(prefab, out int stoneIndex))
                {
                    stoneIndex = treePrototypes.Count;
                    treeDic.Add(prefab, stoneIndex);
                    TreePrototype treePrototype = new TreePrototype();
                    treePrototype.prefab = prefab;
                    treePrototype.navMeshLod = prefab.GetComponent<LODGroup>().lodCount - 1;
                    treePrototypes.Add(treePrototype);
                }
                mapConfig.regions[i].stoneConfig.prefabsID.Add(stoneIndex);
            }
        }

        // 石头
        for (int i = 0; i < mapConfig.regions.Count; i++)
        {
            mapConfig.regions[i].buildingConfig.prefabsID.Clear();
            for (int d = 0; d < mapConfig.regions[i].buildingConfig.prefabs.Count; d++)
            {
                GameObject prefab = mapConfig.regions[i].buildingConfig.prefabs[d];
                if (!treeDic.TryGetValue(prefab, out int buildingIndex))
                {
                    buildingIndex = treePrototypes.Count;
                    treeDic.Add(prefab, buildingIndex);
                    TreePrototype treePrototype = new TreePrototype();
                    treePrototype.prefab = prefab;
                    treePrototype.navMeshLod = prefab.GetComponent<LODGroup>().lodCount - 1;
                    treePrototypes.Add(treePrototype);
                }
                mapConfig.regions[i].buildingConfig.prefabsID.Add(buildingIndex);
            }
        }

        mapConfig.template.treePrototypes = treePrototypes.ToArray();
    }


    public float viewDinstance; // 视野距离 米
    private int viewChunkNum;   // 能看到多少个地图块
    private List<MapChunkController> lastVisibileChunkList = new List<MapChunkController>();
    private Dictionary<Vector2Int, MapChunkController> allChunk = new Dictionary<Vector2Int, MapChunkController>();
    private new Camera camera => Camera.main;
    private Vector3 lastCameraPosition;
    private void UpdateChunk()
    {
        if (camera.transform.position == lastCameraPosition) return;
        lastCameraPosition = camera.transform.position;

        Vector2Int playerChunkCoord = GetMapChunkCoordByWorldPostion(lastCameraPosition);
        // 关闭没必要显示的chunk
        for (int i = 0; i < lastVisibileChunkList.Count; i++)
        {
            Vector2Int coord = lastVisibileChunkList[i].coord;
            if (Mathf.Abs(coord.x - playerChunkCoord.x) > viewChunkNum
                || Mathf.Abs(coord.y - playerChunkCoord.y) > viewChunkNum)
            {
                // 设置隐藏
                lastVisibileChunkList[i].Disable();
            }
        }
        lastVisibileChunkList.Clear();

        // 开启需要显示的chunk
        int startX = playerChunkCoord.x - viewChunkNum;
        int startY = playerChunkCoord.y - viewChunkNum;

        for (int x = 0; x < 2 * viewChunkNum + 1; x++)
        {
            for (int y = 0; y < 2 * viewChunkNum + 1; y++)
            {
                Vector2Int coord = new Vector2Int(startX + x, startY + y);
                // 已经存在
                if (allChunk.TryGetValue(coord, out MapChunkController mapChunk))
                {
                    mapChunk.Enable();
                }
                // 还不存在
                else
                {
                    mapChunk = MapChunkController.Create(mapConfig, coord, transform);
                    allChunk.Add(coord, mapChunk);
                }
                lastVisibileChunkList.Add(mapChunk);
            }
        }

    }

    public void DestroyChunk(Vector2Int coord)
    {
        allChunk.Remove(coord);
    }

    private Vector2Int GetMapChunkCoordByWorldPostion(Vector3 worldPostion)
    {
        int x = Mathf.FloorToInt(worldPostion.x / mapConfig.template.size.x);
        int z = Mathf.FloorToInt(worldPostion.z / mapConfig.template.size.x);
        return new Vector2Int(x, z);
    }
}
