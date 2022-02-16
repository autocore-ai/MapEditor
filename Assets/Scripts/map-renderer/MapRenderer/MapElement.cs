using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using assets.OSMReader;
using System.Xml;
using System;

namespace MapRenderer
{
    public class MapElement : MonoBehaviour
    {
        public Map map;
        public Material elemenrMaterial;
        public List<OSMTag> Tags = new List<OSMTag>();
        public virtual void Start()
        {
            if (!map.Elements.ContainsKey(name)) 
            {
                map.Elements.Add(name, this);
            }
            gameObject.layer = LayerMask.NameToLayer("Element");

        }
        public virtual void OnSelected()
        {
            var outline = gameObject.GetComponent<Outline>();
            if (outline == null) 
            {
                outline=gameObject.AddComponent<Outline>();
                outline.OutlineColor = Color.green;
            }
            
        }
        public virtual void CancelSelect()
        {
            var outline=gameObject.GetComponent<Outline>();
            if(outline != null) Destroy(outline);
        }
        public virtual void OnDestory()
        {
            Debug.Log(name+"has destory");
            map.RemoveElement(this);
        }

        public virtual void ElementUpdateRenderer()
        {
        }
        public virtual void ElementEdit()
        {
            ElementUpdateRenderer();
        }
        public virtual void MoveElement(Vector3 offset)
        {
            ElementEdit();
        }
        public void RemoveTag(string key)
        {
            for (int i = 0; i < Tags.Count; i++)
            {
                if (key == Tags[i].Key)
                {
                    Tags.Remove(Tags[i]);
                    return;
                }
            }
        }
        public void AddOrEditTag(string key, string value)
        {
            for (int i = 0; i < Tags.Count; i++)
            { 
                if (key == Tags[i].Key)
                {
                    OSMTag tag = Tags[i];
                    tag.Value = value;
                    return;
                }
            }
            Tags.Add(new OSMTag(key, value));
        }
        public virtual void UpdateElementData()
        {

        }
        public static Vector3 Absorb2Ground(Vector3 position)
        {
            Vector3 groundPos = position;
            Ray ray=new Ray(position + new Vector3(0,1f,0), Vector3.down);
            Debug.DrawRay(position + new Vector3(0, 1f, 0),Vector3.down);
            if (Physics.Raycast(ray, out RaycastHit groundHit, 2f, CameraManager.Instance.GroundLayerMask))
            {
                groundPos = groundHit.point;
            }
            else
            {
            }
            return groundPos;
        }
        protected XmlElement xmlElement;
        public virtual XmlElement GetOSMXML(XmlDocument doc)
        {
            return xmlElement;
        }
    }
}
