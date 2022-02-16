using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using assets.OSMReader;
using System.Xml;

namespace MapRenderer
{
    public class Relation : MapElement
    {
        public List<Member> members=new List<Member>();

        public bool IsContain(MapElement element)
        {
            for (int i = 0; i <members.Count; i++)
            {
                if (members[i].refID.ToString() == name) return true;
            }
            return false;
        }

        public override void Start()
        {
            base.Start();
            if (!map.relations.Contains(this)) map.relations.Add(this);
        }
        public override void OnDestory()
        {
            if (map.relations.Contains(this)) map.relations.Remove(this);
            base.OnDestory();
        }

        public override XmlElement GetOSMXML(XmlDocument doc)
        {
            UpdateElementData();
            XmlElement relationXML = doc.CreateElement("relation");
            relationXML.SetAttribute("id", name);
            relationXML.SetAttribute("visible", "true");
            relationXML.SetAttribute("version", "1");
            var lanelet = GetComponent<Lanelet>();
            if (lanelet != null)
            {
                relationXML.AppendChild(doc.AddMember("way", lanelet.leftWay.name, "left"));
                relationXML.AppendChild(doc.AddMember("way", lanelet.rightWay.name, "right"));
                foreach (var item in members)
                {
                    if (item.roleType == Member.RoleType.left || item.roleType == Member.RoleType.right)
                    {
                        continue;
                    }
                    var element = map.GetElementByName(item.refID.ToString());
                    if (element is Relation)
                    {
                        if (element.GetComponent<RegulatoryElement>())
                        {
                            relationXML.AppendChild(doc.AddMember("relation", item.refID.ToString(), "regulatory_element"));
                        }
                    }
                }
            }
            else
            {
                var regulatory_element = GetComponent<RegulatoryElement>();
                if (regulatory_element)
                {
                    relationXML.AppendChild(doc.AddTag("type", "regulatory_element"));
                    relationXML.AppendChild(doc.AddTag("subtype", regulatory_element.subType.ToString()));
                    relationXML.AppendChild(doc.AddMember("way", regulatory_element.refers.name, "refers"));
                    relationXML.AppendChild(doc.AddMember("way", regulatory_element.ref_line.name, "ref_line"));
                }
            }
            foreach (OSMTag tag in Tags)
            {
                relationXML.AppendChild(doc.AddTag(tag.Key, tag.Value));
            }
            return relationXML;
        }
    }

}
