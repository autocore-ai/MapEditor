using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

using System.Runtime.InteropServices;

public class OSMTrans : MonoBehaviour
{
    public string path;

    XmlDocument xml = new XmlDocument();


    [DllImport("GeographicWarpper")]
    extern static void UTMUPS_Forward(double lat, double lon, out int zone, out bool northp, out double x, out double y);

    private void Start()
    {
        OSMtransAndSave();
    }

    public void OSMtransAndSave()
    {
        //xml.LoadXml(osm);
        xml.Load(Path.Combine(Application.streamingAssetsPath, path));

        XmlNode OSMNode = xml.SelectSingleNode("osm");
        XmlNodeList nodeXmlList = OSMNode.SelectNodes("node");
        foreach (XmlNode nodeNode in nodeXmlList)
        {
            XmlElement tagLocalX = xml.CreateElement("tag");
            XmlElement tagLocalY = xml.CreateElement("tag");
            double Latitude = double.Parse(nodeNode.Attributes["lat"].Value);
            double Longitude = double.Parse(nodeNode.Attributes["lon"].Value);
            UTMUPS_Forward(Latitude, Longitude, out int zone, out bool northp, out double x, out double y);
            x %= 1e5;
            y %= 1e5;
            string local_x = x.ToString();
            string local_y = y.ToString();
            tagLocalX.SetAttribute("k", "local_x");
            tagLocalX.SetAttribute("v" , local_x);
            tagLocalY.SetAttribute("k", "local_y");
            tagLocalY.SetAttribute("v", local_y);
            nodeNode.AppendChild(tagLocalX);
            nodeNode.AppendChild(tagLocalY);
        }
        xml.Save(Path.Combine(Application.streamingAssetsPath, "osm.xml"));

    }
}
