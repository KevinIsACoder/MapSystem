using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapSystem
{
    public class RoadData
    {
        private Vector3[] m_points;
        private Vector3[] verts;
        private Vector2[] uvs;
        private int[] tris;
        private float roadWidth = 5f;
        private int triIndex = 0;
        private int vertIndex = 0;

        public RoadData(Vector3[] points, float roadwidth)
        {
            m_points = points;
            roadWidth = roadwidth;
            verts = new Vector3[2 * points.Length]; //顶点数组
            uvs = new Vector2[verts.Length];
            tris = new int[2 * (points.Length - 1) * 3]; //三角形索引数组
        }

        public Mesh CreateRoadMesh()
        {
            for (var i = 0; i < m_points.Length; i++)
            {
                var forward = Vector3.zero; //找每个点的forward direction
                if (i < m_points.Length - 1)
                {
                    forward += m_points[i + 1] - m_points[i];
                }

                if (i > 0)
                {
                    forward += m_points[i] - m_points[i - 1];
                }

                forward.Normalize();

                var left = new Vector3(-forward.z, forward.y, forward.x);
                verts[vertIndex] = m_points[i] + left * roadWidth * 0.5f;
                verts[vertIndex + 1] = m_points[i] - left * roadWidth * 0.5f;

                float completionPercent = i / (float)(m_points.Length - 1);
                float v = 1 - Mathf.Abs(2 * completionPercent - 1);
                uvs[vertIndex] = new Vector2(0, v);
                uvs[vertIndex + 1] = new Vector2(1, v);

                if (i < m_points.Length - 1)
                {
                    tris[triIndex] = vertIndex;
                    tris[triIndex + 1] = vertIndex + 2;
                    tris[triIndex + 2] = vertIndex + 1;

                    tris[triIndex + 3] = vertIndex + 1;
                    tris[triIndex + 4] = vertIndex + 2;
                    tris[triIndex + 5] = vertIndex + 3;
                }

                vertIndex += 2; //顶点加2
                triIndex += 6;
            }

            var ms = new Mesh
            {
                vertices = verts,
                triangles = tris,
                uv = uvs
            };
            ms.RecalculateNormals();
            return ms;
        }
    }

    [Serializable]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadGenerator : MonoBehaviour
    {
        public int curveResolution = 1;
        public Material roadMaterial;

        private int m_xRoadNum = 3;
        private int m_yRoadNum = 3;

        private GameObject m_roadParent;

        private List<RoadData> m_RoadLists;
        private List<Vector3> m_splinePoints;

        public int Seed = 100;

        private PriorityQueue<Segment> m_segmentqueue;
        private List<Segment> m_generateSegment;
        private QuadTree<Segment> m_quadTreeSegment;
        
        public RoadGenerator()
        {
        }

        private void OnDrawGizmos()
        {
            if (m_splinePoints == null)
                return;
            for (int i = 0; i < m_splinePoints.Count; i++)
            {
                Gizmos.DrawSphere(m_splinePoints[i], 1);
            }
        }

        public void Generate(int mapSize, bool isClose)
        {
            /*m_roadParent = new GameObject("RoadMesh");
            m_xRoadNum = Random.Range(1, 4);
            m_yRoadNum = Random.Range(1, 4);
            for (var x = 0; x < 1; x++)
            {
                var selectPoints = new List<Vector3>();

                var chunkNum = mapSize / MapConsts.terrainSize;
                var verticalIndex = 0;
                for (var y = 0; y < chunkNum; y++)
                {
                    var trunkIndex = Random.Range(0, chunkNum - 1);
                    var selectTrunk = verticalIndex * chunkNum + trunkIndex;
                    var xCoord = trunkIndex * MapConsts.terrainSize + Random.Range(0, MapConsts.terrainSize - 1);
                    var yCoord = verticalIndex * MapConsts.terrainSize + Random.Range(0, MapConsts.terrainSize - 1);
                    selectPoints.Add(new Vector3(xCoord, MapConsts.terrainHeight, yCoord));
                    verticalIndex += 1;
                }

                m_splinePoints = CreateSplinePoints(selectPoints);
                
                for (var i = 0; i < m_splinePoints.Count; i++)
                {
                    var ray = new Ray(m_splinePoints[i], Vector3.down);
                    RaycastHit hit;
                    if (!Physics.Raycast(ray, out hit, MapConsts.terrainHeight)) continue;
                    var pos = m_splinePoints[i];
                    pos.y = hit.point.y + 2;
                    m_splinePoints[i] = pos;
                }
                
                //设置
                CreateRoad(m_splinePoints);
            }*/

        }

        public void GenerateRoad()
        {
            Random.InitState(Seed);
            m_segmentqueue = new PriorityQueue<Segment>();
            var initSegment = new Segment(new Vector2(0, 0), new Vector2(50, 50), 0, true);
            m_segmentqueue.Enqueue(initSegment);
            m_quadTreeSegment = new QuadTree<Segment>(new Rect(0, 0, 0, 0), 10, 4, 0);
            StartCoroutine(StartGenerateRoad());
        }
        
        IEnumerator StartGenerateRoad()
        {
            yield return null;
            while (!m_segmentqueue.Empty())
            {
                
                yield return null;
            }
        }

        void GenerateRoadStep()
        {
            var segment = m_segmentqueue.Dequeue();
            if (segment == null)
            {
                Debug.Log("No Segment Remain!!");
            }

            var accept = LocalConstraints(segment);
        }


        bool LocalConstraints(Segment segment)
        {
            foreach(var otherSegment in m_quadTreeSegment.Retrieve(segment.Limits))
            {
                //1 intersect
                var intersect = segment.InterSectWith(otherSegment);
                if (intersect != null)
                {
                    
                }
            }
        }

        private Segment[] InitialSegments()
        {
            var segment = new List<Segment>();
            //var firstSegment = new Segment();
            return segment.ToArray();
        }

        private void CreateRoad(List<Vector3> roadPoints)
        {
            var road = new GameObject("road");
            road.transform.SetParent(m_roadParent.transform, false);
            var roadData = new RoadData(roadPoints.ToArray(), MapConsts.roadWidth);
            var mesh = roadData.CreateRoadMesh();
            road.AddComponent<MeshCollider>();
            road.AddComponent<MeshFilter>().mesh = mesh;
            road.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;

            int textureRepeat = Mathf.RoundToInt(roadPoints.Count * .05f);
            road.GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        }

        public Mesh Combine(List<GameObject> meshes)
        {
            var combine = new CombineInstance[meshes.Count];
            for (var i = 0; i < meshes.Count; i++)
            {
                combine[i].mesh = meshes[i].GetComponent<MeshFilter>().mesh;
                combine[i].transform = meshes[i].transform.localToWorldMatrix;
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combine);
            return mesh;
        }

        //生成
        public List<Vector3> CreateSplinePoints(List<Vector3> controlPoints)
        {
            List<Vector3> points = new List<Vector3>(); //All points of the spline
            points.Clear();
            Vector3[] CurveCoordinates; //采样点的坐标
            Vector3 p0, p1, m0, m1;

            int pointsToMake;
            pointsToMake = (curveResolution) * (controlPoints.Count - 1); //要在曲线上采样这么多个点

            if (pointsToMake > 0) //Prevent Number Overflow
            {
                CurveCoordinates = new Vector3[pointsToMake];
                //根据输入的控制点，计算每段曲线需要的4个控制点
                for (int i = 0; i < controlPoints.Count - 1; i++)
                {
                    p0 = controlPoints[i];
                    p1 = controlPoints[i + 1];
                    // m0
                    if (i == 0) m0 = p1 - p0;
                    else m0 = 0.5f * (p1 - controlPoints[i - 1]);
                    // m1
                    if (i < controlPoints.Count - 2) m1 = 0.5f * (controlPoints[(i + 2) % controlPoints.Count] - p0);
                    else m1 = p1 - p0;

                    Vector3 position;
                    float t;
                    var pointStep = 1.0f / curveResolution; //0.1
                    if (i == controlPoints.Count - 2)
                    {
                        pointStep = 1.0f / (curveResolution - 1);
                    }

                    for (var j = 0; j < curveResolution; j++) //遍历10次，确定每个点的位置
                    {
                        t = j * pointStep; //j=0,t=0; j=1,t=0.1;j=2,t=0.2; t从0到0.9
                        position = MapUtils.Interpolate(p0, p1, m0, m1, t);
                        CurveCoordinates[i * curveResolution + j] = position; //0-9 10-19 20-29 30-39
                        points.Add(position);
                    }
                }
            }
            
            return points;
        }
    }
}