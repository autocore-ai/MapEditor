using assets.OSMReader;
using Packages.BezierCurveEditorPackage.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
namespace MapRenderer
{
    public class Way : MapElement
    {
        public override void Start()
        {
            base.Start();
            if (!map.ways.Contains(this)) map.ways.Add(this);
        }
        public List<Point> points = new List<Point>();

        public override void OnDestory()
        {
            base.OnDestory();
            map.ways.Remove(this);
            foreach (Lanelet item in map.lanelets)
            {
                item.RemoveElement(this);
            }
            base.OnDestory();
        }

        public override void ElementUpdateRenderer()
        {
            base.ElementUpdateRenderer();
        }
        public override void UpdateElementData()
        {
            base.UpdateElementData();
        }
        public OSMWay GetWay()
        {
            if (way == null) way = new OSMWay();
            foreach (var item in points)
            {

            }
            return way;
        }
        public override XmlElement GetOSMXML(XmlDocument doc)
        {
            UpdateElementData();
            XmlElement wayXML = doc.CreateElement("way");
            wayXML.SetAttribute("id", name);
            wayXML.SetAttribute("visible", "true");
            wayXML.SetAttribute("version", "1");
            foreach (Point point in points)
            {
                XmlElement nd = doc.CreateElement("nd");
                nd.SetAttribute("ref", point.name);
                wayXML.AppendChild(nd);
            }

            foreach (OSMTag tag in Tags)
            {
                wayXML.AppendChild(doc.AddTag(tag.Key, tag.Value));
            }
            return wayXML;
        }
        public OSMWay way;

    }
}
