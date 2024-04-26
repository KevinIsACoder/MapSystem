using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapSystem.Runtime
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

        public RoadData(Vector3[] points, float roadwidth, bool isHighway = false)
        {
            m_points = points;
            roadWidth = isHighway ? roadwidth : roadwidth * 0.5f;
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

        public int Seed = 200;

        private PriorityQueue<Segment> m_segmentqueue;
        private List<Segment> m_generateSegment;
        private QuadTree<Segment> m_quadTreeSegment;

        public Action OnGenerateComplete;

        private float segmentLength = MapConsts.terrainSize * 2f;
        private int m_segmentNum = 0; //线段数目
        
        private void OnEnable()
        {
            m_roadParent = new GameObject("RoadParent");
            m_roadParent.transform.localPosition = new Vector3(0, 1, 0);
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
            m_generateSegment = new List<Segment>();
            m_segmentqueue = new PriorityQueue<Segment>();
            var initSegment = new Segment(new Vector2(0, MapConsts.terrainSize * 0.5f),
                new Vector2(MapConsts.terrainSize * 2f, MapConsts.terrainSize * 0.5f), true);
            m_segmentqueue.Equeue(initSegment);
            m_quadTreeSegment = new QuadTree<Segment>(MapConsts.QUADTREE_PARAMS, MapConsts.QUADTREE_MAX_OBJECTS,
                MapConsts.QUADTREE_MAX_LEVELS, 0);
            m_quadTreeSegment.Split();
            while (!m_segmentqueue.Empty())
            {
                GenerateRoadData();
            }

            StartCoroutine(StartGenerateRoad());
        }

        IEnumerator StartGenerateRoad()
        {
            yield return null;
            var roadNum = 0;
            /*while (roadNum < m_generateSegment.Count)
            {
                GenerateRoadStep(roadNum);
                roadNum++;
                yield return null;
            }*/
            var points = new List<Vector3>();
            GenerateRoadStep(points);
            
            OnGenerateComplete?.Invoke();
        }

        void GenerateRoadStep(List<Vector3> points)
        {
            var ext = 20;
            var extCorner = 2;
            for (int i = 0; i < m_generateSegment.Count; i++)
            {
                var segmentPoints = new List<Vector3>();
                var seg = m_generateSegment[i];
                var startPoint = new Vector3(seg.startPoint.x, 0, seg.startPoint.y);
                var endPoint = new Vector3(seg.endPoint.x, 0, seg.endPoint.y);
                List<Vector3> cornerPoints = null;
                
                foreach (var forwardSeg in seg.forwardSegment)
                {
                    var v1 = (seg.endPoint - seg.startPoint).normalized;
                    var v2 = (forwardSeg.endPoint - forwardSeg.startPoint).normalized;
                    var corner = new Vector3(seg.endPoint.x, 0, seg.endPoint.y);
                    if (Vector2.Dot(v1, v2) == 0)
                    {
                        var forwardStart = forwardSeg.startPoint;
                        if (seg.IsHorizontal())
                        {
                            forwardStart.y += forwardSeg.endPoint.y > forwardSeg.startPoint.y ? ext : -ext;
                            var isTowardsRight = endPoint.x > startPoint.x;
                            endPoint.x += isTowardsRight ? -ext : ext;
                            corner.x += isTowardsRight ? -extCorner : extCorner;
                            corner.z += forwardSeg.endPoint.y > forwardSeg.startPoint.y ? extCorner : -extCorner;
                        }
                        else
                        {
                            forwardStart.x += forwardSeg.endPoint.x > forwardStart.x ? ext : -ext;
                            var isTowardsUp = endPoint.z > startPoint.z;
                            endPoint.z += isTowardsUp ? -ext : ext;
                            corner.z += isTowardsUp ? -extCorner : extCorner;
                            corner.x += forwardSeg.endPoint.x > forwardSeg.startPoint.x ? extCorner : -extCorner;
                        }
                        
                        cornerPoints = CreateSplinePoints(new List<Vector3>()
                            { endPoint, corner, new Vector3(forwardStart.x, 0, forwardStart.y) });
                        break;
                    }
                }

                foreach (var backwardSeg in seg.backwardSegment)
                {
                    var v1 = (seg.endPoint - seg.startPoint).normalized;
                    var v2 = (backwardSeg.endPoint - backwardSeg.startPoint).normalized;
                    if (Vector2.Dot(v1, v2) == 0)
                    {
                        if (seg.IsHorizontal())
                        {
                            startPoint.x += endPoint.x > startPoint.x ? ext : -ext;
                        }
                        else
                        {
                            startPoint.z += endPoint.z > startPoint.z ? ext : -ext;
                        }

                        break;
                    }
                }

                segmentPoints.Add(startPoint);
                segmentPoints.Add(endPoint);
                if (cornerPoints != null)
                {
                    for (int j = 0; j < cornerPoints.Count; j++)
                    {
                        segmentPoints.Add(cornerPoints[j]);
                    }
                }
                CreateRoad(segmentPoints, seg.isHignWay, seg.GetBounds());
            }
        }

        public List<Segment> GetGenerateSegments()
        {
            return m_generateSegment;
        }

        public float GetSegmentWidth()
        {
            return segmentLength;
        }

        IEnumerator StartBuildHouse()
        {
            yield return null;
        }

        void GenerateRoadData()
        {
            var segment = m_segmentqueue.Dequeue();
            if (segment == null)
            {
                Debug.Log("No Segment Remain!!!");
                return;
            }

            var accept = LocalConstraints(segment);
            if (accept)
            {
                m_segmentNum++;
                if (segment != null && m_segmentNum < MapConsts.maxSegmentNum)
                {
                    m_generateSegment.Add(segment);
                    m_quadTreeSegment.Insert(segment.Limits, segment);

                    var destiny = PopulationAtSegment(segment);
                    if (Math.Abs(destiny) > 0.52f)
                    {
                        var seg = segment.Split(m_generateSegment, m_quadTreeSegment);
                        seg.isHignWay = false;
                        m_segmentqueue.Equeue(seg);
                    }

                    foreach (var minSegment in GlobalGoalsGenerate(segment))
                    {
                        minSegment.m_time = minSegment.m_time + segment.m_time + 1;
                        m_segmentqueue.Equeue(minSegment);
                    }
                }
            }
        }

        List<Segment> GlobalGoalsGenerate(Segment preSegment)
        {
            var newBranches = new List<Segment>();
            var segment = CreateNewSegment(preSegment);
            if (segment != null)
            {
                segment.isHignWay = preSegment.isHignWay;
                segment.m_time = preSegment.m_time + 1;
                newBranches.Add(segment);
            }

            // setup links between each current branch and each existing branch stemming from the previous segment
            for (var i = 0; i < newBranches.Count; i++)
            {
                var branch = newBranches[i];
                foreach (var preforwardSegment in preSegment.forwardSegment)
                {
                    branch.backwardSegment.Add(preforwardSegment); //
                    var containing = preforwardSegment.LinksForEndContaining(preSegment);
                    if (containing != null)
                    {
                        containing.Add(branch);
                    }
                }

                preSegment.forwardSegment.Add(branch);
                branch.backwardSegment.Add(preSegment);
            }

            return newBranches;
        }

        Segment CreateNewSegment(Segment preSegment)
        {
            if (Math.Abs(preSegment.startPoint.x - preSegment.endPoint.x) < 0.0001f) //竖线
            {
                var growType = new int[] { 0, 1, 2 }; // left, right, up/down
                Array.Sort(growType, (a, b) =>
                {
                    var num = Random.Range(0f, 1f);
                    if (num > 0.3f) return 1;
                    if (num > 0.3f && num < 0.6f) return -1;
                    return 0;
                });

                for (int i = 0; i < growType.Length; i++)
                {
                    var segment = GrowVerticalSegment(preSegment, growType[i]);
                    if (IsSegmentValid(segment))
                        return segment;
                }

                return null;
            }
            else //横线
            {
                var growType = new int[] { 0, 1, 2 }; // up, down, left/right
                Array.Sort(growType, (a, b) =>
                {
                    var num = Random.Range(0f, 1f);
                    if (num > 0.3f) return 1;
                    if (num > 0.3f && num < 0.6f) return -1;
                    return 0;
                });

                for (int i = 0; i < growType.Length; i++)
                {
                    var segment = GrowHoritalSegment(preSegment, growType[i]);
                    if (IsSegmentValid(segment))
                        return segment;
                }

                return null;
            }
        }

        Segment GrowVerticalSegment(Segment presegment, int growType)
        {
            var startPoint = presegment.endPoint;
            var endPoint = startPoint;
            if (growType == 0) //left
            {
                endPoint.x -= segmentLength;
            }
            else if (growType == 1) //right
            {
                endPoint.x += segmentLength;
            }
            else if (growType == 2) // up or down
            {
                endPoint.y += presegment.startPoint.y < presegment.endPoint.y ? segmentLength : -segmentLength;
            }

            return Segment.GenerateSegment(startPoint, endPoint);
        }

        Segment GrowHoritalSegment(Segment presegment, int growType)
        {
            var startPoint = presegment.endPoint;
            var endPoint = startPoint;
            if (growType == 0) //up
            {
                endPoint.y += segmentLength;
            }
            else if (growType == 1) //down
            {
                endPoint.y -= segmentLength;
            }
            else if (growType == 2) // left or right
            {
                endPoint.x += presegment.endPoint.x > presegment.startPoint.x ? segmentLength : -segmentLength;
            }

            return Segment.GenerateSegment(startPoint, endPoint);
        }

        bool IsSegmentValid(Segment segment)
        {
            bool isSucceed = !IsOutMap(segment);
            if (!isSucceed)
                return false;
            foreach (var other in m_generateSegment)
            {
                if (other == null) continue;
                if (segment.TryGetIntersectPoint(other, out Vector3 intersectPoint))
                {
                    isSucceed = false;
                    break;
                }
            }

            return isSucceed;
        }

        bool IsOutMap(Segment segment)
        {
            return segment.endPoint.x > MapConsts.mapSize - 1
                   || segment.endPoint.x <= 0
                   || segment.endPoint.y > MapConsts.mapSize - 1
                   || segment.endPoint.y <= 0;
        }

        float PopulationAtSegment(Segment segment)
        {
            var start = Noise.Generate(segment.startPoint.x, segment.startPoint.y);
            var end = Noise.Generate(segment.endPoint.x, segment.endPoint.y);
            return (start + end) * 0.6f;
        }

        bool LocalConstraints(Segment segment)
        {
            if (segment.endPoint.y == 0 || segment.startPoint.y >= MapConsts.mapSize - 1)
            {
                return false;
            }

            if (segment.startPoint.x >= MapConsts.mapSize - 1)
            {
                return false;
            }

            return true;
        }

        private Segment[] InitialSegments()
        {
            var segment = new List<Segment>();
            //var firstSegment = new Segment();
            return segment.ToArray();
        }

        private void CreateRoad(List<Vector3> roadPoints, bool isHighWay, Bounds bounds)
        {
            var road = new GameObject("road");
            road.transform.SetParent(m_roadParent.transform, false);
            var roadData = new RoadData(roadPoints.ToArray(),  isHighWay ? MapConsts.roadWidth : MapConsts.normalRoadWidth, isHighWay);
            var mesh = roadData.CreateRoadMesh();
            road.AddComponent<MeshCollider>();
            road.AddComponent<MeshFilter>().mesh = mesh;
            road.AddComponent<MeshRenderer>().sharedMaterial = roadMaterial;

            int textureRepeat = Mathf.RoundToInt(roadPoints.Count * 0.05f);
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