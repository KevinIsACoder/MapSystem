using System;
using System.Collections.Generic;
using MapSystem.Runtime;
using UnityEngine;

namespace MapSystem.Test
{
    public class RoadTest : MonoBehaviour
    {
        public List<Vector3> Points;
        [NonSerialized]
        public List<Vector3> curePoints;

        private RoadGenerator _roadGenerator;
        private void OnEnable()
        {
            _roadGenerator = gameObject.GetComponent<RoadGenerator>();
        }
        
        private void Start()
        {
            curePoints = _roadGenerator.CreateSplinePoints(Points);
        }

        private void OnDrawGizmos()
        {
            if(curePoints == null)
                return;
            for (int i = 0; i < curePoints.Count; i++)
            {
                Gizmos.DrawSphere(curePoints[i], 100);   
            }
        }
    }
}