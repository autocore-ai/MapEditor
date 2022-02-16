using MapRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RegulatoryElement : Relation
{
    public Way ref_line;
    public Way refers;

    public enum SubType
    {
        road,
        traffic_light,
        traffic_sign
    }
    public SubType subType;

    public override void OnDestory()
    {
        if (map.regulatoryElements.Contains(this)) map.regulatoryElements.Remove(this);
        base.OnDestory();
    }
    public override void Start()
    {
        base.Start();
        if (!map.regulatoryElements.Contains(this)) map.regulatoryElements.Add(this);
    }
}
