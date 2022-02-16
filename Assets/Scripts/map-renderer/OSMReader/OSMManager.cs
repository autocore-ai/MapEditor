using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using MapRenderer;
using System.Xml;
using System;

namespace assets.OSMReader
{

    public class OSMManager : MonoBehaviour
    {
        public MapManager mapManager;
        public static OSMManager Instance;
        public bool isLongitude = false;
        public MapOrigin mapOrigin;

        OSMData oSMData;
        public void Start()
        {
            mapManager = GetComponent<MapManager>();
            MapManager.Instance.OnGetOSM += ReadOSMWithStr;
        }
        public Map map;
        private void Awake()
        {
            Instance = this;
        }
        private void Update()
        {

            //if (Input.GetKeyDown(KeyCode.M))
            //{
            //    CreateOSMFile(mapManager.currentMap);
            //}
        }

        public void BuildMap(OSMData data)
        {
            if (data == null)
            {
                Debug.LogError("data is null");
            }
            map = mapManager.GetOrCreateMap(data.name);
            foreach (OSMNode node in data.nodes)
            {
                Point nodeTemp = map.AddPoint(node.ID.ToString(), node.GetPosition()) ;
                nodeTemp.Tags = node.Tags;
            }
            foreach (OSMWay way in data.ways) 
            {
                List<Point> points = new List<Point>();
                foreach (int node in way.NodeIDs)
                {
                    if (map.Elements.TryGetValue(node.ToString(), out MapElement element))
                    {
                        if (element is Point point)
                        {
                            points.Add(point);
                        }
                        else
                        {
                            Debug.LogError("map error");
                        }
                    }
                }
                //if (way.IsBoundary)
                //{
                //    Area area= new GameObject(way.ID.ToString()).AddComponent<Area>();
                //    area.points = points;
                //    area.areaColor = Color.green;
                //    map.AddArea(area);
                //    continue;
                //}


                switch (way.OSMWayType)
                {
                    case OSMWay.WayType.line_thin:
                        Line line = map.AddWhiteLine(way.ID.ToString());
                        line.Tags = way.Tags;
                        line.points = points;
                        line.color = Color.white;
                        line.color.a = 0.3f;
                        break;
                    case OSMWay.WayType.stop_line:
                        Line_StopLine line_stop = map.AddStopLine(way.ID.ToString());
                        line_stop.Tags = way.Tags;
                        line_stop.points = points;
                        line_stop.color = Color.red;
                        break;
                    case OSMWay.WayType.traffic_light:
                        Sign_TrafficLight sign_traffic = map.AddTrafficLight(way.ID.ToString());
                        if (points.Count != 2)
                        {
                            Debug.LogError("count error");
                        }
                        sign_traffic.Tags = way.Tags;
                        sign_traffic.points= points;
                        break;
                    case OSMWay.WayType.traffic_sign:
                        //Sign sign = new GameObject(way.ID.ToString()).AddComponent<Sign>();
                        //if (points.Count != 2)
                        //{
                        //    Debug.LogError("count error");
                        //}
                        //Vector3 pos1 = points[0].position/2+points[1].position/2;
                        //sign.position = pos1;
                        //float distance =Vector3.Distance( points[0].position, points[1].position);
                        //sign.width = sign.height = distance;
                        //map.AddSign(sign);
                        break;
                    case OSMWay.WayType.area:
                        if (way.OSMSubType == OSMWay.WaySubType.parking_spot)
                        {
                            Area area = map.AddArea(way.ID.ToString());
                            area.Tags = way.Tags;
                            area.points = points;
                            area.color = Color.yellow;

                            //Line areLine = map.AddLine(way.ID.ToString() + "border");
                            //areLine.points = points;
                            //areLine.color = Color.white;
                            //areLine.color.a = 0.4f;
                        }
                        else if (way.OSMSubType == OSMWay.WaySubType.parking_access)
                        {
                            Area area = map.AddArea(way.ID.ToString());
                            area.points = points;
                            area.color = Color.green;

                            //Line areLine = map.AddLine(way.ID.ToString()+"border");
                            //areLine.points = points;
                            //areLine.color = Color.white;
                            //areLine.color.a = 0.4f;
                        }
                        break;
                    case OSMWay.WayType.building:
                        Structure structure = map.AddStructrue(way.ID.ToString());
                        structure.Tags = way.Tags;
                        structure.points = points;
                        structure.height = way.Height;
                        structure.color = new Color(0, 1, 0, 0.2f);
                        break;
                    default:
                        break;
                }
            }
            foreach (OSMRelation relation in data.relations)
            {
                switch (relation.relationType)
                {
                    case OSMRelation.RelationType.lanelet:
                        if (relation.relationSubType == OSMRelation.RelationSubType.lane||relation.relationSubType==OSMRelation.RelationSubType.road)
                        {
                            Lanelet area_Lane = map.AddLaneLet(relation.ID.ToString());
                            area_Lane.Tags = relation.Tags;
                            area_Lane.members = relation.members;
                            foreach (Member member in relation.members)
                            {
                                if (member.roleType == Member.RoleType.left)
                                {
                                    if (map.Elements.TryGetValue(member.refID.ToString(),out MapElement mapElement))
                                    {
                                        if(mapElement is Line lineL)
                                        {
                                            area_Lane.leftWay = lineL;
                                        }
                                        else
                                        {
                                            Debug.Log(member.refID.ToString() + "not fond");
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString()+"not fond");
                                    }
                                }
                                else if (member.roleType == Member.RoleType.right)
                                {
                                    if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                                    {
                                        if (mapElement is Line lineR)
                                        {
                                            area_Lane.rightWay = lineR;
                                        }
                                        else
                                        {
                                            Debug.Log(member.refID.ToString() + "not fond");
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString() + "not fond");
                                    }
                                }
                            }
                            switch (relation.turn_direction)
                            {
                                case OSMRelation.TurnDirection.left:
                                    area_Lane.turnDirection = Lanelet.TurnDirection.Left;
                                    break;
                                case OSMRelation.TurnDirection.right:
                                    area_Lane.turnDirection = Lanelet.TurnDirection.Right;
                                    break;
                                case OSMRelation.TurnDirection.straight:
                                    area_Lane.turnDirection = Lanelet.TurnDirection.Straight;
                                    break;
                            }

                            area_Lane.color = Color.green;
                            area_Lane.color.a = 0.2f;
                        }
                        break;
                    case OSMRelation.RelationType.regulatory_element:
                        RegulatoryElement element = map.AddRegulatoryElement(relation.ID.ToString());
                        switch (relation.relationSubType)
                        {
                            case OSMRelation.RelationSubType.traffic_light:
                                element.subType = RegulatoryElement.SubType.traffic_light;
                                break;
                            case OSMRelation.RelationSubType.traffic_sign:
                                element.subType = RegulatoryElement.SubType.traffic_sign;
                                break;
                            case OSMRelation.RelationSubType.road:
                                element.subType = RegulatoryElement.SubType.road;
                                break;
                            default:
                                Debug.Log(relation.relationSubType.ToString()+"is not support");
                                break;
                        }

                        foreach (Member member in relation.members)
                        {
                            if (member.roleType == Member.RoleType.refers)
                            {
                                if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                                {
                                    if (mapElement is Way way)
                                    {
                                        element.refers = way;
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString() + "not fond");
                                    }
                                }
                                else
                                {
                                    Debug.Log(member.refID.ToString() + "not fond");
                                }
                            }
                            else if (member.roleType == Member.RoleType.ref_line)
                            {
                                if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                                {
                                    if (mapElement is Way way)
                                    {
                                        element.ref_line = way;
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString() + "not fond");
                                    }
                                }
                                else
                                {
                                    Debug.Log(member.refID.ToString() + "not fond");
                                }
                            }
                        }

                        break;
                    case OSMRelation.RelationType.multipolygon:
                        //if (relation.relationSubType == OSMRelation.RelationSubType.parking_spot)
                        //{
                        //    Area area = map.AddArea(relation.ID.ToString() + "r");
                        //    foreach (Member member in relation.menbers)
                        //    {
                        //        if (member.roleType == Member.RoleType.outer)
                        //        {
                        //            if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                        //            {
                        //                if (mapElement is Line a)
                        //                {
                        //                    area.points.AddRange(a.points);
                        //                }
                        //                else
                        //                {
                        //                    Debug.Log(member.refID.ToString() + "not fond");
                        //                }
                        //            }
                        //            else
                        //            {
                        //                Debug.Log(member.refID.ToString() + "not fond");
                        //            }
                        //        }
                        //    }
                        //    area.color = Color.green;
                        //    area.color.a = 0.2f;
                        //}
                        break;
                    default:
                        Debug.LogWarning("Unsupported Relation type");
                        break;
                }

                if (relation.relationSubType == OSMRelation.RelationSubType.traffic_light)
                {
                    Line_StopLine lineStopTemp = null;
                    Sign_TrafficLight trafficTemp = null;
                    foreach (Member member in relation.members)
                    {
                        if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement element))
                        {
                            if (member.roleType == Member.RoleType.ref_line && element is Line_StopLine lineStop)
                            {
                                lineStopTemp = lineStop;
                            }
                            else if (member.roleType == Member.RoleType.refers && element is Sign_TrafficLight trafficLight)
                            {
                                trafficTemp = element.GetComponent<Sign_TrafficLight>();
                            }
                        }
                    }
                    if (lineStopTemp != null && trafficTemp != null)
                    {
                        lineStopTemp.ControlSign = trafficTemp;
                    }
                }
            }
            map.UpdateRenderer();
        }
        IEnumerator StartBuildMap(OSMData data)
        {
            if (data == null)
            {
                Debug.LogError("data is null");
            }
            map = mapManager.GetOrCreateMap(data.name);
            foreach (OSMNode node in data.nodes)
            {
                Point nodeTemp = map.AddPoint(node.ID.ToString(), node.GetPosition());
                nodeTemp.Tags = node.Tags;
                yield return 0;
            }
            foreach (OSMWay way in data.ways)
            {
                List<Point> points = new List<Point>();
                foreach (int node in way.NodeIDs)
                {
                    if (map.Elements.TryGetValue(node.ToString(), out MapElement element))
                    {
                        if (element is Point point)
                        {
                            points.Add(point);
                        }
                        else
                        {
                            Debug.LogError("map error");
                        }
                    }
                }
                //if (way.IsBoundary)
                //{
                //    Area area= new GameObject(way.ID.ToString()).AddComponent<Area>();
                //    area.points = points;
                //    area.areaColor = Color.green;
                //    map.AddArea(area);
                //    continue;
                //}


                switch (way.OSMWayType)
                {
                    case OSMWay.WayType.line_thin:
                        Line line = map.AddWhiteLine(way.ID.ToString());
                        line.Tags = way.Tags;
                        line.points = points;
                        line.color = Color.white;
                        line.color.a = 0.3f;

                        yield return 0;
                        break;
                    case OSMWay.WayType.stop_line:
                        Line_StopLine line_stop = map.AddStopLine(way.ID.ToString());
                        line_stop.Tags = way.Tags;
                        line_stop.points = points;
                        line_stop.color = Color.red;

                        yield return 0;
                        break;
                    case OSMWay.WayType.traffic_light:
                        Sign_TrafficLight sign_traffic = map.AddTrafficLight(way.ID.ToString());
                        if (points.Count != 2)
                        {
                            Debug.LogError("count error");
                        }
                        sign_traffic.Tags = way.Tags;
                        sign_traffic.points = points;

                        yield return 0;
                        break;
                    case OSMWay.WayType.traffic_sign:
                        //Sign sign = new GameObject(way.ID.ToString()).AddComponent<Sign>();
                        //if (points.Count != 2)
                        //{
                        //    Debug.LogError("count error");
                        //}
                        //Vector3 pos1 = points[0].position/2+points[1].position/2;
                        //sign.position = pos1;
                        //float distance =Vector3.Distance( points[0].position, points[1].position);
                        //sign.width = sign.height = distance;
                        //map.AddSign(sign);
                        break;
                    case OSMWay.WayType.area:
                        if (way.OSMSubType == OSMWay.WaySubType.parking_spot)
                        {
                            Area area = map.AddArea(way.ID.ToString());
                            area.Tags = way.Tags;
                            area.points = points;
                            area.color = Color.yellow;

                            yield return 0;
                        }
                        else if (way.OSMSubType == OSMWay.WaySubType.parking_access)
                        {
                            Area area = map.AddArea(way.ID.ToString());
                            area.points = points;
                            area.color = Color.green;

                            yield return 0;
                        }
                        break;
                    case OSMWay.WayType.building:
                        Structure structure = map.AddStructrue(way.ID.ToString());
                        structure.Tags = way.Tags;
                        structure.points = points;
                        structure.height = way.Height;
                        structure.color = new Color(0, 1, 0, 0.2f);
                        yield return 0;
                        break;
                    default:
                        break;
                }
            }
            foreach (OSMRelation relation in data.relations)
            {
                switch (relation.relationType)
                {
                    case OSMRelation.RelationType.lanelet:
                        if (relation.relationSubType == OSMRelation.RelationSubType.lane || relation.relationSubType == OSMRelation.RelationSubType.road)
                        {
                            Lanelet area_Lane = map.AddLaneLet(relation.ID.ToString());
                            area_Lane.Tags = relation.Tags;
                            area_Lane.members = relation.members;
                            foreach (Member member in relation.members)
                            {
                                if (member.roleType == Member.RoleType.left)
                                {
                                    if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                                    {
                                        if (mapElement is Line lineL)
                                        {
                                            area_Lane.leftWay = lineL;
                                        }
                                        else
                                        {
                                            Debug.Log(member.refID.ToString() + "not fond");
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString() + "not fond");
                                    }
                                }
                                else if (member.roleType == Member.RoleType.right)
                                {
                                    if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                                    {
                                        if (mapElement is Line lineR)
                                        {
                                            area_Lane.rightWay = lineR;
                                        }
                                        else
                                        {
                                            Debug.Log(member.refID.ToString() + "not fond");
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString() + "not fond");
                                    }
                                }
                            }
                            switch (relation.turn_direction)
                            {
                                case OSMRelation.TurnDirection.left:
                                    area_Lane.turnDirection = Lanelet.TurnDirection.Left;
                                    break;
                                case OSMRelation.TurnDirection.right:
                                    area_Lane.turnDirection = Lanelet.TurnDirection.Right;
                                    break;
                                case OSMRelation.TurnDirection.straight:
                                    area_Lane.turnDirection = Lanelet.TurnDirection.Straight;
                                    break;
                            }

                            yield return 0;
                        }
                        break;
                    case OSMRelation.RelationType.regulatory_element:
                        RegulatoryElement element = map.AddRegulatoryElement(relation.ID.ToString());
                        switch (relation.relationSubType)
                        {
                            case OSMRelation.RelationSubType.traffic_light:
                                element.subType = RegulatoryElement.SubType.traffic_light;
                                break;
                            case OSMRelation.RelationSubType.traffic_sign:
                                element.subType = RegulatoryElement.SubType.traffic_sign;
                                break;
                            case OSMRelation.RelationSubType.road:
                                element.subType = RegulatoryElement.SubType.road;
                                break;
                            default:
                                Debug.Log(relation.relationSubType.ToString() + "is not support");
                                break;
                        }

                        foreach (Member member in relation.members)
                        {
                            if (member.roleType == Member.RoleType.refers)
                            {
                                if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                                {
                                    if (mapElement is Way way)
                                    {
                                        element.refers = way;
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString() + "not fond");
                                    }
                                }
                                else
                                {
                                    Debug.Log(member.refID.ToString() + "not fond");
                                }
                            }
                            else if (member.roleType == Member.RoleType.ref_line)
                            {
                                if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                                {
                                    if (mapElement is Way way)
                                    {
                                        element.ref_line = way;
                                    }
                                    else
                                    {
                                        Debug.Log(member.refID.ToString() + "not fond");
                                    }
                                }
                                else
                                {
                                    Debug.Log(member.refID.ToString() + "not fond");
                                }
                            }
                        }

                        break;
                    //case OSMRelation.RelationType.multipolygon:
                    //if (relation.relationSubType == OSMRelation.RelationSubType.parking_spot)
                    //{
                    //    Area area = map.AddArea(relation.ID.ToString()+"r");
                    //    foreach (Member member in relation.menbers)
                    //    {
                    //        if (member.roleType == Member.RoleType.outer)
                    //        {
                    //            if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement mapElement))
                    //            {
                    //                if (mapElement is Line a)
                    //                {
                    //                    area.points.AddRange(a.points);
                    //                }
                    //                else
                    //                {
                    //                    Debug.Log(member.refID.ToString() + "not fond");
                    //                }
                    //            }
                    //            else
                    //            {
                    //                Debug.Log(member.refID.ToString() + "not fond");
                    //            }
                    //        }
                    //    }
                    //    area.color = Color.green;
                    //    area.color.a = 0.2f;
                    //}
                    //break;
                    default:
                        Debug.LogWarning("Unsupported Relation type");
                        break;
                }

                if (relation.relationSubType == OSMRelation.RelationSubType.traffic_light)
                {
                    Line_StopLine lineStopTemp = null;
                    Sign_TrafficLight trafficTemp = null;
                    foreach (Member member in relation.members)
                    {
                        if (map.Elements.TryGetValue(member.refID.ToString(), out MapElement element))
                        {
                            if (member.roleType == Member.RoleType.ref_line && element is Line_StopLine lineStop)
                            {
                                lineStopTemp = lineStop;
                            }
                            else if (member.roleType == Member.RoleType.refers && element is Sign_TrafficLight trafficLight)
                            {
                                trafficTemp = element.GetComponent<Sign_TrafficLight>();
                            }
                        }
                    }
                    if (lineStopTemp != null && trafficTemp != null)
                    {
                        lineStopTemp.ControlSign = trafficTemp;
                    }
                }
            }
            map.UpdateRenderer();
        }
        public void ReadOSMWithStr(string xmlStr)
        {
            mapOrigin = MapOrigin.Find();
            xmlStr = System.Text.RegularExpressions.Regex.Replace(xmlStr, "^[^<]", "");
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlStr);
            oSMData = ReadOSMWithXmlDoc(xml);
            BuildMap(oSMData);
        }
        public OSMData ReadOSMWithXmlDoc(XmlDocument xmlDoc)
        {
            OSMData data = new OSMData();
            XmlNode OSMNode = xmlDoc.SelectSingleNode("osm");
            data.name = xmlDoc.Name;
            XmlNodeList nodeXmlList = OSMNode.SelectNodes("node");
            foreach (XmlNode nodeNode in nodeXmlList)
            {
                data.nodes.Add(new OSMNode(nodeNode));
            }
            if (data.nodes.Count > 1)
            {
                OSMNode originNode = data.nodes[0];
                double latitude, longitude;
                longitude = originNode.Longitude;
                latitude = originNode.Latitude;
                int zoneNumber = mapOrigin.GetZoneNumberFromLatLon(latitude, longitude);
                mapOrigin.UTMZoneId = zoneNumber;
                double northing, easting;
                mapOrigin.FromLatitudeLongitude(latitude, longitude, out northing, out easting);

                mapOrigin.OriginNorthing = northing;
                mapOrigin.OriginEasting = easting;
            }
            XmlNodeList wayXmlList = OSMNode.SelectNodes("way");
            foreach (XmlNode wayNode in wayXmlList)
            {
                data.ways.Add(new OSMWay(wayNode));
            }
            XmlNodeList relationXmlList = OSMNode.SelectNodes("relation");
            foreach (XmlNode relationNode in relationXmlList)
            {
                data.relations.Add(new OSMRelation(relationNode));
            }

            return data;
        }
        Vector3 GetVector3FromNode(OSMNode node)
        {
            double lat = (double)node.Latitude;
            double lon = (double)node.Longitude;
            double northing, easting;

            mapOrigin.FromLatitudeLongitude(lat, lon, out northing, out easting);
            Vector3 positionVec = mapOrigin.FromNorthingEasting(northing, easting); // note here y=0 in vec

            if (node.Tags?.Count > 0)
            {
                foreach (var item in node.Tags)
                {
                    if (item.Key == "ele")
                    {
                        var y = float.Parse(item.Value);
                        positionVec.y = y;
                    }
                }
            }

            return positionVec;
        }
        public void SaveMapXMLToPath(Map map,string path)
        {
            XmlDocument xml = new XmlDocument();
            XmlDeclaration xmlDecl = xml.CreateXmlDeclaration("1.0", "UTF-8", "");//设置xml文件编码格式为UTF-8
            xml.AppendChild(xmlDecl);
            XmlElement rootElement = xml.CreateElement("osm");//创建根节点
            rootElement.SetAttribute("generator", "Autoware Map Toolbox");
            rootElement.SetAttribute("version", "0.2.0-preview.2");
            xml.AppendChild(rootElement);
            foreach (Point point in map.points)
            {
                rootElement.AppendChild(point.GetOSMXML(xml));
                //point.UpdateElementData();
                //if (mapOrigin == null) mapOrigin = MapOrigin.Find();
                //GpsLocation gpsLocation = mapOrigin.GetGpsLocation(point.Position);
                //XmlElement nodeElement = xml.CreateElement("node");
                //rootElement.AppendChild(nodeElement);
                //nodeElement.SetAttribute("id", point.name);
                //nodeElement.SetAttribute("visible", "true");
                //nodeElement.SetAttribute("version", "1");
                //nodeElement.SetAttribute("lat", gpsLocation.Latitude.ToString());
                //nodeElement.SetAttribute("lon", gpsLocation.Longitude.ToString());
                //foreach (OSMTag tag in point.Tags)
                //{
                //    XmlElement tagElement = xml.CreateElement("tag");
                //    tagElement.SetAttribute("k", tag.Key);
                //    tagElement.SetAttribute("v", tag.Value);
                //    nodeElement.AppendChild(tagElement);
                //}

            }
            foreach (Way way in map.ways)
            {
                rootElement.AppendChild(way.GetOSMXML(xml));

                //XmlElement lineElement = xml.CreateElement("way");
                //rootElement.AppendChild(lineElement);
                //lineElement.SetAttribute("id", way.name);
                //lineElement.SetAttribute("visible", "true");
                //foreach (Point node in way.points)
                //{
                //    XmlElement ndElement = xml.CreateElement("nd");
                //    ndElement.SetAttribute("ref",node.name);
                //    lineElement.AppendChild(ndElement);
                //}
                //foreach (OSMTag tag in way.Tags)
                //{
                //    XmlElement tagElement = xml.CreateElement("tag");
                //    tagElement.SetAttribute("k", tag.Key);
                //    tagElement.SetAttribute("v", tag.Value);
                //    lineElement.AppendChild(tagElement);
                //}
            }
            foreach (Way way in map.ways)
            {
                if(way is Sign sign)
                {
                    if (sign.targetWay != null)
                    {
                        XmlElement relationElement = xml.CreateElement("relation");
                        rootElement.AppendChild(relationElement); 
                        relationElement.SetAttribute("id", sign.name);
                        relationElement.SetAttribute("visible", "true");
                        relationElement.SetAttribute("version", "1");
                        XmlElement RefLinemember = xml.CreateElement("member");
                        RefLinemember.SetAttribute("type", "way");
                        RefLinemember.SetAttribute("ref", sign.targetWay.name);
                        RefLinemember.SetAttribute("role", "ref_line");
                        relationElement.AppendChild(RefLinemember);
                        XmlElement RefersLinemember = xml.CreateElement("member");
                        RefersLinemember.SetAttribute("type", "way");
                        RefersLinemember.SetAttribute("ref", sign.name);
                        RefersLinemember.SetAttribute("role", "refers");
                        relationElement.AppendChild(RefersLinemember);
                        XmlElement tagElement1 = xml.CreateElement("tag");
                        tagElement1.SetAttribute("k", "subtype");
                        tagElement1.SetAttribute("v", "regulatory_element");
                        relationElement.AppendChild(tagElement1);
                        XmlElement tagElement2 = xml.CreateElement("tag");
                        tagElement2.SetAttribute("k", "type");
                        tagElement2.SetAttribute("v", "traffic_light");
                        relationElement.AppendChild(tagElement2);
                    }
                }
            }
            foreach (Relation relation in map.relations)
            {
                rootElement.AppendChild (relation.GetOSMXML(xml));
                //XmlElement relationElement = xml.CreateElement("relation");
                //rootElement.AppendChild (relationElement);
                //relationElement.SetAttribute("id", relation.name);
                //relationElement.SetAttribute("visible", "true");
                //relationElement.SetAttribute("version", "1");
                //foreach (Member member in relation.members)
                //{
                //    XmlElement memberElement = xml.CreateElement("member");
                //    memberElement.SetAttribute("type", member.menberType.ToString());
                //    memberElement.SetAttribute("ref", member.refID.ToString());
                //    memberElement.SetAttribute("role", member.roleType.ToString());
                //    relationElement.AppendChild(memberElement);
                //}
                //foreach (OSMTag tag in relation.Tags)
                //{
                //    XmlElement tagElement = xml.CreateElement("tag");
                //    tagElement.SetAttribute("k", tag.Key);
                //    tagElement.SetAttribute("v", tag.Value);
                //    relationElement.AppendChild(tagElement);
                //}
            }
            xml.Save(path);
        }
    }
}
