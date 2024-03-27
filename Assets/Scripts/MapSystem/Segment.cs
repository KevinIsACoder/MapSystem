using System;
using UnityEngine;
using System.Collections.Generic;
namespace MapSystem
{
    public class Segment
    {
        public bool isHignWay;
        public Vector3 startPoint; //路段开始点
        public Vector3 endPoint; //路段结束点

        public Segment forwardSegment; //下个路段
        public Segment backwardSegment; //上个路段
        
        
    }
}