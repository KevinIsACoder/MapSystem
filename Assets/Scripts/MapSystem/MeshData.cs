using UnityEngine;

namespace MapSystem
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

        public MeshData(int width, int height, float mapWidth, float offsetX, float offsetZ)
        {
            vertices = new Vector3[(width + 1) * (height + 1)];
            uvs = new Vector2[vertices.Length];
            tangents = new Vector4[vertices.Length];
            colours = new Color[vertices.Length];
            triangles = new int[width * height * 6];
            meshWidth = width;
            meshHeight = height;
            m_offsetX = offsetX;
            m_offsetZ = offsetZ;
            m_mapWidth = mapWidth;
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

        public Mesh GenerateNoiseMesh()
        {
            //生成顶点数据
            var vertIndex = 0;
            for (var y = 0; y <= meshHeight; y++)
            {
                for (var x = 0; x <= meshWidth; x++)
                {
                    var vertexHeight = GeneratePerlinValue(x, y) * MapConsts.terrainHeight;
                    vertices[vertIndex] = new Vector3(x, vertexHeight, y);
                    vertIndex++;
                }
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

        private float GeneratePerlinValue(int x, int y)
        {
            var xCoord = (float) (x + m_offsetX) / m_mapWidth * MapConsts.scaleFatter + MapConsts.offsetFatter;
            var zCoord = (float) (y + m_offsetZ) / m_mapWidth * MapConsts.scaleFatter + MapConsts.offsetFatter;
            return Mathf.PerlinNoise(xCoord, zCoord);
        }
    }
}