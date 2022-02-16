using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using MapRenderer;
using assets.OSMReader;
using ACGrpcServer;

public class CameraSelectElements : MonoBehaviour
{
    public Color rectColor = Color.green;
    public Vector3 posStart = Vector3.zero;
    public bool drawRectangle = false;

    public bool IsMutiSelect;
    // Start is called before the first frame update
    void Start()
    {
        CreateLineMaterial();
    }
    Ray mainRay;

    // Update is called once per frame

    void Update()
    {
        if (EditManager.Instance.editMode == EditManager.EditMode.View)
        {
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if (!IsMutiSelect)
                {
                    foreach (var item in EditManager.Instance.SelectedElements)
                    {
                        if (item != null) item.CancelSelect();
                    }
                    EditManager.Instance.SelectedElements.Clear();
                }
                //UIManager.Instance.toolPanel.ClosePanelTool();
                CameraManager.Instance.CameraSetTarget();
                drawRectangle = true;
                posStart = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                drawRectangle = false;
                CheckSelection(posStart, posEnd);
            }
        }



        if (drawRectangle)
        {
            posEnd = Input.mousePosition;
        }

    }
    public Material lineMaterial;
    public void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
    public Vector3 posEnd;
    void OnPostRender()
    {
        if (drawRectangle)
        {
            if (lineMaterial == null)
            {
                Debug.Log("No mat");
                return;
            }
            lineMaterial.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Color(new Color(rectColor.r, rectColor.g, rectColor.b, 0.1f));
            GL.Vertex(new Vector3(posStart.x / Screen.width, posStart.y / Screen.height, 0));
            GL.Vertex(new Vector3(posEnd.x / Screen.width, posStart.y / Screen.height, 0));
            GL.Vertex(new Vector3(posEnd.x / Screen.width, posEnd.y / Screen.height, 0));
            GL.Vertex(new Vector3(posStart.x / Screen.width, posEnd.y / Screen.height, 0));
            GL.End();

            GL.Begin(GL.LINES);
            GL.Color(rectColor);//设置方框的边框颜色 边框不透明  
            GL.Vertex3(posStart.x / Screen.width, posStart.y / Screen.height, 0);
            GL.Vertex3(posEnd.x / Screen.width, posStart.y / Screen.height, 0);
            GL.Vertex3(posEnd.x / Screen.width, posStart.y / Screen.height, 0);
            GL.Vertex3(posEnd.x / Screen.width, posEnd.y / Screen.height, 0);
            GL.Vertex3(posEnd.x / Screen.width, posEnd.y / Screen.height, 0);
            GL.Vertex3(posStart.x / Screen.width, posEnd.y / Screen.height, 0);
            GL.Vertex3(posStart.x / Screen.width, posEnd.y / Screen.height, 0);
            GL.Vertex3(posStart.x / Screen.width, posStart.y / Screen.height, 0);
            GL.End();
        }
    }
    void CheckSelection(Vector3 start, Vector3 end)
    {
        if (MapManager.Instance.CurrentMap == null || MapManager.Instance.CurrentMap.points == null || MapManager.Instance.CurrentMap.points.Count == 0)
        {
            return;
        }
        Vector3 p1 = Vector3.zero;
        Vector3 p2 = Vector3.zero;
        if (start.x > end.x)
        {
            //这些判断是用来确保p1的xy坐标小于p2的xy坐标，因为画的框不见得就是左下到右上这个方向的  
            p1.x = end.x;
            p2.x = start.x;
        }
        else
        {
            p1.x = start.x;
            p2.x = end.x;
        }
        if (start.y > end.y)
        {
            p1.y = end.y;
            p2.y = start.y;
        }
        else
        {
            p1.y = start.y;
            p2.y = end.y;
        }
        foreach (Point point in MapManager.Instance.CurrentMap.points)
        {
            if (point == null)
            {
                continue;
            }
            //把可选择的对象保存在SelectedElements里 
            Vector3 location = CameraManager.Instance.m_camera.WorldToScreenPoint(point.transform.position);//把对象的position转换成屏幕坐标  
            if (location.x < p1.x || location.x > p2.x || location.y < p1.y || location.y > p2.y
                || location.z < CameraManager.Instance.m_camera.nearClipPlane || location.z > CameraManager.Instance.m_camera.farClipPlane)//z方向就用摄像机的设定值，看不见的也不需要选择了  
            {
                if (EditManager.Instance.selectedPoints.Contains(point))
                {
                    point.CancelSelect();
                    EditManager.Instance.selectedPoints.Remove(point);
                }
            }
            else
            {
                if (!EditManager.Instance.selectedPoints.Contains(point))
                {
                    point.OnSelected();
                    EditManager.Instance.selectedPoints.Add(point);
                }
            }
        }
        //如果框选没选到点，用射线检测看当前是否选到元素
        if (EditManager.Instance.selectedPoints.Count == 0)
        {
            mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(mainRay, out RaycastHit ElementHit, 100, CameraManager.Instance.ElementLayerMask))
            {
                var element = ElementHit.transform.GetComponentInParent<MapElement>();
                if (element != null)
                {
                    EditManager.Instance.SelectedElements.Add(element);
                    element.OnSelected();
                }
            }
            else
            {
                Debug.Log("not selected");
            }
        }
        else
        {
            EditManager.Instance.SelectedElements.AddRange(EditManager.Instance.selectedPoints);
        }
        if (EditManager.Instance.SelectedElements.Count != 0)
        {
            ShowSelectedCenterPos();
        }
        else
        {
            UIManager.Instance.dragHelperPanel.HideDragHelper();
        }
    }

    public Vector3 SelectedElementsCenterPos;
    private void ShowSelectedCenterPos()
    {
        SelectedElementsCenterPos = Vector3.zero;
        if (EditManager.Instance.SelectedElements.Count == 0) return;
        if (EditManager.Instance.SelectedElements.Count == 1)
        {
            switch (EditManager.Instance.SelectedElements[0])
            {
                case Point point:
                    SelectedElementsCenterPos = point.Position;
                    break;
                case Line line:
                    SelectedElementsCenterPos = (line.pointsPosList[0] + line.pointsPosList[line.pointsPosList.Count - 1]) / 2;
                    break;
                case Lanelet lanelet:
                    SelectedElementsCenterPos = (lanelet.LanePoses[0] + lanelet.LanePoses[lanelet.LanePoses.Count - 1]) / 2;
                    break;
            }
        }
        else
        {
            foreach (MapElement ele in EditManager.Instance.SelectedElements)
            {
                if (ele != null)
                {
                    switch (ele)
                    {
                        case Point point:
                            SelectedElementsCenterPos += point.Position;
                            break;
                        case Line line:
                            SelectedElementsCenterPos += (line.pointsPosList[0] + line.pointsPosList[line.pointsPosList.Count - 1]) / 2;
                            break;
                        case Lanelet lanelet:
                            SelectedElementsCenterPos += (lanelet.anchorPos[0] + lanelet.anchorPos[lanelet.anchorPos.Count - 1]) / 2;
                            break;
                    }

                }
            }
            SelectedElementsCenterPos = SelectedElementsCenterPos / EditManager.Instance.SelectedElements.Count;
        }
        UIManager.Instance.dragHelperPanel.PanelInit(SelectedElementsCenterPos);
    }

}
