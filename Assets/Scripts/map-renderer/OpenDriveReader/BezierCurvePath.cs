using MapRenderer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace assets.OpenDriveReader
{
    [Serializable]
    public class LaneAttribute
    {
        public int laneID;
        public float width;
        public string color;
        public string type;
        public string material;
        public float totalWidth;
    }
    public class BezierCurvePath : MonoBehaviour
    {
        public List<BezierCurveData> curveDatas = new List<BezierCurveData>();
        public List<Transform> archors = new List<Transform>();
        public List<Vector3> archorPoss=new List<Vector3>();
        public List<Vector3> archorsRight = new List<Vector3>();
        private readonly float HALF_PI = 0.5f * Mathf.PI;
        public float curveGranularity = 100f;
        public float pathLenth;
        public List<LaneAttribute> laneAttributes=new List<LaneAttribute>();

        public void Generator(float x, float y, float len, float angle, float curvature)
        {
            float radius;
            float centerX;
            float centerY;

            bool clockwise;
            float startAngle;
            float roadLength;

            float theta;
            float arcTheta;

            float x_end;
            float y_end;

            if (curvature == 0)
            {
                x_end = len * Mathf.Cos(angle);
                y_end = len * Mathf.Sin(angle);
                float add_pointx = x_end / 3;
                float add_pointy = y_end / 3;

                float point1x, point2x, point3x, point4x;
                float point1y, point2y, point3y, point4y;

                point1x = x;
                point2x = point1x + add_pointx;
                point3x = point2x + add_pointx;
                point4x = point3x + add_pointx;

                point1y = y;
                point2y = point1y + add_pointy;
                point3y = point2y + add_pointy;
                point4y = point3y + add_pointy;

                var curve = new BezierCurveData();
                curve.points = new Vector3[4]
                {
                new Vector3 (point1x, 0.1f, point1y),
                new Vector3 (point2x, 0.1f, point2y),
                new Vector3 (point3x, 0.1f, point3y),
                new Vector3 (point4x, 0.1f, point4y)
                };
                curveDatas.Add(curve);


                float xEnd = len * Mathf.Cos(angle);
                float yEnd = len * Mathf.Sin(angle);
                Vector3 startPos = new Vector3(x, 0.1f, y);
                Vector3 endPos = new Vector3(x + xEnd, 0.1f, y + yEnd);
                archorPoss.Add(startPos);
                archorPoss.Add(endPos);
            }
            else
            {
                startAngle = angle;
                roadLength = len;
                radius = 1.0f / Mathf.Abs(curvature);
                clockwise = curvature < 0.0f;
                centerX = x - radius * Mathf.Cos(startAngle - HALF_PI) * (clockwise ? -1.0f : 1.0f);
                centerY = y - radius * Mathf.Sin(startAngle - HALF_PI) * (clockwise ? -1.0f : 1.0f);

                float roadPos = 0.0f;
                theta = clockwise ? startAngle - roadPos / radius : startAngle + roadPos / radius;

                arcTheta = theta - HALF_PI;
                float r = radius * (clockwise ? 1.0f : -1.0f);

                if (roadLength < 750) r = -r;
                Vector3 first_vector = GetCurvePart(startAngle, r - 10, centerX, centerY);

                var end_vector = new Vector3(x, 0.01f, y);
                var start_vector = first_vector;
                int count = 0;
                float distance = 0f;

                while (true)
                {
                    startAngle = startAngle + 0.05f;
                    Vector3 second_vector = GetCurvePart(startAngle, r - 10, centerX, centerY);
                    BezierCurveData curve_new;
                    count++;
                    distance += Vector3.Distance(start_vector, second_vector);

                    if (distance >= roadLength)
                    {
                        curve_new = GetNewCurve(start_vector, end_vector);
                        curveDatas.Add(curve_new);
                        archorPoss.Add(end_vector);
                        break;
                    }
                    curve_new = GetNewCurve(start_vector, second_vector);
                    curveDatas.Add(curve_new);
                    archorPoss.Add(start_vector);
                    start_vector = second_vector;
                }
            }

            pathLenth = 0;
            foreach (BezierCurveData curveData in curveDatas)
            {
                pathLenth += curveData.GetApproximateLength((int)curveGranularity);
                Transform curveHelper = new GameObject("helper").transform;
                curveHelper.transform.SetParent(transform);
                curveHelper.position = transform.position + curveData.GetPoint(0);
                curveHelper.LookAt(transform.position + curveData.GetPoint(1));
                archors.Add(curveHelper);
                archorsRight.Add(curveHelper.right);
            }
        }
        public void CreateLines()
        {
            foreach (LaneAttribute laneAttribute in laneAttributes)
            {
                float totalWidth = laneAttribute.width;
                if (laneAttribute.laneID > 0)
                {
                    foreach (LaneAttribute laneAttribute2 in laneAttributes)
                    {
                        if(laneAttribute2.laneID > 0 && laneAttribute2.laneID< laneAttribute.laneID)
                        {
                            totalWidth+= laneAttribute.width;
                        }
                    }

                }
                else if (laneAttribute.laneID < 0)
                {
                    foreach (LaneAttribute laneAttribute2 in laneAttributes)
                    {
                        if (laneAttribute2.laneID < 0 && laneAttribute2.laneID > laneAttribute.laneID)
                        {
                            totalWidth += laneAttribute.width;
                        }
                    }
                }
                laneAttribute.totalWidth = totalWidth;
                List<Point> points = new List<Point>();
                float offset = totalWidth;
                for (int i = 0; i < archorPoss.Count; i++)
                {
                    Vector3 direction;
                    if (i >= archorsRight.Count)
                    {
                        direction = archorsRight[archorsRight.Count - 1].normalized;
                    }
                    else
                    {
                        direction = archorsRight[i].normalized;
                    }
                    points.Add(OpenDriveManager.Instance.map.AddPoint(OpenDriveManager.Instance.PointIndex.ToString(), archorPoss[i] + direction * offset));
                }
                Line_WhiteLine line = OpenDriveManager.Instance.map.AddWhiteLine("Line" + OpenDriveManager.Instance.LineIndex.ToString());
                line.points = points;
                switch (laneAttribute.color)
                {
                    case "yellow":
                        line.color = Color.yellow;
                        break;
                    case "white":
                        line.color = Color.white;
                        break;
                    case "red":
                        line.color = Color.red;
                        break;
                    default:
                        line.color = Color.white;
                        break;
                }
            }
        }
        Transform CreatePathHelper()
        {
            GameObject go = new GameObject("pahtHelper");
            go.transform.SetParent(transform);
            return go.transform;
        }

        Vector3 GetCurvePart(float start_angle, float r, float center_x, float center_y)
        {
            float start_x = 0.0f;
            float start_y = 0.0f;

            start_x = r * Mathf.Cos(start_angle);
            start_y = r * Mathf.Sin(start_angle);

            return new Vector3(start_x + center_x, 0.1f, start_y + center_y);
        }
        BezierCurveData GetNewCurve(Vector3 prev_vector, Vector3 end_vector)
        {
            var curve_data = new BezierCurveData();
            curve_data.points = new Vector3[4]{
            prev_vector,
            prev_vector,
            end_vector,
            end_vector
            };
            return curve_data;
        }
    }
}
