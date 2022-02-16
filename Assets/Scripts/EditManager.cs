using ACGrpcServer;
using MapRenderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

public class EditManager : MonoBehaviour
{
    public static EditManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public enum EditMode
    {
        View,
        AddLaneLet,
        AddStopLine,
        AddWhiteLine,
        AddTrafficLight,
        EditLanelet,
        EditLine,
        SetTrafficLightTarget,
        SetTrafficLighRegulatory
    }
    public EditMode editMode = EditMode.View;

    public bool IsEditWithBezier = false;


    private Lanelet currentLanelet;
    private Line currentLine;
    private Sign_TrafficLight currenTrafficLight;
    public MapElement currentElement;
    public void EditElementWithBezier(bool isBezier)
    {
        IsEditWithBezier = isBezier;
    }


    public List<MapElement> SelectedElements = new List<MapElement>();
    public List<Point> selectedPoints = new List<Point>();
    private List<Line> selectedLines = new List<Line>();
    public bool IsMutiSelect;

    bool isAllPoint = false;
    bool isAllLine = false;
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.bZHelperPanel.OnBezierEdit += OnEditMapElement;

        GRPCManager.OnLoadFile += SetFileUrl;
        GRPCManager.OnSaveFile += MapManager.Instance.SaveMapToPath;
        GRPCManager.OnExit += Exit;
        GRPCManager.OnAddOSMMap +=MapManager.Instance.CreateMap;
        GRPCManager.OnBack += Back;
        GRPCManager.OnRedo += ReDo;
        GRPCManager.OnStartAddElement += StartAddElement;
        GRPCManager.OnEndAddElement += EndEdit;
        GRPCManager.OnSeverElementSelected += SelectElement;
        GRPCManager.OnSetTrafficLight += TrafficLightSetLine;
        GRPCManager.OnSetBezierMode += EditElementWithBezier;
    }

    private void TrafficLightSetLine(string id)
    {
        Debug.Log("SetTrafficLight");
        var element= MapManager.Instance.CurrentMap.GetElementByName(id);
        if (element is Sign_TrafficLight sign)
        {
            currenTrafficLight = sign;
            currentElement = element;
            editMode = EditMode.SetTrafficLightTarget;
        }
        else
        {
            //GRPCManager.
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type">
    /// lanelet=0
    /// whiteline=1
    /// stopline=2
    /// trafficlight=3
    /// 
    /// </param>
    public void StartAddElement(int type)
    {
        switch (type)
        {
            case 0:
                editMode = EditMode.AddLaneLet;
                break;
            case 1:
                editMode = EditMode.AddWhiteLine;
                break;
            case 2:
                if(selectedPoints.Count == 2)
                {
                    editMode = EditMode.AddStopLine;
                }
                else
                {
                    Debug.Log("need select two point");
                    GRPCManager.SenLogInfo("need select two point");
                }
                break;
            case 3:
                editMode = EditMode.AddTrafficLight;
                break;
            case 4:
                editMode = EditMode.SetTrafficLighRegulatory;
                break;
        }
    }

    public void EndEdit()
    {
        editMode = EditMode.View;
        foreach (var element in SelectedElements)
        {
            element.CancelSelect();
        }
        SelectedElements.Clear();
        UIManager.Instance.dragHelperPanel.HideDragHelper();
        currentElement = null;
        currenTrafficLight = null;
        currentLanelet = null;
        currentLine = null;
    }

    void SelectElement(string name)
    {
        MapElement element = MapManager.Instance.CurrentMap.GetElementByName(name);
        GRPCManager.SendCurrentElement(currentElement);
        SetSelectedElement(element);
    }
    void SendElementDataToServer()
    {
        if (currentElement != null)
            GRPCManager.SendElementData(currentElement);
    }
    public void SetSelectedElement(MapElement element)
    {
        currentElement = element;
        SendElementDataToServer();
    }


    public void EditLaneLet(Lanelet lanelet)
    {
        currentLanelet = lanelet;
        editMode = EditMode.EditLanelet;
    }
    public void EditLine(Line line)
    {
        editMode = EditMode.EditLine;
        currentLine = line;
    }
    public void DeleleCurrentElement()
    {
        for (int i = 0; i < SelectedElements.Count; i++)
        {
            Destroy(SelectedElements[i].gameObject);
        }
        SelectedElements.Clear();
        EndEdit();
    }
    public void CopyLanelet()
    {
        if(currentElement is Lanelet lanelet)
        {
            currentLanelet=lanelet;

        }
    }

    Ray mainRay;
    public string path;

    // Update is called once per frame

    void Update()
    {
        mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(mainRay, out RaycastHit groundHit, 100, CameraManager.Instance.GroundLayerMask))
        {
            CameraManager.Instance.MousePos = groundHit.point;
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            MergeElement();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            GRPCManager.OnLoadFile.Invoke(path);
            //MapManager.Instance.SaveMapToPath(Path.Combine(Application.streamingAssetsPath, "osm.osm"));
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            StartAddElement(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
           StartAddElement(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartAddElement(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartAddElement(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartAddElement(4);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            EditElementWithBezier(true);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            EndEdit();
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeleleCurrentElement();
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            IsMutiSelect = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            IsMutiSelect = false;
        }
        if (Input.GetKeyDown(KeyCode.S)&& IsMutiSelect)
        {
            GRPCManager.OnSaveFile.Invoke(Path.Combine(Application.streamingAssetsPath, "osm.osm"));
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (SelectedElements.Count == 1)
            {
                switch (SelectedElements[0])
                {
                    case Point point:
                        Debug.Log("point can't edit bezier");
                        break;
                    case Line line:
                        EditLine(line);
                        break;
                    case Lanelet lanelet:
                        EditLaneLet(lanelet);
                        break;
                }
            }
        }

        if (EventSystem.current.IsPointerOverGameObject() || CameraManager.Instance.IsMovingCamera) return;

        if (Input.GetMouseButtonDown(0))
        {
            CameraManager.Instance.CameraSetTarget();
        }
        switch (editMode)
        {
            case EditMode.View:
                UIManager.Instance.bZHelperPanel.PanelClear();
                break;
            case EditMode.AddWhiteLine:
                currentLine = MapManager.Instance.CurrentMap.AddWhiteLine();
                currentElement = currentLine;
                editMode = EditMode.EditLine;
                break;
            case EditMode.AddStopLine:
                if (selectedPoints.Count == 2)
                {
                    currentLine = MapManager.Instance.CurrentMap.AddStopLine();
                    currentElement = currentLine;
                    currentLine.points.AddRange(selectedPoints);
                    currentLine.ElementEdit();
                    EndEdit();
                }
                break;
            case EditMode.AddTrafficLight:
                if (currenTrafficLight == null)
                {
                    currenTrafficLight = MapManager.Instance.CurrentMap.AddTrafficLight();
                    currentElement = currenTrafficLight;
                }
                if (currenTrafficLight.points.Count >= 2)
                {
                    currenTrafficLight.ElementUpdateRenderer();
                    editMode = EditMode.View;
                    EndEdit();
                }
                if (Input.GetMouseButtonDown(0))
                {
                    Point point = MapManager.Instance.CurrentMap.AddPoint(CameraManager.Instance.MousePos+new Vector3(0,3,0));
                    currenTrafficLight.points.Add(point);
                }
                break;
            case EditMode.AddLaneLet:
                currentLanelet = MapManager.Instance.CurrentMap.AddLaneLet();
                currentElement = currentLanelet;
                editMode = EditMode.EditLanelet;
                break;
            case EditMode.EditLine:
                if (currentElement == null)
                {
                    editMode = EditMode.View;
                    return;
                }
                if (currentElement is Line line) currentLine = line;
                else
                {
                    editMode = EditMode.View;
                    return;
                }
                if (IsEditWithBezier)
                {
                    if (currentLine.bezierCurve == null)
                    {
                        currentLine.StartEditElementWithBezierCurve();
                    }
                    if (currentLine.pointsPosList != null)
                    {
                        currentLine.InitBZHelperPanel();
                    }
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        currentLine.AddBezierPoint(CameraManager.Instance.MousePos);
                        currentLine.InitBZHelperPanel();
                        currentLine.ElementEdit();
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        currentLine.AddPoint(CameraManager.Instance.MousePos);
                    }
                }
                break;
            case EditMode.EditLanelet:
                if (currentElement == null)
                {
                    editMode = EditMode.View;
                    return;
                }
                if (currentElement is Lanelet lanelet) currentLanelet = lanelet;
                else
                {
                    editMode = EditMode.View;
                    return;
                }
                if (IsEditWithBezier)
                {
                    if (currentLanelet.bezierCurve == null)
                    {
                        currentLanelet.StartEditElementWithBezierCurve();
                    }
                    if (currentLanelet.anchorPos != null)
                    {
                        currentLanelet.InitBZHelperPanel();
                    }
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        currentLanelet.AddBezierPoint(CameraManager.Instance.MousePos);
                        currentLanelet.InitBZHelperPanel();
                        currentLanelet.ElementEdit();
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                    {
                        currentLanelet.AddPoint(CameraManager.Instance.MousePos);
                    }
                }
                break;
            case EditMode.SetTrafficLightTarget:
                if (Input.GetMouseButton(0))
                {
                    mainRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(mainRay, out RaycastHit ElementHit, 100, CameraManager.Instance.ElementLayerMask))
                    {
                        var targetLine = ElementHit.transform.GetComponent<Line>();
                        if (targetLine != null)
                        {
                            currenTrafficLight.targetWay = targetLine;
                            Debug.Log(currenTrafficLight.name+" set target "+targetLine.name);
                            EndEdit();
                        }
                    }
                }
                break;
            case EditMode.SetTrafficLighRegulatory:
                if (SelectedElements.Count == 2)
                {
                    Line_StopLine line_StopLineTemp=null;
                    if (SelectedElements[0] is Line_StopLine stopLine1)
                    {
                        line_StopLineTemp=stopLine1;
                    }
                    else if (SelectedElements[1] is Line_StopLine stopLine2)
                    {
                        line_StopLineTemp = stopLine2;
                    }
                    if (line_StopLineTemp != null)
                    {
                        Sign_TrafficLight sign_TrafficLight = null;

                        if (SelectedElements[0] is Sign_TrafficLight sign_TrafficLight1)
                        {
                            sign_TrafficLight = sign_TrafficLight1;
                        }
                        else if (SelectedElements[1] is Sign_TrafficLight sign_TrafficLight2)
                        {
                            sign_TrafficLight = sign_TrafficLight2;
                        }
                        if (sign_TrafficLight!= null)
                        {
                            sign_TrafficLight.targetWay = line_StopLineTemp;
                            line_StopLineTemp.ControlSign = sign_TrafficLight;
                            EndEdit();
                        }
                    }
                }
                break;
            default:
                break;

        }
    }

    public void SetFileUrl(string path)
    {
        MapManager.Instance.LoadMap(path);
    }
    void Exit()
    {
        Application.Quit();
    }
    void Back()
    {
        Debug.Log("back");
    }
    void ReDo()
    {
        Debug.Log("redo");

    }
    public void OnEditMapElement()
    {
        currentElement.ElementEdit();
    }
    public void EditLaneLet()
    {
        editMode = EditMode.EditLanelet;
    }

    public void EditLine()
    {
        editMode = EditMode.EditLine;
    }

    public void MoveSelectedElements(Vector3 offset)
    {
        if (SelectedElements.Count != 0)
        {
            foreach (MapElement element in SelectedElements)
            {
                if (element == null) continue;
                element.MoveElement(offset);
                if (element is Point point)
                {
                    foreach (Way way in MapManager.Instance.CurrentMap.ways)
                    {
                        if (way.points.Contains(point))
                        {
                            way.ElementUpdateRenderer();
                        }
                    }
                    foreach (Relation relation in MapManager.Instance.CurrentMap.relations)
                    {
                        if (relation is Lanelet lanelet)
                        {
                            if (lanelet.leftWay.points.Contains(point) || lanelet.rightWay.points.Contains(point))
                            {
                                lanelet.ElementUpdateRenderer();
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log("not select");
            Debug.Log(SelectedElements.Count);
        }
    }
    public void SaveMoveCommand(Vector3 offset)
    {
        MoveCommand cmd = new MoveCommand(SelectedElements,offset);
        CommandManager.Instance.AddCommand(cmd);
    }
    public void MergeElement()
    {
        if (SelectedElements.Count < 2)
        {
            GrpcClient.Instance.SendMessage("Multiple points should be selected");
            return;
        }
        isAllPoint = true;
        isAllLine = true;
        selectedPoints = new List<Point>();
        selectedLines = new List<Line>();
        foreach (var item in SelectedElements)
        {
            if (item is Point point)
            {
                isAllLine = false;
                selectedPoints.Add(point);
            }
            if (item is Line line)
            {
                isAllPoint = false;
                selectedLines.Add(line);
            }
            else
            {
                GrpcClient.Instance.SendMessage("Can't merge");
                return;
            }
        }
        if (!isAllPoint && !isAllLine)
        {
            GRPCManager.SenLogInfo("Can't merge");
            return;
        }
        else if (isAllLine)
        {
            Line mergedline = selectedLines[0];
            for (int i = 1; i < selectedLines.Count; i++)
            {
                Line line = selectedLines[i];
                foreach (Lanelet lanelet in MapManager.Instance.CurrentMap.lanelets)
                {
                    if (lanelet.leftWay == line)
                    {
                        lanelet.leftWay = mergedline;
                        lanelet.ElementUpdateRenderer();
                    }
                    if (lanelet.rightWay == line)
                    {
                        lanelet.rightWay = mergedline;
                        lanelet.ElementUpdateRenderer();
                    }
                }
                Destroy(line.gameObject);
            }
        }
        else if (isAllPoint)
        {
            Point CenterPoint = MapManager.Instance.CurrentMap.AddPoint(MapManager.Instance.CurrentMap.CreateElementName(),CameraManager.Instance.cameraSelectElements.SelectedElementsCenterPos);
            for (int i = 0; i < selectedPoints.Count; i++)
            {
                Point point = selectedPoints[i];
                foreach (Way way in MapManager.Instance.CurrentMap.ways)
                {
                    if (way.points.Contains(point))
                    {
                        if (!way.points.Contains(CenterPoint))
                        {
                            int index = way.points.IndexOf(point);
                            way.points[index] = CenterPoint;
                        }
                        Destroy(point.gameObject);
                    }
                }

            }
            SelectedElements.Clear();
            SelectedElements.Add(CenterPoint);
        }
        GRPCManager.SenLogInfo("merged");
    }
}
