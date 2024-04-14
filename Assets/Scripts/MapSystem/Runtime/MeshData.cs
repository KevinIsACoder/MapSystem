using System.Drawing;
using System.Numerics;
using UnityEngine;

namespace MapSystem.Runtime
{
    public class MeshData
    {
        public int[] triangles; //三角形
        public Vector3[] vertices; //顶点
        public Vector2[] uvs; //uv
        public Color[] colours; //顶点颜色
        public Vector4[] tangents; //切线
        
        private int meshWidth = 0, meshHeight = 0;
        private Vector3[] m_vertices;
        private float m_offsetX, m_offsetZ;

        private float m_mapWidth; //地图大小

        public float[,] verticeHeight = null;

        private MapGenerator.EDistrict m_districtType;

        public bool isChanged = false;
        public MeshData(int width, int height)
        {
            vertices = new Vector3[(width + 1) * (height + 1)];
            uvs = new Vector2[vertices.Length];
            tangents = new Vector4[vertices.Length];
            colours = new Color[vertices.Length];
            triangles = new int[width * height * 6]; //三角形个数
            meshWidth = width;
            meshHeight = height;
        }
        
        public MeshData(int width, int height, int meshSize, float offsetX, float offsetZ, float[,] noiseMap, MapGenerator.EDistrict district)
        {
            meshWidth = meshSize;
            meshHeight = meshSize;
            vertices = new Vector3[(meshWidth + 1) * (meshWidth + 1)];
            uvs = new Vector2[vertices.Length];
            tangents = new Vector4[vertices.Length];
            colours = new Color[vertices.Length];
            triangles = new int[width * height * 6];
            m_offsetX = offsetX;
            m_offsetZ = offsetZ;
            m_mapWidth = width;
            m_districtType = district;
            verticeHeight = new float[width + 1, height + 1];
            
            TerrainChunk leftChunk = TerrainManager.Instance.GetTerrainTrunk(new Vector2((m_offsetX - meshWidth), m_offsetZ));
            TerrainChunk bottomChunk = TerrainManager.Instance.GetTerrainTrunk(new Vector2(m_offsetX, (m_offsetZ - meshHeight)));
            
            
            //生成顶点数据
            var vertIndex = 0;

            for (var y = 0; y <= meshHeight; y++)
            {
                for (var x = 0; x <= meshWidth; x++)
                {
                    var vertexHeight = noiseMap[x, y] == null ? 0 : noiseMap[x, y] * MapConsts.terrainHeight;                  
                    vertices[vertIndex] = new Vector3((x * 1f / meshWidth) * m_mapWidth, vertexHeight, (y * 1f / meshHeight) * m_mapWidth);
                    vertIndex++;
                }
            }
            
        }
        
        //创建三角形
        private void CreateTriangle()
        {
            for (int ti = 0, vi = 0, y = 0; y < meshHeight; y++, vi++)
            {
                for (var x = 0; x < meshWidth; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 1] = triangles[ti + 4] = vi + meshWidth + 1;
                    triangles[ti + 5] = vi + meshWidth + 2;
                }
            }
        }

        public Mesh GenerateNoiseMesh(float[,] noiseMap)
        {
            //检测临界的地块
            TerrainChunk leftChunk = TerrainManager.Instance.GetTerrainTrunk(new Vector2((m_offsetX - meshWidth), m_offsetZ));
            if (leftChunk != null)
            {
              // CheckVertice(0, leftChunk);   
            }
            
            TerrainChunk rightChunk = TerrainManager.Instance.GetTerrainTrunk(new Vector2((m_offsetX + meshWidth), m_offsetZ));
            if (rightChunk != null)
            {
               // CheckVertice(1, rightChunk);   
            }

            //生成三角形
            CreateTriangle();
            
            //生成UV
            for (var i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2((float)vertices[i].x / meshWidth, (float)vertices[i].z / meshHeight); //生成UV
                tangents[i] = new Vector4(1f, 0f, 0f, -1f);
            }
            
            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs,
                colors = colours,
                tangents = tangents
            };
            
            mesh.RecalculateNormals(); //重新计算法线
            return mesh;
        }

        void CheckVertice(int towards, TerrainChunk trunk)
        {
            if (towards == 0) //left
            {
                for (var y = 0; y <= meshHeight; y++)
                {
                    var x = 0;
                    var posY = y;
                    verticeHeight[x, posY] = trunk.MeshData.verticeHeight[x, posY];
                }
            }
            else if (towards == 1) //right
            {
                for (var y = 0; y <= meshWidth; y++)
                {
                    var x = (int)m_offsetX + meshWidth;
                    var posY = (int)m_offsetZ + y;
                    verticeHeight[x, posY] = trunk.MeshData.verticeHeight[x, posY];
                }
            }
            else if (towards == 2) //top
            {
                for (var y = 0; y <= meshWidth; y++)
                {
                    var x = (int)m_offsetX + meshWidth;
                    var posY = (int)m_offsetZ + y;
                    verticeHeight[x, posY] = trunk.MeshData.verticeHeight[x, posY];
                }
            }
            else if (towards == 3) //bottom
            {
                for (var y = 0; y <= meshWidth; y++)
                {
                    var x = (int)m_offsetX + meshWidth;
                    var posY = (int)m_offsetZ + y;
                    verticeHeight[x, posY] = trunk.MeshData.verticeHeight[x, posY];
                }
            }
            
        }
    }
}