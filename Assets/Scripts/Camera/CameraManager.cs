using MapRenderer;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;
    public CameraMove cameraMove;
    public CameraSelectElements cameraSelectElements;
    public Camera cameraCoordinate;
    public Vector3 MousePos;
    public Vector3 TargetPos;
    public bool IsMovingCamera=false;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        m_camera = GetComponent<Camera>();
        cameraMove=GetComponent<CameraMove>();
        cameraSelectElements=GetComponent<CameraSelectElements>();
    }
    private void Start()
    {
        OnCameraMove += UIManager.Instance.bZHelperPanel.PanelUpdate;
        OnCameraMove += UIManager.Instance.dragHelperPanel.PanelUpdate;
    }
    public LayerMask GroundLayerMask;
    public LayerMask ElementLayerMask;
    public LayerMask CoordinateLayerMask;

    //private Vector3 mousePosDragStart;
    //private Vector3 gOScreenPosStart;
    //private bool isDrag;
    //private GameObject gODrag;
    //public LayerMask BezierHelpLayerMask;

    public Action OnCameraMove;
    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            cameraMove.CameraFocusAtTarget(TargetPos);
        }
      
    }
    public void CameraSetTarget(Vector3 target)
    {
        MousePos = target;
        CameraSetTarget();
    }
    public void CameraSetTarget()
    {
        TargetPos = MousePos;
        cameraMove.SetCameraTargetPos(TargetPos);
    }

    public Camera m_camera;
    public void SwitchProjection(out bool isOrth)
    {
        m_camera.orthographic = !m_camera.orthographic;
        isOrth = m_camera.orthographic;
    }
    public void SetCameraSize(float value)
    {
        if (m_camera.orthographic)
        {
            m_camera.orthographicSize = value;
        }
    }
    public void SetCameraFov(float value)
    {
        if (!m_camera.orthographic)
        {
            m_camera.fieldOfView = value;
        }
    }
    public void ChangeCameraSize(float scale)
    {
        if (scale == 0) return;
        if (m_camera.orthographic)
        {
            float size = m_camera.orthographicSize * (scale > 0 ? 1.1f : 0.9f);
            size = Mathf.Clamp(size, 1, 100);
            UIManager.Instance?.viewPanel?.SetCameraSize(size);
        }
        else
        {
            float size= m_camera.fieldOfView * (scale > 0 ? 1.1f : 0.9f);
            size = Mathf.Clamp(size, 20, 120);
            UIManager.Instance?.viewPanel?.SetCameraFOV(size);
        }
    }
    public Material mat;
    void OnPostRender()
    {
        if (Lines == null) return;
        mat.SetPass(0);

        GL.Color(new Color(1, 1, 0, 0.8F));

        GL.PushMatrix();
        GL.Begin(GL.LINES);
        foreach (List<Vector3> line in Lines)
        {
            for (int i = 0; i < line.Count-1; i++)
            {
                GL.Vertex(line[i]);
                GL.Vertex(line[i+1]);
            }
        }

        GL.End();
        GL.PopMatrix();
        Lines.Clear();
    }
    public List<List<Vector3>> Lines;
    public void ShowLineOnScreen(List<Vector3> line)
    {
        if (line.Count < 2) return;
        if(Lines == null) Lines = new List<List<Vector3>>();
        Lines.Add(line);
        List<Vector3> tempLine1 = new List<Vector3>();

    }

}
