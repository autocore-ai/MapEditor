using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace assets.OSMReader
{
    public class OSMRelation : OSMBase
    {
        public OSMRelation() { }
        public OSMRelation(XmlNode node)
        {
           Load(node);
        }

        public enum RelationType
        {
            lanelet = 0,
            regulatory_element = 1,
            multipolygon = 2
        }
        public enum RelationSubType
        {
            road = 0,
            traffic_light = 1,
            traffic_sign = 2,
            lane = 3,
            parking_spot,
            parking_access
        }
        public enum TurnDirection
        {
            straight,
            left,
            right
        }
        public RelationType relationType { get; private set; }
        public RelationSubType relationSubType { get; private set; }
        public TurnDirection turn_direction { get; private set; }
        public List<Member> members { get; set; }
        public float speedLimit { get; private set; }


        public override void Load(XmlNode xmlNode)
        {
            ID = GetAttribute<long>("id", xmlNode.Attributes);
            XmlNodeList memberNodes = xmlNode.SelectNodes("member");

            members = new List<Member>();
            foreach (XmlNode n in memberNodes)
            {
                Member member = new Member();
                member.refID = GetAttribute<long>("ref", n.Attributes);
                member.menberType = (Member.MemberType)Enum.Parse(typeof(Member.MemberType), GetAttribute<string>("type", n.Attributes));
                member.roleType = (Member.RoleType)Enum.Parse(typeof(Member.RoleType), GetAttribute<string>("role", n.Attributes));
                this.members.Add(member);
            }

            ReadTags(xmlNode);

            foreach (OSMTag t in Tags)
            {
                switch (t.Key)
                {
                    case "type":
                        relationType = (RelationType)Enum.Parse(typeof(RelationType), t.Value);
                        break;
                    case "subtype":
                        relationSubType = (RelationSubType)Enum.Parse(typeof(RelationSubType), t.Value);
                        break;
                    case "turn_direction":
                        turn_direction = (TurnDirection)Enum.Parse(typeof (TurnDirection), t.Value);
                        break;
                    case "speed_limit":
                        if (t.Value.Contains("km/h"))
                        {
                            speedLimit = float.Parse(t.Value.Replace("km/h", ""));
                        }
                        else speedLimit = 60;
                        break;
                    default:
                        break;
                }
            }
        }
        public override XmlElement Save(XmlDocument doc)
        {
            XmlElement relationElement = doc.CreateElement("relation");
            relationElement.SetAttribute("id", ID.ToString());
            relationElement.SetAttribute("visible", "true");
            relationElement.SetAttribute("version", "1");
            foreach (Member member in members)
            {
                XmlElement memberElement = doc.CreateElement("member");
                memberElement.SetAttribute("type", member.menberType.ToString());
                memberElement.SetAttribute("ref", member.refID.ToString());
                memberElement.SetAttribute("role", member.roleType.ToString());
                relationElement.AppendChild(memberElement);
            }
            foreach (OSMTag tag in Tags)
            {
                XmlElement tagElement = doc.CreateElement("tag");
                tagElement.SetAttribute("k", tag.Key);
                tagElement.SetAttribute("v", tag.Value);
                relationElement.AppendChild(tagElement);
            }
            return relationElement;
        }
    }
}
