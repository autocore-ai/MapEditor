using assets.OSMReader;
using Packages.BezierCurveEditorPackage.Scripts;
using System.Collections.Generic;
using UnityEngine;

namespace MapRenderer
{
    public class Line : Way
    {
        public float lineWidth = 0.1f;
        public Color color=Color.white;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        public BezierCurve bezierCurve;
        public List<Vector3> anchorPos=new List<Vector3>();

        public override void Start()
        {
            base.Start();
            if (!map.lines.Contains(this)) map.lines.Add(this);
        }
        public void StartEditElementWithBezierCurve()
        {
            if (bezierCurve == null) 
            {
                bezierCurve = gameObject.AddComponent<BezierCurve>();
                UpdateAnchorPos(points);
                for (int i = 0; i < anchorPos.Count; i++)
                {
                    AddBezierPoint(anchorPos[i]);
                }
                ElementEdit();
            }
            InitBZHelperPanel();
        }
        public override void ElementUpdateRenderer()
        {
            base.ElementUpdateRenderer();
            if (elemenrMaterial == null)
            {
                elemenrMaterial = new Material(Shader.Find("Standard"));
                elemenrMaterial.EnableKeyword("_EMISSION");
                elemenrMaterial.SetColor("_EmissionColor", color);
                elemenrMaterial.color = color;
            }

            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();
            CaculateEdgePoint();
            CaculateIndices();
            Mesh mesh = new Mesh();
            mesh.vertices = Vertexes.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshRenderer.material = elemenrMaterial;
        }

        public void AddPoint(Vector3 pos)
        {
            Point point = map.AddPoint(map.CreateElementName(), pos);
            points.Add(point);
            point.ElementUpdateRenderer();
            ElementUpdateRenderer();
        }
        public void RemoveLastPoint()
        {
            if(bezierCurve != null)
            {
                anchorPos.RemoveAt(anchorPos.Count - 1);
            }
            else
            {
                Point lastPoint = map.points[map.points.Count - 1];
                Destroy(lastPoint);
            }
            ElementEdit();
        }
        private void UpdateAnchorPos(List<Point> points)
        {
            anchorPos = new List<Vector3>();
            if (points.Count > 1)
            {
                anchorPos.Add(points[0].Position);
                anchorPos.Add(points[points.Count-1].Position);
            }
        }
        public void AddBezierPoint(Vector3 pos)
        {
            if(bezierCurve == null)bezierCurve=gameObject.AddComponent<BezierCurve>();
            BezierPoint point = bezierCurve.AddPointAt(pos);
            int index = bezierCurve.GetPointIndex(point);
            if (index <= 0)
            {
                point.handle1 = Vector3.zero;
            }
            else if (index > 0)
            {
                Vector3 offset = bezierCurve[index].position - bezierCurve[index - 1].position;
                point.handle1 = -offset / 4;
                if (index == 1)
                {
                    bezierCurve[0].handle1 = -offset / 4;
                }
            }
        }
        public void InitBZHelperPanel()
        {
            UIManager.Instance.bZHelperPanel.PanelInit(bezierCurve);
        }

        public void UpdateBZHelperPanel()
        {
            UIManager.Instance.bZHelperPanel.PanelUpdate(bezierCurve);
        }
        public override void MoveElement(Vector3 offset)
        {
            if(bezierCurve != null)
            {
                for (int i = 0; i < bezierCurve.pointCount; i++)
                {
                    bezierCurve[i].position += offset;
                }
                UpdateBZHelperPanel();
                for (int i = 0; i < pointsPosList.Count; i++)
                {
                    pointsPosList[i] += offset;
                }
                ElementEdit();
            }
            else
            {
                for (int i = 0; i < points.Count; i++)
                {
                    points[i].Position += offset;
                }
                ElementUpdateRenderer();
            }
        }
        public float lineLenth;
        private int resolution;
        public override void ElementEdit()
        {
            if (bezierCurve != null)
            {
                if (bezierCurve.pointCount > 1)
                {
                    pointsPosList = new List<Vector3>();
                    lineLenth = bezierCurve.length;
                    resolution = (int)lineLenth + 1;
                    for (int i = 0; i < resolution; i++)
                    {
                        float t = (float)i / (resolution - 1);
                        Vector3 pos = Absorb2Ground(bezierCurve.GetPointAt(t));
                        pointsPosList.Add(pos);
                    }
                    UpdateLine();
                }
            }
            foreach (Lanelet item in map.lanelets)
            {
                if (item.leftWay == this || item.rightWay == this)
                {
                    item.ElementUpdateRenderer();
                }
            }
            base.ElementEdit();
        }
        public override void OnDestory()
        {
            base.OnDestory();
            map.lines.Remove(this);
        }
        private void UpdateLine()
        {
            if (points == null) points = new List<Point>();
            //删除多余的Point
            for (int j = resolution; j < points.Count; j++)
            {
                Point tempPoint = points[j];
                points.RemoveAt(j);
                Destroy(tempPoint.gameObject);
            }
            //生成或者移动Point
            for (int i = 0; i < resolution; i++)
            {
                if (points.Count <= i)
                {
                    Point tempPoint = map.AddPoint(map.CreateElementName(), Absorb2Ground(pointsPosList[i]));
                    points.Add(tempPoint);
                }
                else
                {
                    if (points[i] == null) 
                    { 
                        points[i] = map.AddPoint(Absorb2Ground(pointsPosList[i]));
                    }
                    else
                    {

                        points[i].Position = Absorb2Ground(pointsPosList[i]);
                    }
                }
            }
        }
        public override void OnSelected()
        {
            base.OnSelected();
            EditManager.Instance.EditLine(this);
        }

        public List<Vector3> pointsPosList = new List<Vector3>();
        //碰撞体轮廓点位
        List<Vector3> Vertexes;
        private void CaculateEdgePoint()
        {
            #region 计算edgePoint
            //碰撞体宽度
            float colliderWidth = lineWidth;
            //Vector3转Vector2
            pointsPosList.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                if (points[i] == null)
                {
                    points.RemoveAt(i);
                    continue;
                }
                Vector3 tempPos = points[i].Position;
                if (!pointsPosList.Contains(tempPos))
                {
                    pointsPosList.Add(tempPos);
                }
            }
            Vertexes = new List<Vector3>();
            for (int j = 1; j < pointsPosList.Count; j++)
            {
                //当前点指向前一点的向量
                Vector3 distanceVector = pointsPosList[j - 1] - pointsPosList[j];
                //法线向量
                Vector3 crossVector = Vector3.Cross(distanceVector, Vector3.down);
                //标准化, 单位向量
                Vector3 offectVector = crossVector.normalized;
                //沿法线方向与法线反方向各偏移一定距离
                Vector3 left = pointsPosList[j - 1] + 0.5f * colliderWidth * offectVector;
                Vector3 right = pointsPosList[j - 1] - 0.5f * colliderWidth * offectVector;
                //把点加到list
                Vertexes.Add(left);
                Vertexes.Add(right);
                //加入最后一点
                if (j == pointsPosList.Count - 1)
                {
                    left = pointsPosList[j] + 0.5f * colliderWidth * offectVector;
                    right = pointsPosList[j] - 0.5f * colliderWidth * offectVector;
                    Vertexes.Add(left);
                    Vertexes.Add(right);
                }
            }
            #endregion
        }

        private List<int> indices;
        private void CaculateIndices()
        {
            indices=new List<int>();
            for (int i = 0; i < pointsPosList.Count-1; i++)
            {
                //第一个三角形
                indices.Add(i * 2);
                indices.Add(i * 2 + 2);
                indices.Add(i * 2 + 3);
                //第二个三角形
                indices.Add(i * 2);
                indices.Add(i * 2 + 3);
                indices.Add(i * 2 + 1);
            }
        }
    }
}
