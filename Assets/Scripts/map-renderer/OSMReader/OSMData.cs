using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using System.Runtime.InteropServices;
namespace assets.OSMReader
{
    public class Member
    {
        public enum MemberType
        {
            node,
            way,
            relation
        }
        public enum RoleType
        {
            left,
            right,
            regulatory_element,
            refers,
            ref_line,
            outer
        }
        public MemberType menberType;
        public long refID;
        public RoleType roleType;
    }

    public class OSMData
    {
        public string name;
        public List<OSMNode> nodes;
        public List<OSMWay> ways;
        public List<OSMRelation> relations;
        public OSMData()
        {
            nodes = new List<OSMNode>();
            ways = new List<OSMWay>();
            relations = new List<OSMRelation>();
        }
    }
}