using System;
using System.Collections.Generic;
using UnityEngine;

namespace MapSystem.Runtime
{
    public class MetaInfo
    {
        public bool highway;
        public bool served;
    }

    public class InterSetctInfo
    {
        public float x;
        public float y;
        public float t;
    }

    public class Segment
    {
        public bool isHignWay = true;
        public Vector2 startPoint; //路段开始点
        public Vector2 endPoint; //路段结束点

        public List<Segment> forwardSegment = new List<Segment>(); //下个路段
        public List<Segment> backwardSegment = new List<Segment>(); //上个路段

        public Func<List<Segment>> setupBranchLinks;

        public MetaInfo segmentMetaInfo;

        public float m_time;

        private float m_width;

        public Segment(Vector2 start_Point, Vector2 end_Point, bool isHignWay = false)
        {
            m_width = isHignWay ? MapConsts.roadWidth : MapConsts.roadWidth;
            startPoint = start_Point;
            endPoint = end_Point;
            this.isHignWay = isHignWay;
        }

        public float Length => Vector2.Distance(startPoint, endPoint);

        public Bound Limits => new Bound((int)Math.Min(startPoint.x, endPoint.x),
            (int)Math.Min(startPoint.y, endPoint.y), (int)Math.Abs(endPoint.x - startPoint.x),
            (int)Math.Abs(endPoint.y - startPoint.y));

        public bool StartIsBackwards()
        {
            if (backwardSegment.Count > 0)
            {
                if (backwardSegment == null) Debug.LogError(("impossib"));
                var ba =
                    MathUtil.EqualV(this.backwardSegment[0].startPoint, this.startPoint) ||
                    MathUtil.EqualV(this.backwardSegment[0].endPoint, this.startPoint);
                if (!ba) Debug.LogError("warning: backward");
                return ba;
            }
            else
            {
                // just in case we have no backwards links (we are start segment)
                return (
                    MathUtil.EqualV(this.forwardSegment[0].startPoint, this.endPoint) ||
                    MathUtil.EqualV(forwardSegment[0].endPoint, endPoint)
                );
            }
        }

        public List<Segment> LinksForEndContaining(Segment segment)
        {
            if (backwardSegment.IndexOf(segment) != -1)
            {
                return backwardSegment;
            }
            else if (forwardSegment.IndexOf(segment) != -1)
            {
                return forwardSegment;
            }
            else
            {
                return null;
            }
        }

        /**
   * split this segment into two segments, connecting the given segment to the newly created crossing
   *
   * left example in https://phiresky.github.io/procedural-cities/img/20151213214559.png
   *
   * @param point the coordinates the split will be at
   * @param thirdSegment the third segment that will be joined to the newly created crossing
   * @param segmentList the full list of all segments (new segment will be added here)
   * @param qTree quadtree for faster finding of segments (new segment will be added here)
   */
        public void Split(Vector2 point, Segment thirdSegment, List<Segment> segmentList, QuadTree<Segment> quadTree)
        {
            var splitSegment = Clone();
            var startIsBackwards = StartIsBackwards();
            segmentList.Add(splitSegment);
            quadTree.Insert(splitSegment.Limits, splitSegment);
            splitSegment.endPoint = point;
            startPoint = point;
            // links are not copied in the constructor, so
            // copy link array for the split part, keeping references the same
            splitSegment.backwardSegment = backwardSegment;
            splitSegment.forwardSegment = forwardSegment;

            Segment firstSplit;
            Segment[] fixLinks;
            Segment secondSplit;

            // determine which links correspond to which end of the split segment
            if (startIsBackwards)
            {
                firstSplit = splitSegment;
                secondSplit = this;
                fixLinks = splitSegment.backwardSegment.ToArray();
            }
            else
            {
                firstSplit = this;
                secondSplit = splitSegment;
                fixLinks = splitSegment.forwardSegment.ToArray();
            }

            // one of the ends of our segment is now instead part of the newly created segment
            // go through all linked roads at that end, and replace their inverse references from referring to this to referring to the newly created segment
            for (int i = 0; i < fixLinks.Length; i++)
            {
                var index = fixLinks[i].backwardSegment.IndexOf(this);
                if (index > 0)
                {
                    fixLinks[i].backwardSegment[index] = splitSegment;
                }
                else
                {
                    index = fixLinks[i].forwardSegment.IndexOf(this);
                    if (index > 0)
                    {
                        fixLinks[index] = splitSegment;
                    }
                }
            }

            // new crossing is between firstSplit, secondSplit, and thirdSegment
            firstSplit.forwardSegment.Clear();
            firstSplit.forwardSegment.Add(thirdSegment);
            firstSplit.forwardSegment.Add(secondSplit);

            secondSplit.backwardSegment.Clear();
            secondSplit.backwardSegment.Add(thirdSegment);
            secondSplit.backwardSegment.Add(firstSplit);
            thirdSegment.forwardSegment.Add(firstSplit);
            thirdSegment.forwardSegment.Add(secondSplit);
        }

        // public static Segment GenerateSegment(Vector2 startPoint, Vector2 direction, float length, float time, MetaInfo metaInfo = null)
        // {
        //     var endPoint = startPoint + direction * length;
        //     return new Segment(startPoint, endPoint, time);
        // }

        public static Segment GenerateSegment(Vector2 startPoint, Vector2 endPoint,
            MetaInfo metaInfo = null)
        {
            return new Segment(startPoint, endPoint);
        }

        public Vector2 Dir()
        {
            return (endPoint - startPoint).normalized;
        }

        public InterSetctInfo InterSectWith(Segment segment)
        {
            var vec1 = MathUtil.SubtractPoints(endPoint, startPoint);
            var vec2 = MathUtil.SubtractPoints(segment.endPoint, segment.startPoint);

            var f = MathUtil.CrossProduct(MathUtil.SubtractPoints(segment.startPoint, startPoint), vec1);
            var k = MathUtil.CrossProduct(vec1, vec2);
            if ((f == 0 && k == 0) || k == 0)
                return null;
            f /= k;
            var e = MathUtil.CrossProduct(MathUtil.SubtractPoints(segment.startPoint, startPoint), vec2) / k;
            var intersetct = 0.001 < e && 0.999 > e && 0.001 < f && 0.999 > f;
            return intersetct
                ? new InterSetctInfo()
                {
                    x = startPoint.x + e * endPoint.x,
                    y = startPoint.y + e * endPoint.y
                }
                : null;
        }

        /// <summary>
        /// 判断endPoint是不是在线段上
        /// <returns>是否相交 true:相交 false:未相交</returns>
        public bool TryGetIntersectPoint(Segment segment, out Vector3 intersectPos)
        {
            intersectPos = Vector3.zero;
            
            if (IsHorizontal())
            {
                if (Math.Abs(segment.startPoint.y - startPoint.y) < 0.0001f)
                {
                    if (Math.Abs(startPoint.x - segment.endPoint.x) < 0.0001f &&
                        Math.Abs(endPoint.x - segment.startPoint.x) < 0.0001f)
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (Math.Abs(segment.startPoint.x - startPoint.x) < 0.0001f)
                {
                    if(Math.Abs(startPoint.y - segment.endPoint.y) < 0.0001f &&
                        Math.Abs(endPoint.y - segment.startPoint.y) < 0.0001f)
                    {
                        return true;
                    }
                }
            }
            
            Vector2 v1 = endPoint - segment.startPoint;
            Vector2 v2 = segment.endPoint - segment.startPoint;
            float angle =Mathf.Abs(Vector3.Angle(v1.normalized,v2.normalized));
            float length_v1 = Vector3.SqrMagnitude(v1);
            float length_v2 = Vector3.SqrMagnitude(v2);
            return angle <= 0.0001f && length_v1 < length_v2;
        }

        public bool IsEqual(Segment segment)
        {
            return Math.Abs(startPoint.x - segment.startPoint.x) <= 0.0001f 
                   && Math.Abs(startPoint.x - segment.startPoint.x) <= 0.0001f
                   && Math.Abs(startPoint.y - segment.startPoint.y) <= 0.0001f 
                   && Math.Abs(startPoint.y - segment.startPoint.y) <= 0.0001f;
        }
        
        public Segment Clone()
        {
            var segment = new Segment(startPoint, endPoint, isHignWay);
            
            return new Segment(startPoint, endPoint, isHignWay);
        }

        public bool IsHorizontal()
        {
            return Math.Abs(endPoint.x - startPoint.x) > 0;
        }
    }
}