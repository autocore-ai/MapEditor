using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace assets.OSMReader
{
    public class OSMNode : OSMBase
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public float ele;
        public float local_x;
        public float local_y;

        public static implicit operator Vector3(OSMNode node)
        {
            return new Vector3(node.local_x, node.ele, node.local_y);
        }
        public OSMNode() { }

        //[DllImport("GeographicWarpper")]
        //extern static void UTMUPS_Forward(double lat, double lon, out int zone, out bool northp, out double x, out double y);
        public OSMNode(XmlNode node)
        {
            Load(node);
        }

        public override void Load(XmlNode xmlNode)
        {
            ID = GetAttribute<long>("id", xmlNode.Attributes);
            Latitude = GetAttribute<float>("lat", xmlNode.Attributes);
            Longitude = GetAttribute<float>("lon", xmlNode.Attributes);
            ReadTags(xmlNode);

            foreach (OSMTag tag in Tags)
            {
                if (tag.Key == "local_x")
                {
                    local_x = float.Parse(tag.Value);
                }
                else if (tag.Key == "ele")
                {
                    ele = float.Parse(tag.Value);
                }
                else if (tag.Key == "local_y")
                {
                    local_y = float.Parse(tag.Value);
                }
            }
        }
        public override XmlElement Save(XmlDocument doc)
        {
            XmlElement node = doc.CreateElement("node");
            node.SetAttribute("id", ID.ToString());
            node.SetAttribute("visible", "true");
            node.SetAttribute("version", "1");
            node.SetAttribute("lat", Latitude.ToString());
            node.SetAttribute("lon", Longitude.ToString());
            foreach (OSMTag tag in Tags)
            {
                XmlElement tagElement = doc.CreateElement("tag");
                tagElement.SetAttribute("k", tag.Key);
                tagElement.SetAttribute("v", tag.Value);
                node.AppendChild(tagElement);
            }
            return node;
        }
        public Vector3 GetPosition()
        {
            foreach (OSMTag tag in Tags)
            {
                if (tag.Key == "local_x")
                {
                    return new Vector3(local_x, ele, local_y);
                }
            }
            return OSMManager.Instance.mapOrigin.FromGpsLocation(Latitude,Longitude);

            //return new Vector3(Latitude, ele, Longitude);
        }
    }
}
