using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace assets.OSMReader
{

    public class OSMBase
    {
        public long ID;
        public bool Visible = true;
        public string Version = "1";

        protected T GetAttribute<T>(string attName, XmlAttributeCollection attributes)
        {
            string strValue = attributes[attName].Value;
            return (T)Convert.ChangeType(strValue, typeof(T));
        }

        public void GPS2XY(double I, double B, out double x, out double y)
        {
            try
            {
                I = I * Math.PI / 180;
                B = B * Math.PI / 180;
                double B0 = 30 * Math.PI / 180;
                double N = 0, e = 0, a = 0, b = 0, e2 = 0, K = 0;
                a = 6378137;
                b = 6356752.3142;
                e = Math.Sqrt(1 - (b / a) * (b / a));
                e2 = Math.Sqrt((a / b) * (a / b) - 1);

                double CosB0 = Math.Cos(B0);
                N = (a * a / b) / Math.Sqrt(1 + e2 * e2 * CosB0 * CosB0);
                K = N * CosB0;

                double SinB = Math.Sin(B);
                double tan = Math.Tan(Math.PI / 4 + B / 2);
                double E2 = Math.Pow((1 - e * SinB) / (1 + e * SinB), e / 2);
                double xx = tan * E2;

                x = K * Math.Log(xx);
                y = K * I;

                return;

            }
            catch (Exception ErrInfo)
            {
            }
            x = -1;
            y = -1;
        }

        public List<OSMTag> Tags = new List<OSMTag>();
        protected void ReadTags(XmlNode node)
        {
            XmlNodeList tags = node.SelectNodes("tag");
            foreach (XmlNode t in tags)
            {
                OSMTag tag = new OSMTag(GetAttribute<string>("k", t.Attributes), GetAttribute<string>("v", t.Attributes));
                Tags.Add(tag);
            }
        }

        public virtual void Load(XmlNode xmlNode) { }

        public virtual XmlElement Save(XmlDocument doc) { return null; }

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
    }
    public struct OSMTag
    {
        public OSMTag(string key, string value) 
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }

}
