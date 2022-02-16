using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MapRenderer;
using System.Xml;
using System;
using System.Xml.Serialization;
using System.Text;
using Schemas;
using System.Linq;

namespace assets.OpenDriveReader
{
    public class OpenDriveManager : MonoBehaviour
    {
        double LaneSectionSkipLength = 0.5; // skip importing such lane sections

        GameObject TrafficLanes;
        GameObject Intersections;
        GameObject SingleLaneRoads;
        Dictionary<string, MapLine> Id2MapLine = new Dictionary<string, MapLine>();
        Dictionary<string, string> RoadId2IntersectionId = new Dictionary<string, string>(); 
        Dictionary<string, List<int>> RoadId2laneSections = new Dictionary<string, List<int>>();
        Dictionary<string, float> RoadId2Speed = new Dictionary<string, float>();
        Dictionary<string, List<Dictionary<int, MapTrafficLane>>> Roads = new Dictionary<string, List<Dictionary<int, MapTrafficLane>>>(); // roadId: laneSectionId: laneId
        public MapManager mapManager;
        public static OpenDriveManager Instance;
        public Map map;
        private int pointIndex = 0;
        public int PointIndex
        {
            get
            {
                return pointIndex++;
            }
        }
        private int lineIndex = 0;
        public int LineIndex
        {
            get
            {
                return lineIndex++;
            }
        }
        [SerializeField]
        OpenDRIVE openDrive;
        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            mapManager = GetComponent<MapManager>();
            MapManager.Instance.OnGetOpenDrive += ReadOpenDriveMapWithBytes;
        }

        private void ReadOpenDriveMapWithBytes(byte[] value)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(OpenDRIVE));
            using (MemoryStream reader = new MemoryStream(value))
            {
                try
                {
                    openDrive = (OpenDRIVE)serializer.Deserialize(reader);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            if (openDrive != null)
            {
                Debug.Log(openDrive.road.Length);
                BuildMap();
            }
        }

        double GeometrySkipLength = 0.01;
        List<double> ReferenceLinePointSValue = new List<double>();
        List<double> SectionRefPointSValue = new List<double>();

        private void BuildMap()
        {
            map = MapManager.Instance.GetOrCreateMap("Map");
            TrafficLanes = new GameObject("TrafficLanes");
            Intersections = new GameObject("Intersections");
            SingleLaneRoads = new GameObject("SingleLaneRoads");
            TrafficLanes.transform.parent = map.transform;
            Intersections.transform.parent = map.transform;
            SingleLaneRoads.transform.parent = TrafficLanes.transform;
            foreach (OpenDRIVERoad road in openDrive.road)
            {
                var roadLength = road.length;
                var roadId = road.id;
                var link = road.link;
                var elevationProfile = road.elevationProfile;
                var lanes = road.lanes;
                //ImportRoadSpeed(road);
                var laneSections = lanes.laneSection;
                var junctionId = road.junction;
                if (junctionId != "-1")
                {
                    RoadId2IntersectionId[roadId] = junctionId;
                }
                List<Vector3> referenceLinePoints = new List<Vector3>();

                ReferenceLinePointSValue.Clear();
                var sectionsS = laneSections.Select(laneSection => laneSection.s).ToList();
                var sectionsPointsIdx = new List<int>();
                var sectionIdx = 0;
                for (int i = 0; i < road.planView.Length; i++)
                {
                    var geometry = road.planView[i];
                    if (geometry.length < GeometrySkipLength)
                    {
                        continue; // skip if it is too short
                    }
                    var affectedIdxStart = sectionsPointsIdx.Count;
                    var dists = GetDists(ref sectionsS, ref sectionIdx, geometry, sectionsPointsIdx, referenceLinePoints);
                    var affectedIdxEnd = sectionsPointsIdx.Count;

                    if (geometry.Items[0] is OpenDRIVERoadGeometryLine) // Line
                    {
                        List<Tuple<Vector3, double>> pointsAndS = CalculateLinePoints(geometry, elevationProfile, dists);
                        UpdateReferenceLinePoint2s(referenceLinePoints.Count, pointsAndS, ReferenceLinePointSValue);
                        referenceLinePoints.AddRange(pointsAndS.Select(entry => entry.Item1).ToList());
                    }
                    else if (geometry.Items[0] is OpenDRIVERoadGeometrySpiral) // Spiral
                    {
                        OpenDRIVERoadGeometrySpiral spi = geometry.Items[0] as OpenDRIVERoadGeometrySpiral;
                        if (spi.curvStart == 0)
                        {
                            Vector3 geo = new Vector3((float)geometry.x, 0f, (float)geometry.y);
                            List<Tuple<Vector3, double>> pointsAndS = CalculateSpiralPoints(geometry,
                                elevationProfile, geo, geometry.hdg, dists);
                            UpdateReferenceLinePoint2s(referenceLinePoints.Count, pointsAndS, ReferenceLinePointSValue);
                            referenceLinePoints.AddRange(pointsAndS.Select(entry => entry.Item1).ToList());
                        }
                        else
                        {
                            // Update index if points get reversed
                            UpdateSectionsPointsIdx(sectionsPointsIdx, affectedIdxStart,
                                affectedIdxEnd, referenceLinePoints.Count, dists.Count);
                            List<Tuple<Vector3, double>> pointsAndS;
                            if (i != road.planView.Length - 1)
                            {
                                var geometryNext = road.planView[i + 1];
                                Vector3 geo = new Vector3((float)geometryNext.x, 0f, (float)geometryNext.y);
                                pointsAndS = CalculateSpiralPoints(geometry, elevationProfile, geo, geometryNext.hdg, dists);
                            }
                            else
                            {
                                var tmp = GetRoadById(openDrive, Int32.Parse(link.successor.elementId));
                                var geometryNext = tmp.planView[tmp.planView.Length - 1];
                                Vector3 geo = new Vector3((float)geometryNext.x, 0f, (float)geometryNext.y);
                                pointsAndS = CalculateSpiralPoints(geometry, elevationProfile, geo, -geometryNext.hdg, dists);
                            }
                            UpdateReferenceLinePoint2s(referenceLinePoints.Count, pointsAndS, ReferenceLinePointSValue); // sequence of s doesn't need to be reversed
                            pointsAndS.Reverse();
                            referenceLinePoints.AddRange(pointsAndS.Select(entry => entry.Item1).ToList());
                        }
                    }
                    else if (geometry.Items[0] is OpenDRIVERoadGeometryArc) // Arc
                    {
                        List<Tuple<Vector3, double>> pointsAndS = CalculateArcPoints(geometry, elevationProfile, dists);
                        UpdateReferenceLinePoint2s(referenceLinePoints.Count, pointsAndS, ReferenceLinePointSValue);
                        referenceLinePoints.AddRange(pointsAndS.Select(entry => entry.Item1).ToList());
                    }
                    else if (geometry.Items[0] is OpenDRIVERoadGeometryPoly3) // Poly3
                    {
                        List<Tuple<Vector3, double>> pointsAndS = CalculatePoly3Points(geometry, elevationProfile, dists);
                        UpdateReferenceLinePoint2s(referenceLinePoints.Count, pointsAndS, ReferenceLinePointSValue);
                        referenceLinePoints.AddRange(pointsAndS.Select(entry => entry.Item1).ToList());
                    }
                    else if (geometry.Items[0] is OpenDRIVERoadGeometryParamPoly3) // ParamPoly3
                    {
                        List<Tuple<Vector3, double>> pointsAndS = CalculateParamPoly3Points(geometry, elevationProfile, dists);
                        UpdateReferenceLinePoint2s(referenceLinePoints.Count, pointsAndS, ReferenceLinePointSValue);
                        referenceLinePoints.AddRange(pointsAndS.Select(entry => entry.Item1).ToList());
                    }
                }
                if (referenceLinePoints.Count == 0)
                {
                    var geometry = road.planView[0];
                    Vector3 origin = new Vector3((float)geometry.x, 0f, (float)geometry.y);
                    referenceLinePoints.Add(origin);
                    ReferenceLinePointSValue.Add(geometry.s);

                    double y = GetElevation(geometry.s + roadLength, elevationProfile);
                    Vector3 pos = new Vector3((float)roadLength, (float)y, 0f);
                    // rotate
                    pos = Quaternion.Euler(0f, -(float)(geometry.hdg * 180f / Math.PI), 0f) * pos;
                    referenceLinePoints.Add(origin + pos);
                    ReferenceLinePointSValue.Add(geometry.s + roadLength);
                }
                // if the length of the reference line is less than 1 meter
                else if (referenceLinePoints.Count == 1)
                {
                    var geometry = road.planView[0];
                    Vector3 origin = new Vector3((float)geometry.x, 0f, (float)geometry.y);

                    double y = GetElevation(geometry.s + geometry.length, elevationProfile);
                    Vector3 pos = new Vector3((float)geometry.length, (float)y, 0f);
                    // rotate
                    pos = Quaternion.Euler(0f, -(float)(geometry.hdg * 180f / Math.PI), 0f) * pos;
                    referenceLinePoints.Add(origin + pos);
                    ReferenceLinePointSValue.Add(geometry.s + geometry.length);
                }
                Roads[roadId] = new List<Dictionary<int, MapTrafficLane>>();

                var referenceLinePointsOffset = new List<Vector3>(referenceLinePoints);

                if (lanes.laneOffset != null)
                {
                    referenceLinePointsOffset = UpdateReferencePoints(lanes,
                       referenceLinePoints, ReferenceLinePointSValue);
                }
                var laneSectionsLength = ComputeSectionLength(laneSections, roadLength);
                for (int i = 0; i < laneSections.Length; i++)
                {
                    Roads[roadId].Add(new Dictionary<int, MapTrafficLane>());

                    if (laneSectionsLength[i] < LaneSectionSkipLength)
                    {
                        Debug.LogWarning($"Road {roadId} laneSection {i} is too short, skipped importing");
                        if (RoadId2laneSections.ContainsKey(roadId))
                        {
                            RoadId2laneSections[roadId].Add(i);
                        }
                        else
                        {
                            RoadId2laneSections[roadId] = new List<int> { i };
                        }
                        continue;
                    }

                    var startIdx = sectionsPointsIdx[i];
                    int endIdx;
                    if (i == laneSections.Length - 1)
                    {
                        endIdx = referenceLinePointsOffset.Count;
                    }
                    else
                    {
                        endIdx = sectionsPointsIdx[i + 1];
                    }

                    if (endIdx == startIdx)
                    {
                        continue;
                    }

                    var sectionRefPointsOffset = new List<Vector3>(referenceLinePointsOffset.GetRange(startIdx, endIdx - startIdx));
                    var sectionRefPoints = new List<Vector3>(referenceLinePoints.GetRange(startIdx, endIdx - startIdx));
                    SectionRefPointSValue = new List<double>(ReferenceLinePointSValue.GetRange(startIdx, endIdx - startIdx));
                    for (int j = endIdx - startIdx - 1; j >= 0; --j)
                    {
                        SectionRefPointSValue[j] -= SectionRefPointSValue[0];
                    }
                    if (i == 10)
                    {
                        Debug.Log(sectionRefPointsOffset.Count);
                    }
                    Debug.Assert(referenceLinePoints.Count > 0,
                        $"referenceLinePointsOffset has no elements, roadId {roadId} laneSectionId {i} s = {laneSections[i].s}");
                    CreateMapLanes(roadId, i, laneSections[i], sectionRefPointsOffset, sectionRefPoints);
                }
            }


            map.UpdateRenderer();
        }
        void CreateMapLanes(string roadId, int laneSectionId,
           OpenDRIVERoadLanesLaneSection laneSection, List<Vector3> sectionRefPointsOffset,
           List<Vector3> sectionRefPoints)
        {
            var roadIdLaneSectionId = $"road{roadId}_section{laneSectionId}";
            MapLine refMapLine = GetRefMapLine(roadIdLaneSectionId, laneSection, sectionRefPointsOffset);

            CreateLines(refMapLine);
            // From left to right, compute other MapLines
            // Get number of lanes and move lane into a new MapLaneSection or SingleLanes
            GameObject parentObj = GetParentObj(roadIdLaneSectionId, laneSection);
            if (laneSection.left != null && laneSection.left.lane != null) CreateLinesLanes(roadId, laneSectionId, sectionRefPoints, refMapLine, laneSection.left.lane, parentObj, true);
            if (laneSection.right != null && laneSection.right.lane != null) CreateLinesLanes(roadId, laneSectionId, sectionRefPoints, refMapLine, laneSection.right.lane, parentObj, false);

           

            refMapLine.transform.parent = parentObj.transform;

             
        }
        private GameObject GetParentObj(string roadIdLaneSectionId, OpenDRIVERoadLanesLaneSection laneSection)
        {
            var leftLanes = laneSection.left?.lane;
            var rightLanes = laneSection.right?.lane;
            var numLanes = (leftLanes?.Length ?? 0) + (rightLanes?.Length ?? 0);

            GameObject parentObj;
            if (numLanes > 1)
            {
                parentObj = new GameObject($"MapLaneSection_{roadIdLaneSectionId}");
                parentObj.transform.parent = TrafficLanes.transform;
                parentObj.AddComponent<MapLaneSection>();
            }
            else
            {
                parentObj = SingleLaneRoads;
            }

            return parentObj;
        }
        void CreateLinesLanes(string roadId, int laneSectionId, List<Vector3> sectionRefPoints, MapLine refMapLine, lane[] lanes, GameObject parentObj, bool isLeft)
        {
            var centerLinePoints = new List<Vector3>(refMapLine.mapWorldPositions);
            List<Vector3> curLeftBoundaryPoints = centerLinePoints;

            IEnumerable<lane> it;
            it = isLeft ? lanes.Reverse() : lanes;

            foreach (var curLane in it)
            {
                var id = curLane.id.ToString();
                List<Vector3> curRightBoundaryPoints = CalculateRightBoundaryPoints(sectionRefPoints, curLeftBoundaryPoints, curLane, isLeft);

                var mapLineId = $"MapLine_road{roadId}_section{laneSectionId}_{id}";
                var mapLineObj = new GameObject(mapLineId);
                var mapLine = mapLineObj.AddComponent<MapLine>();
                Id2MapLine[mapLineId] = mapLine;

                var lanePoints = GetLanePoints(curLeftBoundaryPoints, curRightBoundaryPoints);
                if (lanePoints.Count == 0)
                {
                    Debug.LogError($"Not able to get correct lane points, skip importing lane {id} (roadId: {roadId})");
                }
                if (isLeft)
                {
                    lanePoints.Reverse();
                }

                //CreateLane(roadId, laneSectionId, curLane, lanePoints, parentObj);

                curLeftBoundaryPoints = new List<Vector3>(curRightBoundaryPoints);

                if (isLeft)
                {
                    curRightBoundaryPoints.Reverse();
                }
                mapLine.mapWorldPositions = curRightBoundaryPoints;
                mapLine.transform.position = GetAverage(curRightBoundaryPoints);

                mapLine.transform.parent = parentObj.transform;
                if (curLane.roadMark != null)
                {
                    // Note: centerLane might have more than one road mark, currently we only use the first one
                    var centerLaneRoadMark = curLane.roadMark[0];
                    switch (centerLaneRoadMark.color)
                    {
                        case color.standard:
                            mapLine.color = Color.white;
                            break;
                        case color.blue:
                            mapLine.color = Color.blue;
                            break;
                        case color.green:
                            mapLine.color = Color.green;
                            break;
                        case color.red:
                            mapLine.color = Color.red;
                            break;
                        case color.white:
                            mapLine.color = Color.white;
                            break;
                        case color.yellow:
                            mapLine.color = Color.yellow;
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    mapLine.color = Color.white;
                }
                CreateLines(mapLine);
            }
        }
        private List<Vector3> CalculateRightBoundaryPoints(List<Vector3> sectionRefPoints, List<Vector3> curLeftBoundaryPoints, lane curLane, bool isLeft)
        {
            var curRightBoundaryPoints = new List<Vector3>();
            int curWidthIdx = 0;
            Debug.Assert(curLane.Items.Length > 0);

            var width = curLane.Items[0] as laneWidth;
            var sOffset = width.sOffset;
            for (int idx = 0; idx < curLeftBoundaryPoints.Count; idx++)
            {
                Vector3 point = curLeftBoundaryPoints[idx];
                var s = SectionRefPointSValue[idx];
                if (s >= sOffset)
                {
                    while (curWidthIdx < curLane.Items.Length - 1 && s >= ((laneWidth)curLane.Items[curWidthIdx + 1]).sOffset)
                    {
                        width = curLane.Items[++curWidthIdx] as laneWidth;
                        sOffset = width.sOffset;
                    }
                }

                var ds = s - sOffset;
                var widthValue = width.a + width.b * ds + width.c * ds * ds + width.d * ds * ds * ds;
                Vector3 normalDir;
                normalDir = GetNormalDir(sectionRefPoints, idx, isLeft);

                var newPoint = point + (float)widthValue * normalDir;
                if (idx > 0)
                {
                    var p1 = curLeftBoundaryPoints[idx - 1];
                    var p2 = curRightBoundaryPoints[idx - 1];
                    var p3 = curLeftBoundaryPoints[idx];
                    var isIntersect = LineSegementsIntersect(ToVector2(p1), ToVector2(p2), ToVector2(p3), ToVector2(newPoint), out var intersect);
                    if (isIntersect) newPoint = p2;
                }
                curRightBoundaryPoints.Add(newPoint);
            }

            return curRightBoundaryPoints;
        }
        Vector2 ToVector2(Vector3 pt)
        {
            return new Vector2(pt.x, pt.z);
        }
        public bool LineSegementsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection, bool considerCollinearOverlapAsIntersect = false)
        {
            intersection = new Vector2();

            var r = a2 - a1;
            var s = b2 - b1;
            var rxs = Cross(r, s);
            var qpxr = Cross(b1 - a1, r);

            // If r x s = 0 and (b1 - a1) x r = 0, then the two lines are collinear.
            if (Math.Abs(rxs) < 0.0001f && Math.Abs(qpxr) < 0.0001f)
            {
                // 1. If either  0 <= (b1 - a1) * r <= r * r or 0 <= (a1 - b1) * s <= * s
                // then the two lines are overlapping,
                if (considerCollinearOverlapAsIntersect)
                {
                    if ((0 <= Vector2.Dot(b1 - a1, r) && Vector2.Dot(b1 - a1, r) <= Vector2.Dot(r, r)) || (0 <= Vector2.Dot(a1 - b1, s) && Vector2.Dot(a1 - b1, s) <= Vector2.Dot(s, s)))
                    {
                        return true;
                    }
                }

                // 2. If neither 0 <= (b1 - a1) * r = r * r nor 0 <= (a1 - b1) * s <= s * s
                // then the two lines are collinear but disjoint.
                // No need to implement this expression, as it follows from the expression above.
                return false;
            }

            // 3. If r x s = 0 and (b1 - a1) x r != 0, then the two lines are parallel and non-intersecting.
            if (Math.Abs(rxs) < 0.0001f && !(Math.Abs(qpxr) < 0.0001f))
                return false;

            // t = (b1 - a1) x s / (r x s)
            var t = Cross(b1 - a1, s) / rxs;

            // u = (b1 - a1) x r / (r x s)

            var u = Cross(b1 - a1, r) / rxs;

            // 4. If r x s != 0 and 0 <= t <= 1 and 0 <= u <= 1
            // the two line segments meet at the point a1 + t r = b1 + u s.
            if (!(Math.Abs(rxs) < 0.0001f) && (0 <= t && t <= 1) && (0 <= u && u <= 1))
            {
                // We can calculate the intersection point using either t or u.
                intersection = a1 + t * r;

                // An intersection was found.
                return true;
            }

            // 5. Otherwise, the two line segments are not parallel but do not intersect.
            return false;
        }
        public float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.x * v2.y - v1.y * v2.x;
        }
        List<Vector3> GetLanePoints(List<Vector3> leftBoundaryPoints, List<Vector3> rightBoundaryPoints)
        {
            float resolution = 5; // 5 meters

            // Get the length of longer boundary line
            float leftLength = RangedLength(leftBoundaryPoints);
            float rightLength = RangedLength(rightBoundaryPoints);
            float longerDistance = (leftLength > rightLength) ? leftLength : rightLength;
            int partitions = (int)Math.Ceiling(longerDistance / resolution);
            if (partitions < 2)
            {
                // For boundary line whose length is less than resolution
                partitions = 2; // Make sure every line has at least 2 partitions.
            }

            float leftResolution = leftLength / partitions;
            float rightResolution = rightLength / partitions;

            leftBoundaryPoints = SplitLine(leftBoundaryPoints, leftResolution, partitions);
            rightBoundaryPoints = SplitLine(rightBoundaryPoints, rightResolution, partitions);

            if (leftBoundaryPoints.Count != rightBoundaryPoints.Count ||
                (longerDistance > 1 && leftBoundaryPoints.Count != partitions + 1)) // neglect too short lines
            {
                Debug.LogError("Something wrong with number of points. (left, right, partitions): (" +
                    leftBoundaryPoints.Count + ", " + rightBoundaryPoints.Count + ", " + partitions + ")");
                return new List<Vector3>();
            }

            List<Vector3> lanePoints = new List<Vector3>();
            for (int i = 0; i < leftBoundaryPoints.Count; i++)
            {
                Vector3 centerPoint = (leftBoundaryPoints[i] + rightBoundaryPoints[i]) / 2;
                lanePoints.Add(centerPoint);
            }

            return lanePoints;
        }
        public float RangedLength(List<Vector3> points)
        {
            float len = 0;

            for (int i = 0; i < points.Count - 1; i++)
            {
                len += Vector3.Distance(points[i], points[i + 1]);
            }

            return len;
        }
        public static List<Vector3> SplitLine(List<Vector3> line, float resolution, int partitions, bool reverse = false)
        {
            List<Vector3> splittedLinePoints = new List<Vector3>();
            splittedLinePoints.Add(line[0]); // Add first point

            float residue = 0; // Residual length from previous segment

            // loop through each segment in boundry line
            for (int i = 1; i < line.Count; i++)
            {
                if (splittedLinePoints.Count >= partitions) break;

                Vector3 lastPoint = line[i - 1];
                Vector3 curPoint = line[i];

                // Continue if no points are made within current segment
                float segmentLength = Vector3.Distance(lastPoint, curPoint);
                if (segmentLength + residue < resolution)
                {
                    residue += segmentLength;
                    continue;
                }

                Vector3 direction = (curPoint - lastPoint).normalized;
                for (float length = resolution - residue; length <= segmentLength; length += resolution)
                {
                    Vector3 partitionPoint = lastPoint + direction * length;
                    splittedLinePoints.Add(partitionPoint);
                    if (splittedLinePoints.Count >= partitions) break;
                    residue = segmentLength - length;
                }

                if (splittedLinePoints.Count >= partitions) break;
            }

            splittedLinePoints.Add(line[line.Count - 1]);

            if (reverse)
            {
                splittedLinePoints.Reverse();
            }

            return splittedLinePoints;
        }
        private MapLine GetRefMapLine(string roadIdLaneSectionId, OpenDRIVERoadLanesLaneSection laneSection, List<Vector3> sectionRefPointsOffset)
        {

            var refMapLineId = $"MapLine_{roadIdLaneSectionId}_0";
            var refMapLineObj = new GameObject(refMapLineId);
            var refMapLine = refMapLineObj.AddComponent<MapLine>();
            Id2MapLine[refMapLineId] = refMapLine;

            var centerLane = laneSection.center.lane;
            refMapLine.mapWorldPositions = sectionRefPointsOffset;
            refMapLine.transform.position = GetAverage(sectionRefPointsOffset);

            if (centerLane.roadMark != null)
            {
                // Note: centerLane might have more than one road mark, currently we only use the first one
                var centerLaneRoadMark = centerLane.roadMark[0];
                switch (centerLaneRoadMark.color)
                {
                    case color.standard:
                        refMapLine.color = Color.white;
                        break;
                    case color.blue:
                        refMapLine.color = Color.blue;
                        break;
                    case color.green:
                        refMapLine.color = Color.green;
                        break;
                    case color.red:
                        refMapLine.color = Color.red;
                        break;
                    case color.white:
                        refMapLine.color = Color.white;
                        break;
                    case color.yellow:
                        refMapLine.color = Color.yellow;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                 refMapLine.color = Color.white;
            }
            return refMapLine;
        }
        public void CreateLines(MapLine mapLine)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < mapLine.mapWorldPositions.Count; i++)
            {
                points.Add(map.AddPoint(PointIndex.ToString(), mapLine.mapWorldPositions[i]));
            }
            Line line = map.AddWhiteLine("Line" + LineIndex.ToString());
            line.points = points;
            line.color = mapLine.color;
        }

        List<Vector3> UpdateReferencePoints(OpenDRIVERoadLanes lanes,
            List<Vector3> referencePoints, List<double> referenceLinePointSValue)
        {
            var updatedReferencePoints = new List<Vector3>();
            var laneOffsets = lanes.laneOffset;
            int curLaneOffsetIdx = 0;
            Debug.Assert(laneOffsets.Length > 0);

            var laneOffset = laneOffsets[0];
            var laneOffsetS = laneOffset.s;
            for (int idx = 0; idx < referencePoints.Count; idx++)
            {
                Vector3 point = referencePoints[idx];
                var curS = referenceLinePointSValue[idx];
                if (curS >= laneOffsetS)
                {
                    while (curLaneOffsetIdx < laneOffsets.Length - 1 && curS >= (laneOffsets[curLaneOffsetIdx + 1]).s)
                    {
                        laneOffset = laneOffsets[++curLaneOffsetIdx];
                        laneOffsetS = laneOffset.s;
                    }
                }

                var ds = curS - laneOffsetS;
                var offsetValue = laneOffset.a + laneOffset.b * ds + laneOffset.c * ds * ds + laneOffset.d * ds * ds * ds;
                // Debug.Log($"idx {idx} s: {curS} offset: {offsetValue} curLaneOffsetIdx {curLaneOffsetIdx}");
                var normalDir = GetNormalDir(referencePoints, idx, true);
                updatedReferencePoints.Add(point + (float)offsetValue * normalDir);
            }
            return updatedReferencePoints;
        }

        public static Vector3 GetNormalDir(List<Vector3> points, int index, bool isLeft)
        {
            Vector3 normalDir = Vector3.zero;

            for (int i = index + 1; i < points.Count; i++)
            {
                if (Vector3.Distance(points[index], points[i]) > 0.01)
                {
                    normalDir += Vector3.Cross(Vector3.up, points[i] - points[index]);
                    break;
                }
            }
            for (int i = index - 1; i >= 0; i--)
            {
                if (Vector3.Distance(points[index], points[i]) > 0.01)
                {
                    normalDir += Vector3.Cross(Vector3.up, points[index] - points[i]);
                    break;
                }
            }
            return normalDir.normalized * (isLeft ? -1 : +1);
        }
        void ImportRoadSpeed(OpenDRIVERoad road)
        {
            if (road.type != null && road.type[0].speed != null)
            {
                var speed = road.type[0].speed; // only use 1st type's speed if road has multiple types
                if (speed.max == "no limit" || speed.max == "undefined")
                {
                    return;
                }

                if (!float.TryParse(speed.max, out float speedLimit) || speedLimit < 0)
                {
                    return;
                }

                var speedUnit = speed.unit;
                if (!speed.unitSpecified || !ConvertSpeed(speedUnit, ref speedLimit))
                {
                    return;
                }
                RoadId2Speed[road.id] = speedLimit;
            }
        }
        bool ConvertSpeed(unit speedUnit, ref float speedLimit)
        {
            if (speedUnit == unit.ms)
            {
                return true;
            }
            if (speedUnit == unit.kmh)
            {
                speedLimit *= 0.277778f;
                return true;
            }
            if (speedUnit == unit.mph)
            {
                speedLimit *= 0.44704f;
                return true;
            }

            return false;
        }
        List<double> GetDists(ref List<double> sectionsS, ref int sectionIdx,
            OpenDRIVERoadGeometry geometry, List<int> sectionPointsIdx, List<Vector3> refPoints)
        {
            // Add two closest dists for nonzero laneSections, s - 0.1 and s to get correct laneOffsets
            // previous lane section points until the point of s - 0.1, next lane section start the point of s
            if (sectionIdx < sectionsS.Count && sectionsS[sectionIdx] < 0)
            {
                sectionPointsIdx.Add(refPoints.Count);
                sectionIdx++;
            }

            var dists = new List<double>(); // distances from geometry.s
            var resolution = 1;
            for (var i = 0; i < geometry.length - 0.1; i += resolution)
            {
                dists.Add(i);
            }
            if (geometry.length - 0.1 >= 0)
            {
                dists.Add(geometry.length - 0.1);
            }

            // separate dists at least 1 meter except for the two points around lanesection idx
            if (dists.Count > 1 && (dists.Last() - dists[dists.Count - 2]) < 0.1)
            {
                dists.RemoveAt(dists.Count - 2);
            }

            if (sectionIdx < sectionsS.Count && Math.Abs(geometry.s - sectionsS[sectionIdx]) < 0.001)
            {
                sectionPointsIdx.Add(refPoints.Count);
                sectionIdx++;
            }

            int idx = 1;
            while (sectionIdx < sectionsS.Count && sectionsS[sectionIdx] < geometry.s + geometry.length - 0.1)
            {
                var sectionS = sectionsS[sectionIdx];
                while (idx < dists.Count && geometry.s + dists[idx] < sectionS) idx++;

                double diff = geometry.s + dists[idx] - sectionS;
                if (diff < 0.001)
                {
                    dists.Insert(idx, dists[idx++] - 0.1);
                    sectionPointsIdx.Add(refPoints.Count + idx);
                    sectionIdx++;
                }
                else
                {
                    var sectionDist = sectionS - geometry.s;
                    // Remove closest element in dists
                    if (sectionDist - dists[idx - 1] < dists[idx] - sectionDist)
                    {
                        dists.RemoveAt(idx - 1);
                        idx--;
                    }
                    else
                    {
                        dists.RemoveAt(idx);
                    }
                    dists.Insert(idx++, sectionDist - 0.1);
                    dists.Insert(idx, sectionDist);
                    sectionPointsIdx.Add(refPoints.Count + idx);
                    sectionIdx++;
                }
            }
            return dists;
        }
        List<double> ComputeSectionLength(OpenDRIVERoadLanesLaneSection[] laneSections, double roadLength)
        {
            var sectionLengths = new List<double>();
            for (int i = 0; i < laneSections.Length; i++)
            {
                if (i < laneSections.Length - 1)
                {
                    sectionLengths.Add(laneSections[i + 1].s - laneSections[i].s);
                }
                else
                {
                    sectionLengths.Add(roadLength - laneSections[i].s);
                }
            }

            return sectionLengths;
        }
        public List<Tuple<Vector3, double>> CalculateLinePoints(OpenDRIVERoadGeometry geometry,
           OpenDRIVERoadElevationProfile elevationProfile, List<double> dists)
        {
            OpenDRIVERoadGeometryLine line = geometry.Items[0] as OpenDRIVERoadGeometryLine;

            Vector3 origin = new Vector3((float)geometry.x, 0f, (float)geometry.y);
            List<Tuple<Vector3, double>> pointsAndS = new List<Tuple<Vector3, double>>();

            double hdg = geometry.hdg;
            double s = geometry.s;
            for (int i = 0; i < dists.Count; i++)
            {
                var dist = dists[i];
                var x = dist;
                var curS = s + dist;
                var pos = GetPos(elevationProfile, curS, x, 0, hdg);
                pointsAndS.Add(new Tuple<Vector3, double>(origin + pos, curS));
            }

            return pointsAndS;
        }

        Vector3 GetPos(OpenDRIVERoadElevationProfile elevationProfile,
            double s, double x, double z, double hdg)
        {
            double y = GetElevation(s, elevationProfile);
            Vector3 pos = new Vector3((float)x, (float)y, (float)z);
            // rotate
            pos = Quaternion.Euler(0f, -(float)(hdg * 180f / Math.PI), 0f) * pos;

            return pos;
        }

        // Calculate elevation at the specific length along the reference path
        public double GetElevation(double l, OpenDRIVERoadElevationProfile elevationProfile)
        {
            if (l < 0)
            {
                l = 0;
            }
            double elevation = 0;
            if (elevationProfile == null || elevationProfile.elevation == null)
            {
                return 0;
            }
            else if (elevationProfile.elevation.Length == 1)
            {
                double a = elevationProfile.elevation[0].a;
                double b = elevationProfile.elevation[0].b;
                double c = elevationProfile.elevation[0].c;
                double d = elevationProfile.elevation[0].d;
                double ds = l;
                elevation = a + b * ds + c * ds * ds + d * ds * ds * ds;
                return elevation;
            }
            else
            {
                // decide which elevation profile to be used
                for (int i = 0; i < elevationProfile.elevation.Length; i++)
                {
                    if (i != elevationProfile.elevation.Length - 1)
                    {
                        double s = elevationProfile.elevation[i].s;
                        double sNext = elevationProfile.elevation[i + 1].s;
                        if (l >= s && l < sNext)
                        {
                            double a = elevationProfile.elevation[i].a;
                            double b = elevationProfile.elevation[i].b;
                            double c = elevationProfile.elevation[i].c;
                            double d = elevationProfile.elevation[i].d;
                            double ds = l - s;
                            elevation = a + b * ds + c * ds * ds + d * ds * ds * ds;

                            return elevation;
                        }
                    }
                    else
                    {
                        double s = elevationProfile.elevation[i].s;
                        double a = elevationProfile.elevation[i].a;
                        double b = elevationProfile.elevation[i].b;
                        double c = elevationProfile.elevation[i].c;
                        double d = elevationProfile.elevation[i].d;
                        double ds = l - s;
                        elevation = a + b * ds + c * ds * ds + d * ds * ds * ds;

                        return elevation;
                    }
                }
            }

            return 0;
        }

        void UpdateReferenceLinePoint2s(int refPointsCount, List<Tuple<Vector3, double>> pointsAndS,
            List<double> referenceLinePointSValue)
        {
            for (int i = 0; i < pointsAndS.Count; i++)
            {
                var s = pointsAndS[i].Item2;
                var index = refPointsCount + i;
                referenceLinePointSValue.Add(s);
            }
        }
        public List<Tuple<Vector3, double>> CalculateSpiralPoints(OpenDRIVERoadGeometry geometry,
            OpenDRIVERoadElevationProfile elevationProfile, Vector3 origin, double hdg,
            List<double> dists)
        {
            OpenDRIVERoadGeometrySpiral spi = geometry.Items[0] as OpenDRIVERoadGeometrySpiral;

            List<Tuple<Vector3, double>> pointsAndS = new List<Tuple<Vector3, double>>();
            OdrSpiral.Spiral spiral = new OdrSpiral.Spiral();

            double x_ = new double();
            double z_ = new double();
            double t_ = new double();

            var curvStart = spi.curvStart;
            var curvEnd = spi.curvEnd;
            var length = geometry.length;
            spiral.odrSpiral(length, (curvEnd - curvStart) / length, ref x_, ref z_, ref t_);

            var s = geometry.s;
            for (int i = 0; i < dists.Count; i++)
            {
                var dist = dists[i];
                double x = new double();
                double z = new double();
                double t = new double();
                spiral.odrSpiral(dist, (curvEnd - curvStart) / length, ref x, ref z, ref t);

                var curS = s + dist;
                double y = GetElevation(curS, elevationProfile);
                Vector3 pos;
                if (curvStart == 0)
                {
                    pos = new Vector3((float)x, (float)y, (float)z);
                }
                else
                {
                    pos = new Vector3(-(float)x, (float)y, -(float)z);
                }
                // rotate
                pos = Quaternion.Euler(0f, -(float)(hdg * 180f / Math.PI), 0f) * pos;
                pointsAndS.Add(new Tuple<Vector3, double>(origin + pos, curS));
            }

            return pointsAndS;
        }

        void UpdateSectionsPointsIdx(List<int> sectionsPointsIdx, int affectedIdxStart,
            int affectedIdxEnd, int refPointsCount, int distsCount)
        {
            for (int idx = affectedIdxStart; idx < affectedIdxEnd; idx++)
            {
                var offset = sectionsPointsIdx[idx] - refPointsCount;
                offset = distsCount - offset;
                sectionsPointsIdx[idx] = refPointsCount + offset;
            }
        }
        public List<Tuple<Vector3, double>> CalculateArcPoints(OpenDRIVERoadGeometry geometry,
            OpenDRIVERoadElevationProfile elevationProfile, List<double> dists)
        {
            OpenDRIVERoadGeometryArc arc = geometry.Items[0] as OpenDRIVERoadGeometryArc;

            Vector3 origin = new Vector3((float)geometry.x, 0f, (float)geometry.y);
            List<Tuple<Vector3, double>> pointsAndS = new List<Tuple<Vector3, double>>();
            OdrSpiral.Spiral a = new OdrSpiral.Spiral();

            double s = geometry.s;
            var hdg = geometry.hdg;
            for (int i = 0; i < dists.Count; i++)
            {
                var dist = dists[i];
                double x = new double();
                double z = new double();
                a.odrArc(dist, arc.curvature, ref x, ref z);

                var curS = s + dist;
                var pos = GetPos(elevationProfile, curS, x, z, hdg);
                pointsAndS.Add(new Tuple<Vector3, double>(origin + pos, curS));
            }

            return pointsAndS;
        }
        public List<Tuple<Vector3, double>> CalculatePoly3Points(OpenDRIVERoadGeometry geometry,
            OpenDRIVERoadElevationProfile elevationProfile, List<double> dists)
        {
            OpenDRIVERoadGeometryPoly3 poly3 = geometry.Items[0] as OpenDRIVERoadGeometryPoly3;

            Vector3 origin = new Vector3((float)geometry.x, 0f, (float)geometry.y);
            var hdg = geometry.hdg;

            List<Tuple<Vector3, double>> pointsAndS = new List<Tuple<Vector3, double>>();
            double a = poly3.a;
            double b = poly3.b;
            double c = poly3.c;
            double d = poly3.d;

            double s = geometry.s;
            for (int i = 0; i < dists.Count; i++)
            {
                var dist = dists[i];
                double x = dist;
                double z = a + b * x + c * x * x + d * x * x * x;

                var curS = s + dist;
                var pos = GetPos(elevationProfile, curS, x, z, hdg);
                pointsAndS.Add(new Tuple<Vector3, double>(origin + pos, curS));
            }

            return pointsAndS;
        }
        public List<Tuple<Vector3, double>> CalculateParamPoly3Points(OpenDRIVERoadGeometry geometry,
           OpenDRIVERoadElevationProfile elevationProfile, List<double> dists)
        {
            OpenDRIVERoadGeometryParamPoly3 pPoly3 = geometry.Items[0] as OpenDRIVERoadGeometryParamPoly3;
            Vector3 origin = new Vector3((float)geometry.x, 0f, (float)geometry.y);

            List<Tuple<Vector3, double>> pointsAndS = new List<Tuple<Vector3, double>>();
            double aU = pPoly3.aU;
            double bU = pPoly3.bU;
            double cU = pPoly3.cU;
            double dU = pPoly3.dU;
            double aV = pPoly3.aV;
            double bV = pPoly3.bV;
            double cV = pPoly3.cV;
            double dV = pPoly3.dV;

            bool useArcLength = false;
            double pMax = 1;
            var pPoly3Length = geometry.length;
            if (pPoly3.pRangeSpecified == true && pPoly3.pRange == pRange.arcLength)
            {
                useArcLength = true;
                pMax = pPoly3Length;
            }

            double s = geometry.s;
            var hdg = geometry.hdg;
            for (int i = 0; i < dists.Count; i++)
            {
                var dist = dists[i];
                var p = useArcLength ? dist : dist / pPoly3Length;

                double x = aU + bU * p + cU * p * p + dU * p * p * p;
                double z = aV + bV * p + cV * p * p + dV * p * p * p;

                var curS = s + dist;
                var pos = GetPos(elevationProfile, curS, x, z, hdg);
                pointsAndS.Add(new Tuple<Vector3, double>(origin + pos, curS));
            }

            return pointsAndS;
        }
        OpenDRIVERoad GetRoadById(OpenDRIVE map, int id)
        {
            foreach (var road in map.road)
            {
                if (Int32.Parse(road.id) == id)
                {
                    return road;
                }
            }
            return null;
        }
        Vector3 GetAverage(List<Vector3> vectors)
        {
            if (vectors.Count == 0)
            {
                Debug.LogError("Given points has no elements. Returning (0, 0, 0) instead.");
                return new Vector3(0, 0, 0);
            }

            float x = 0f, y = 0f, z = 0f;
            foreach (var vector in vectors)
            {
                x += vector.x;
                y += vector.y;
                z += vector.z;
            }
            return new Vector3(x / vectors.Count, y / vectors.Count, z / vectors.Count);
        }


    }
    public class MapLaneSection : MonoBehaviour
    {

        [System.NonSerialized]
        public List<MapTrafficLane> lanes = new List<MapTrafficLane>();
        [System.NonSerialized]
        public List<MapTrafficLane> lanesForward = new List<MapTrafficLane>();
        [System.NonSerialized]
        public List<MapTrafficLane> lanesReverse = new List<MapTrafficLane>();
    }
    public class MapLine:MonoBehaviour
    {
        public Vector3 pos;
        public Color color;
        [System.NonSerialized]
        public List<MapSignal> signals = new List<MapSignal>();
        [System.NonSerialized]
        public MapIntersection intersection;
        [System.NonSerialized]
        public string id = null;
        public List<MapTrafficLane> befores { get; set; } = new List<MapTrafficLane>();
        public List<MapTrafficLane> afters { get; set; } = new List<MapTrafficLane>();
        public List<Vector3> mapWorldPositions = new List<Vector3>();
    }
    public class MapSignal : MonoBehaviour
    {

        public MapLine stopLine;
    }
    public class MapIntersection : MonoBehaviour
    {
        [System.NonSerialized]
        public List<MapSignal> facingGroup = new List<MapSignal>();
        [System.NonSerialized]
        public List<MapSignal> oppFacingGroup = new List<MapSignal>();
        [System.NonSerialized]
        public List<MapSignal> currentSignalGroup = new List<MapSignal>();
        [System.NonSerialized]
        public List<MapLine> stopLines = new List<MapLine>();


        [System.NonSerialized]
        List<MapSignal> signalGroup = new List<MapSignal>();

    }

    public class MapTrafficLane: MapLine
    {

        public float speedLimit = 20.0f;
        //public void ReversePoints()
        //{
        //    if (mapLocalPositions.Count < 2) return;

        //    mapLocalPositions.Reverse();

        //    // For parking, self-reverse lane should not have same waypoint coordinates.
        //    for (int i = 0; i < mapLocalPositions.Count; i++)
        //        mapLocalPositions[i] = new Vector3((float)(mapLocalPositions[i].x + 0.1), (float)(mapLocalPositions[i].y + 0.1), (float)(mapLocalPositions[i].z + 0.1));
        //}
    }
}
