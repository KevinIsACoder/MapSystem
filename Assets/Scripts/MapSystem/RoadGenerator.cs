using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapSystem
{

    public class LineData
    {
            
    }
    
    [Serializable]
    public class RoadGenerator : MonoBehaviour
    {
        public int curveResolution = 1;
        public Material roadMaterial;
        
        public RoadGenerator()
        {
            
        }

        public void Generate()
        {
            
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
        
        public  List<Vector3> CreateSplinePoints(List<Vector3> controlPoints)
        {
            List<Vector3> points = new List<Vector3>(); //All points of the spline
            points.Clear();
            Vector3[] CurveCoordinates;//采样点的坐标
            Vector3 p0, p1, m0, m1;

            int pointsToMake;
            pointsToMake = (curveResolution) * (controlPoints.Count - 1);//要在曲线上采样这么多个点

            if (pointsToMake > 0) //Prevent Number Overflow
            {
                CurveCoordinates = new Vector3[pointsToMake];
                //根据输入的控制点，计算每段曲线需要的4个控制点
                for (int i = 0; i < controlPoints.Count - 1; i++)
                {
                    p0 = controlPoints[i];
                    p1 = controlPoints[i + 1];
                    // m0
                    if (i == 0)m0 = p1 - p0;
                    else m0 = 0.5f * (p1 - controlPoints[i - 1]);
                    // m1
                    if (i < controlPoints.Count - 2)m1 = 0.5f * (controlPoints[(i + 2) % controlPoints.Count] - p0); 
                    else m1 = p1 - p0;

                    Vector3 position;
                    float t;
                    var pointStep = 1.0f / curveResolution;//0.1
                    if (i == controlPoints.Count - 2)
                    {
                        pointStep = 1.0f / (curveResolution - 1);
                    }
                    for (int j = 0; j < curveResolution; j++)//遍历10次，确定每个点的位置
                    {
                        t = j * pointStep;//j=0,t=0; j=1,t=0.1;j=2,t=0.2; t从0到0.9
                        position = MapUtils.Interpolate(p0, p1, m0, m1, t);
                        CurveCoordinates[i * curveResolution + j] = position;//0-9 10-19 20-29 30-39
                        points.Add(position);
                    }
                }
            }
            return points;
        }
    }
}