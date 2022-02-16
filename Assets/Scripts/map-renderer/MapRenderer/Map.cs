using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapRenderer
{
    public class Map : MonoBehaviour
    {
        public Dictionary<string, MapElement> Elements;
        public List<Point> points;
        public List<Way> ways;
        public List<Relation> relations;

        public List<Line> lines;
        public List<Area> areas;
        public List<Sign> signs;
        public List<Structure> structures;
        public List<Lanelet> lanelets;
        public List<RegulatoryElement> regulatoryElements;



        public Transform lineParent;
        public Transform SignParent;
        public Transform AreaParent;
        public Transform StructureParent;
        public Transform pointParent;
        public Transform laneLetParent;
        public Transform RegulatoryParent;

        public GameObject goTrafficLight;
        public Material panelMaterial;

        public GameObject goWhiteLine;

        public GameObject goStructure;

        public GameObject goArea;

        public GameObject goPoint;

        public GameObject goLaneLet;

        public Material pointMaterial;

        public MapElement GetElementByName(string name)
        {
            if (Elements.ContainsKey(name))
            {
                return Elements[name];
            }
            else
            {
                Debug.Log("no element");
                return null;
            }
        }
        public void Init()
        {
            if (lineParent == null)
            {
                lineParent = new GameObject("Lines").transform;
                lineParent.SetParent(transform);
            }
            if (SignParent == null)
            {
                SignParent = new GameObject("Signs").transform;
                SignParent.SetParent(transform);
            }
            if (AreaParent == null)
            {
                AreaParent = new GameObject("Areas").transform;
                AreaParent.SetParent(transform);
            }
            if (StructureParent == null)
            {
                StructureParent = new GameObject("Structures").transform;
                StructureParent.SetParent(transform);
            }
            if (pointParent == null)
            {
                pointParent = new GameObject("Points").transform;
                pointParent.SetParent(transform);
            }
            if (laneLetParent == null)
            {
                laneLetParent = new GameObject("LaneLets").transform;
                laneLetParent.SetParent(transform);
            }
            if (RegulatoryParent == null)
            {
                laneLetParent = new GameObject("Regulatorys").transform;
                laneLetParent.SetParent(transform);
            }
            Elements = new Dictionary<string, MapElement>();
            points = new List<Point>();
            ways = new List<Way>();
            relations = new List<Relation>();

            lines = new List<Line>();
            areas = new List<Area>();
            signs = new List<Sign>();
            structures = new List<Structure>();
            lanelets = new List<Lanelet>();
            while (lineParent.childCount != 0)
            {
                DestroyImmediate(lineParent.GetChild(0).gameObject);
            }
            while (SignParent.childCount != 0)
            {
                DestroyImmediate(SignParent.GetChild(0).gameObject);
            }
            while (AreaParent.childCount != 0)
            {
                DestroyImmediate(AreaParent.GetChild(0).gameObject);
            }
            while (StructureParent.childCount != 0)
            {
                DestroyImmediate(StructureParent.GetChild(0).gameObject);
            }

            panelMaterial = new Material(Shader.Find("Standard"));
            panelMaterial.EnableKeyword("_EMISSION");
            panelMaterial.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            panelMaterial.SetColor("_EmissionColor", new Color(0.5f, 0.5f, 0.5f));
            panelMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            panelMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            panelMaterial.SetInt("_ZWrite", 0);
            panelMaterial.DisableKeyword("_ALPHATEST_ON");
            panelMaterial.EnableKeyword("_ALPHABLEND_ON");
            panelMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            panelMaterial.renderQueue = 3000;
            if (goTrafficLight == null)
            {
                goTrafficLight = GameObject.CreatePrimitive(PrimitiveType.Cube);
                goTrafficLight.name = "Panel";
                goTrafficLight.transform.localScale = new Vector3(0.5f, 0.2f, 0.05f);
                goTrafficLight.GetComponent<MeshRenderer>().material = panelMaterial;
                Destroy(goTrafficLight.GetComponent<BoxCollider>());

                Material redMaterial = new Material(Shader.Find("Standard"));
                redMaterial.EnableKeyword("_EMISSION");
                redMaterial.SetColor("_EmissionColor", new Color(1, 0, 0));
                Material yellowMaterial = new Material(Shader.Find("Standard"));
                yellowMaterial.EnableKeyword("_EMISSION");
                yellowMaterial.SetColor("_EmissionColor", new Color(1, 1, 0));
                Material greenMaterial = new Material(Shader.Find("Standard"));
                greenMaterial.EnableKeyword("_EMISSION");
                greenMaterial.SetColor("_EmissionColor", new Color(0, 1, 0));

                GameObject red = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(goTrafficLight.GetComponent<CapsuleCollider>());
                red.name = "Red";
                red.transform.localScale = new Vector3(0.15f, 0.03f, 0.15f);
                red.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
                red.GetComponent<MeshRenderer>().material = redMaterial;
                red.transform.SetParent(goTrafficLight.transform);

                GameObject yellow = Instantiate(red, goTrafficLight.transform);
                yellow.name = "Yellow";
                yellow.transform.position = new Vector3(0.175f, 0, 0);
                yellow.GetComponent<MeshRenderer>().material = yellowMaterial;

                GameObject green = Instantiate(red, goTrafficLight.transform);
                green.name = "Green";
                green.transform.position = new Vector3(-0.175f, 0, 0);
                green.GetComponent<MeshRenderer>().material = greenMaterial;

            }

            //LineMaterial = new Material(Shader.Find("Standard"));
            //LineMaterial.EnableKeyword("_EMISSION");

            //LineStopMaterial = new Material(Shader.Find("Standard"));
            //LineStopMaterial.EnableKeyword("_EMISSION");

            //LineLaneMaterial = new Material(Shader.Find("Standard"));
            //LineStopMaterial.EnableKeyword("_EMISSION");

            if (goWhiteLine == null)
            {
                goWhiteLine = new GameObject("goLine");
                goWhiteLine.AddComponent<MeshFilter>();
                goWhiteLine.AddComponent<MeshRenderer>();
                goWhiteLine.layer = LayerMask.NameToLayer("Element");
            }

            if (goStructure == null)
            {
                goStructure = new GameObject("goStructure");
                goStructure.gameObject.AddComponent<MeshFilter>();
                goStructure.gameObject.AddComponent<MeshRenderer>();
                goStructure.layer = LayerMask.NameToLayer("Element");
            }
            if (goArea == null)
            {
                goArea = new GameObject("goArea");
                goArea.AddComponent<MeshFilter>();
                goArea.AddComponent<MeshRenderer>();
                goArea.layer = LayerMask.NameToLayer("Element");
            }
            if (goPoint == null)
            {
                goPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                goPoint.name = "goPoint";
                pointMaterial = new Material(Shader.Find("Unlit/Color"));
                pointMaterial.SetColor("_Color", new Color(0, 150, 225));
                goPoint.GetComponent<MeshRenderer>().sharedMaterial = pointMaterial;
                float scale = MapManager.Instance.PointDiameter;
                goPoint.transform.localScale = new Vector3(scale, scale, scale);
                goPoint.layer = LayerMask.NameToLayer("Element");
            }
            if (goLaneLet == null)
            {
                goLaneLet = new GameObject("goLaneLet");
                goLaneLet.AddComponent<MeshFilter>();
                goLaneLet.AddComponent<MeshRenderer>();
                goLaneLet.layer = LayerMask.NameToLayer("Element");
            }
            count = Elements.Count;
        }
        /// <summary>
        /// 添加点
        /// </summary>
        /// <param name="point"></param>
        public void AddPoint(Point point)
        {
            points.Add(point);
            point.transform.SetParent(pointParent);
            point.map = this;
            Elements.Add(point.name, point);
        }
        public Point AddPoint(Vector3 pos)
        {
            return AddPoint(CreateElementName(), pos);
        }
        public Point AddPoint(string name, Vector3 pos)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            Point point = Instantiate(goPoint).AddComponent<Point>();
            point.name = name;
            point.transform.position = point.Position = pos;
            AddPoint(point);
            return point;
        }
        /// <summary>
        /// 删除点
        /// </summary>
        /// <param name="point"></param>
        public void RemovePoint(Point point)
        {
            points.Remove(point);
            Elements.Remove(point.name);
        }
        /// <summary>
        /// 修改点坐标
        /// </summary>
        /// <param name="point"></param>
        /// <param name="position"></param>
        public void UpdatePoint(Point point, Vector3 position)
        {

        }
        public void AddLine(Line line)
        {
            Elements.Add(line.name, line);
            line.transform.SetParent(lineParent);
            line.map = this;
        }
        public Line_WhiteLine AddWhiteLine(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            Line_WhiteLine line = Instantiate(goWhiteLine).AddComponent<Line_WhiteLine>();
            line.elemenrMaterial = MapManager.Instance.LineMat;
            line.name = name;
            AddLine(line);
            return line;
        }
        public Line_WhiteLine AddWhiteLine()
        {
            return AddWhiteLine(CreateElementName());
        }
        public Line_StopLine AddStopLine(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            Line_StopLine line = Instantiate(goWhiteLine).AddComponent<Line_StopLine>();
            line.elemenrMaterial = MapManager.Instance.StopLineMat;
            line.name = name;
            AddLine(line);
            return line;
        }
        public Line_StopLine AddStopLine()
        {
            return AddStopLine(CreateElementName());
        }
        public void RemoveLine(Line line)
        {
            Elements.Remove(line.name);
            lines.Remove(line);
        }
        public void AddArea(Area area)
        {
            Elements.Add(area.name, area);
            area.transform.SetParent(AreaParent);
            areas.Add(area);
            area.map = this;
        }
        public void AddLaneLet(Lanelet lanelet)
        {
            Elements.Add(lanelet.name, lanelet);
            lanelet.transform.SetParent(laneLetParent);
            lanelets.Add(lanelet);
            relations.Add(lanelet);
            lanelet.map = this;
        }
        public void AddRegulatoryElement(RegulatoryElement element)
        {
            Elements.Add(element.name, element);
            element.transform.SetParent(RegulatoryParent);
            regulatoryElements.Add(element);
            relations.Add(element);
            element.map = this;
        }
        public Area AddArea(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError(name + " name 重复");
                return null;
            }
            Area area = Instantiate(goArea).AddComponent<Area>();
            area.name = name;
            AddArea(area);
            return area;
        }
        public Lanelet AddLaneLet(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            Lanelet laneLet = Instantiate(goLaneLet).AddComponent<Lanelet>();
            laneLet.name = name;
            laneLet.elemenrMaterial = MapManager.Instance.laneletMat;
            AddLaneLet(laneLet);
            return laneLet;
        }
        public Lanelet AddLaneLet()
        {
            return AddLaneLet(CreateElementName());
        }


        public RegulatoryElement AddRegulatoryElement(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            RegulatoryElement regulatory = Instantiate(goLaneLet).AddComponent<RegulatoryElement>();
            regulatory.name = name;
            AddRegulatoryElement(regulatory);
            return regulatory;
        }
        public void RemoveArea(Area area)
        {
            Elements.Remove(area.name);
            areas.Remove(area);
        }
        public void AddSign(Sign sign)
        {
            Elements.Add(sign.name, sign);
            sign.transform.SetParent(SignParent);
            signs.Add(sign);
            sign.map = this;
        }
        public Sign AddSign(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            Sign sign = new GameObject(name).AddComponent<Sign>();
            AddSign(sign);
            return sign;
        }
        public Sign_TrafficLight AddTrafficLight(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            Sign_TrafficLight sign = Instantiate(goTrafficLight).AddComponent<Sign_TrafficLight>();
            sign.name = name;
            AddSign(sign);
            return sign;
        }
        public Sign_TrafficLight AddTrafficLight()
        {
            return AddTrafficLight(CreateElementName());
        }
        public void RemoveSign(Sign sign)
        {
            Elements.Remove(sign.name);
            signs.Remove(sign);
        }
        public void AddStructrue(Structure structure)
        {
            Elements.Add(structure.name, structure);
            structure.transform.SetParent(StructureParent);
            structures.Add(structure);
            structure.map = this;
        }
        public Structure AddStructrue(string name)
        {
            if (Elements.ContainsKey(name))
            {
                Debug.LogError("name 重复");
                return null;
            }
            Structure structure = Instantiate(goStructure).AddComponent<Structure>();
            structure.name = name;
            AddStructrue(structure);
            return structure;
        }
        public void RemoveStructrue(Structure structure)
        {
            Elements.Remove(structure.name);
            structures.Remove(structure);
        }
        public void UpdateRenderer()
        {
            foreach (KeyValuePair<string, MapElement> item in Elements)
            {
                if (item.Value != null)
                    item.Value.ElementUpdateRenderer();
            }
        }
        public void RemoveElement(MapElement element)
        {
            if (Elements.ContainsKey(element.name))
            {
                Elements.Remove(element.name);
            }
            else
            {
                Debug.LogError("no this element");
            }
            switch (element)
            {
                case Point point:
                    foreach (Line line in lines)
                    {
                        if (line.points.Contains(point))
                        {
                            line.points.Remove(point);
                        }
                    }
                    break;
                case Line_WhiteLine line:
                    foreach (Lanelet lanelet in lanelets)
                    {
                        if (lanelet.IsContain(line))
                        {
                            Destroy(lanelet.gameObject);
                        }
                    }
                    break;
                case Line_StopLine line:
                    break;
                case Sign_TrafficLight trafficLight:
                    break;
                default:
                    break;
            }
        }

        private int count;
        public string CreateElementName()
        {
            count++;
            while (Elements.ContainsKey(count.ToString()))
            {
                count++;
            }
            string name = count.ToString();
            return name;
        }
    }
}