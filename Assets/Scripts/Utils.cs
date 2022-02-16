using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

static class Utils
{
    public static XmlElement AddTag(this XmlDocument doc, string key, string value)
    {
        XmlElement tag = doc.CreateElement("tag");
        tag.SetAttribute("k", key);
        tag.SetAttribute("v", value);
        return tag;
    }
    public static XmlElement AddMember(this XmlDocument doc, string type, string @ref, string role)
    {
        XmlElement member = doc.CreateElement("member");
        member.SetAttribute("type", type);
        member.SetAttribute("ref", @ref);
        member.SetAttribute("role", role);
        return member;
    }
}
