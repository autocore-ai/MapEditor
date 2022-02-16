using UnityEngine;
using UnityEngine.EventSystems;
using assets.OSMReader;
using System.Xml;
using System;

namespace MapRenderer
{
    public class Point : MapElement
    {
        public override void Start()
        {
            base.Start();
            if (!map.points.Contains(this)) map.points.Add(this);
        }
        public Vector3 Position
        {
            get
            {
                return transform.position;
            }
            set
            {
                transform.position = value;
            }
        }

        public override void MoveElement(Vector3 offset)
        {
            Position += offset;
            base.MoveElement(offset);
        }
        public override void UpdateElementData()
        {
            AddOrEditTag("ele", Position.y.ToString());
            AddOrEditTag("local_x", Position.x.ToString());
            AddOrEditTag("local_y", Position.z.ToString());
        }
        public override void OnDestory()
        {
            base.OnDestory();
            foreach (Way way in map.ways)
            {
                if (way.points.Contains(this))
                {
                    way.points.Remove(this);
                    way.ElementEdit();
                }
            }
        }
        public OSMNode GetNode()
        {
            if (node == null)
            {
                node = new OSMNode();
            }
            node.AddOrEditTag("ele", Position.y.ToString());
            node.AddOrEditTag("local_x", Position.x.ToString());
            node.AddOrEditTag("local_y", Position.z.ToString());
            GpsLocation location = MapOrigin.Find().GetGpsLocation(Position);
            node.Latitude = location.Latitude;
            node.Longitude = location.Longitude;
            return node;
        }
        public override XmlElement GetOSMXML(XmlDocument doc)
        {
            UpdateElementData();
            xmlElement = doc.CreateElement("node");
            GpsLocation location = MapOrigin.Find().GetGpsLocation(Position);
            xmlElement.SetAttribute("id", name);
            xmlElement.SetAttribute("visible", "true");
            xmlElement.SetAttribute("version", "1");
            xmlElement.SetAttribute("lat", location.Latitude.ToString());
            xmlElement.SetAttribute("lon", location.Longitude.ToString());
            foreach (OSMTag tag in Tags)
            {
                xmlElement.AppendChild(doc.AddTag(tag.Key, tag.Value));
            }
            return xmlElement;
        }
        public OSMNode node;
    }
}
