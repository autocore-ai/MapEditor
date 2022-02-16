using Packages.BezierCurveEditorPackage.Scripts;
using Schemas;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MapRenderer
{
    public class Lanelet : Relation
    {
        public Color color;
        public Way leftWay;
        public Way rightWay;
        public float laneWidth = 3.0f;
        public float laneLenth;
        public int resolution;
        public List<Vector3> LanePoses;
        private List<Vector3> LaneDirections;

        public BezierCurve bezierCurve;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MeshCollider meshCollider;
        private List<int> indices;
        //顶点数组
        private Vector3[] Vertexes;

        public bool isLeftWayReverse = false;

        public override void Start()
        {
            base.Start();

        }
        public enum TurnDirection
        {
            Null,
            Straight,
            Left,
            Right
        }
        public TurnDirection turnDirection = TurnDirection.Null;

        public void SetDirection(TurnDirection direction)
        {
            turnDirection = direction;
            if(turnDirection == TurnDirection.Null)
            {
                RemoveTag("turn_direction");
            }
            else
            {
                AddOrEditTag("turn_direction", turnDirection.ToString());
            }
        }
        public float speed_limit = 60;
        public void SetMaxSpeed(float speed)
        {
            speed_limit = speed;
            AddOrEditTag("speed_limit", speed_limit+"km/h");
        }
        private void Awake()
        {
        }

        public List<Vector3> anchorPos = new List<Vector3>();
        public void StartEditElementWithBezierCurve()
        {
            if (bezierCurve == null)
            {
                bezierCurve = gameObject.AddComponent<BezierCurve>();
                for (int i = 0; i < anchorPos.Count; i++)
                {
                    AddBezierPoint(anchorPos[i]);
                }
                ElementEdit();
            }
            InitBZHelperPanel();
        }

        public void AddBezierPoint(Vector3 pos)
        {
            if (bezierCurve == null) bezierCurve = gameObject.AddComponent<BezierCurve>();
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
        public void AddPoint(Vector3 pos)
        {
            anchorPos.Add(pos); 
            ElementEdit();
            ElementUpdateRenderer();
        }
        public override void UpdateElementData()
        {
            base.UpdateElementData();
            AddOrEditTag("type", "lanelet");
            AddOrEditTag("subtype", "road");
            SetMaxSpeed(60);
            SetDirection(turnDirection);
        }
        public override void ElementUpdateRenderer()
        {
            base.ElementUpdateRenderer();
            //elemenrMaterial.color = color;
            //elemenrMaterial.SetColor("_EmissionColor", color);
            if (elemenrMaterial == null)
            {
                elemenrMaterial = new Material(Shader.Find("Custom/DoubleTransparent"));
                color = Color.green;
                color.a = 0.5f;
                elemenrMaterial.color = color;
                elemenrMaterial.SetColor("_EmissionColor", color);
            }
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            if (meshCollider == null) meshCollider = gameObject.AddComponent<MeshCollider>();

            if (leftWay == null || rightWay == null) return;
            if (leftWay.points.Count <= 2 && rightWay.points.Count <= 2) return;
            int leftCount = leftWay.points.Count;
            int rightCount = rightWay.points.Count;
            Vector3 dirL = leftWay.points[leftCount - 1].Position - leftWay.points[0].Position;
            Vector3 dirR = rightWay.points[rightCount - 1].Position - rightWay.points[0].Position;
            isLeftWayReverse = IsSameDir(dirL, dirR);
            Vertexes = new Vector3[leftCount + rightCount];
            for (int i = 0; i < leftCount; i++) Vertexes[i] = leftWay.points[i].Position;
            if (isLeftWayReverse)
            {
                for (int i = 0; i < rightCount; i++) 
                { 
                    Vertexes[leftCount + i] = rightWay.points[i].Position;
                }
            }
            else
            {
                for (int i = 0; i < rightWay.points.Count; i++) 
                { 
                    Vertexes[Vertexes.Length-1 - i] = rightWay.points[i].Position; 
                }
            }
            color.a = 0.5f;

            //三角形顶点ID数组
            indices = new List<int>();
            int indexLeft = 0;
            int indexRight = 0;
            while (indexLeft < leftCount - 1 && indexRight < rightCount - 1)
            {
                indexLeft += 1;
                indexRight += 1;
                if (indexLeft >= leftCount) indexLeft = leftCount - 1;
                else
                {
                    indices.Add(indexLeft);
                    indices.Add(leftCount + indexRight - 1);
                    indices.Add(indexLeft - 1);
                }
                if (indexRight >= rightCount) indexRight = rightCount - 1;
                else
                {
                    indices.Add(indexLeft);
                    indices.Add(leftCount + indexRight);
                    indices.Add(leftCount + indexRight - 1);
                }
            }
            Mesh mesh = new Mesh();
            mesh.vertices = Vertexes;
            //mesh.uv = uvs;
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            //areaMaterial.color = areaColor;
            meshRenderer.material = elemenrMaterial;
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
            for (int i = 0; i < anchorPos.Count; i++)
            {
                anchorPos[i] += offset;
            }
            for (int i = 0; i < LanePoses.Count; i++)
            {
                LanePoses[i] += offset;
            }
            if (bezierCurve!=null)
            {
                for (int i = 0; i < bezierCurve.pointCount; i++)
                {
                    bezierCurve[i].position += offset;
                }
            }
            UpdateBZHelperPanel();
            ElementEdit();
        }
        private void Update()
        {
        }


        public override void OnSelected()
        {
            base.OnSelected();
            EditManager.Instance.EditLaneLet(this);
        }
        public override void ElementEdit()
        {
            LanePoses = new List<Vector3>();
            LaneDirections = new List<Vector3>();
            if (leftWay == null)
            {
                leftWay = map.AddWhiteLine();
            }
            if (rightWay == null)
            {
                rightWay = map.AddWhiteLine();
            }
            if (bezierCurve == null)
            {
                if (anchorPos.Count > 1)
                {
                    resolution = 0;
                    for (int i = 1; i <anchorPos.Count; i++)
                    {
                        int dis = (int)Vector3.Distance(anchorPos[i], anchorPos[i - 1])+1;
                        resolution += dis;
                        Vector3 dir= (anchorPos[i]-anchorPos[i-1]).normalized;
                        LanePoses.Add(anchorPos[i-1]);
                        for (int j = 1; j < dis; j++)
                        {
                            Vector3 pos = Absorb2Ground(anchorPos[i]+j*dir);
                            LanePoses.Add(pos);
                        }
                    }
                    LanePoses.Add(anchorPos[anchorPos.Count - 1]);
                    for (int i = 0; i < LanePoses.Count; i++)
                    {
                        Vector3 dirction = Vector3.zero;
                        if (i == 0)
                        {
                            dirction = (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i + 1] - LanePoses[i])).normalized;
                        }
                        else if (i >= LanePoses.Count - 1)
                        {
                            dirction = (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i] - LanePoses[i - 1])).normalized;
                        }
                        else
                        {
                            dirction += (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i + 1] - LanePoses[i])).normalized;
                            dirction += (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i] - LanePoses[i - 1])).normalized;
                            dirction = dirction.normalized;
                        }
                        LaneDirections.Add(dirction);
                    }
                }
            }
            else
            {
                if (bezierCurve.pointCount > 1)
                {
                    laneLenth = bezierCurve.length;
                    resolution = (int)laneLenth + 1;
                    for (int i = 0; i < resolution; i++)
                    {
                        float t = (float)i / (resolution - 1);
                        Vector3 pos = Absorb2Ground(bezierCurve.GetPointAt(t));
                        LanePoses.Add(pos);
                    }
                    for (int i = 0; i < resolution; i++)
                    {
                        Vector3 dirction = Vector3.zero;
                        if (i == 0) 
                        { 
                            dirction = (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i + 1] - LanePoses[i])).normalized; 
                        }
                        else if (i == resolution - 1)
                        {
                            dirction = (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i] - LanePoses[i - 1])).normalized;
                        }
                        else
                        {
                            dirction += (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i + 1] - LanePoses[i])).normalized;
                            dirction += (Quaternion.AngleAxis(90, Vector3.up) * (LanePoses[i] - LanePoses[i - 1])).normalized;
                            dirction = dirction.normalized;
                        }
                        LaneDirections.Add(dirction);
                    }
                }

            }
            UpdateLanedata();
            leftWay?.ElementUpdateRenderer();
            rightWay?.ElementUpdateRenderer();
            base.ElementEdit();
        }
        private void UpdateLanedata()
        {
            if (leftWay.points == null) leftWay.points = new List<Point>();
            if (rightWay.points == null) rightWay.points = new List<Point>();
            //删除多余的Point
            for (int j = resolution; j < leftWay.points.Count; j++)
            {
                Point tempLeftPoint = leftWay.points[j];
                Point tempRightPoint = rightWay.points[j];
                leftWay.points.RemoveAt(j);
                rightWay.points.RemoveAt(j);
                Destroy(tempLeftPoint.gameObject);
                Destroy(tempRightPoint.gameObject);
            }
            //生成或者移动Point
            for (int i = 0; i < resolution; i++)
            {
                if (leftWay.points.Count <= i)
                {
                    Point tempLeft = map.AddPoint(map.CreateElementName(), Absorb2Ground(LanePoses[i] - LaneDirections[i] * laneWidth / 2));
                    leftWay.points.Add(tempLeft);
                }
                else
                {
                    leftWay.points[i].Position = Absorb2Ground(LanePoses[i] - LaneDirections[i] * laneWidth / 2);
                }
                if (rightWay.points.Count <= i)
                {
                    Point tempRight = map.AddPoint(map.CreateElementName(), Absorb2Ground(LanePoses[i] + LaneDirections[i] * laneWidth / 2));
                    rightWay.points.Add(tempRight);
                }
                else
                {
                    rightWay.points[i].Position = Absorb2Ground(LanePoses[i] + LaneDirections[i] * laneWidth / 2);
                }
            }
        }

        private bool IsSameDir(Vector3 v1, Vector3 v2)
        {
            float dot = Vector3.Dot(v1, v2);
            return dot >= 0;
        }
        public override void OnDestory()
        {
            if (map.lanelets.Contains(this)) map.lanelets.Remove(this);
            base.OnDestory();
        }
        public void RemoveElement(MapElement element)
        {
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i].refID.ToString() == element.name)
                {
                    if(members[i].roleType==assets.OSMReader.Member.RoleType.left|| members[i].roleType == assets.OSMReader.Member.RoleType.right)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        members.Remove(members[i]);
                    }
                }
            }
        }
    }
}
