using System.Collections.Generic;
using UnityEngine;

namespace MapRenderer
{
    public class Structure : MapElement
    {
        public List<Point> points;
        public float height;
        public Vector3 position;
        public Vector3 size;
        public float yaw;
        public Color color =new Color(0,0,0,0.5f);
        private MeshFilter mf;
        private MeshRenderer mr;
        public override void Start()
        {
            base.Start();
            if(!map.structures.Contains(this))map.structures.Add(this);
        }
        public override void OnDestory()
        {
            base.OnDestory();
            if (map.structures.Contains(this)) map.structures.Remove(this);
        }

        public override void ElementUpdateRenderer()
        {
            base.ElementUpdateRenderer();
            elemenrMaterial = new Material(Shader.Find("Standard"));
            elemenrMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            elemenrMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            elemenrMaterial.SetInt("_ZWrite", 0);
            elemenrMaterial.DisableKeyword("_ALPHATEST_ON");
            elemenrMaterial.EnableKeyword("_ALPHABLEND_ON");
            elemenrMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            elemenrMaterial.renderQueue = 3000;
            elemenrMaterial.EnableKeyword("_EMISSION");
            color.a = 0.5f;
            elemenrMaterial.color = color;

            elemenrMaterial.SetColor("_EmissionColor", color);
            mf = GetComponent<MeshFilter>();
            mr = GetComponent<MeshRenderer>();

            mr.material = elemenrMaterial;

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int i = 1; i < points.Count; i++)
            {
                Vector3 p1 = points[i - 1].Position;
                Vector3 p2 = points[i].Position;

                Vector3 v1 = p1;
                Vector3 v2 = p2;
                Vector3 v3 = v1 + new Vector3(0, height, 0);
                Vector3 v4 = v2 + new Vector3(0, height, 0);

                vectors.Add(v1);
                vectors.Add(v2);
                vectors.Add(v3);
                vectors.Add(v4);

                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);

                // index values
                int idx1, idx2, idx3, idx4;
                idx4 = vectors.Count - 1;
                idx3 = vectors.Count - 2;
                idx2 = vectors.Count - 3;
                idx1 = vectors.Count - 4;

                // first triangle v1, v3, v2
                indices.Add(idx1);
                indices.Add(idx3);
                indices.Add(idx2);

                // second triangle v3, v4, v2
                indices.Add(idx3);
                indices.Add(idx4);
                indices.Add(idx2);

                // third triangle v2, v3, v1
                indices.Add(idx2);
                indices.Add(idx3);
                indices.Add(idx1);

                // fourth triangle v2, v4, v3
                indices.Add(idx2);
                indices.Add(idx4);
                indices.Add(idx3);
            }
            for (int i = 1; i < points.Count; i++)
            {
                Vector3 pt = points[i].Position + new Vector3(0, height, 0);
                vectors.Add(pt);
                normals.Add(-Vector3.forward);
            }

            //得到三角形的数量
            int trianglesCount = points.Count - 3;

            for (int i = 0; i < trianglesCount; i++)
            {
                for (int j = 3; j > 0; --j)
                {
                    indices.Add(vectors.Count - 1 - (j == 3 ? 0 : i + j));
                }
            }

            for (int i = 1; i < points.Count; i++)
            {
                Vector3 pt = points[i].Position;
                vectors.Add(pt);
                normals.Add(-Vector3.forward);
            }

            for (int i = 0; i < trianglesCount; i++)
            {
                for (int j = 0; j < 3; ++j)
                {
                    indices.Add(vectors.Count - 1 - (j == 0 ? 0 : i + j));
                }
            }
            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indices.ToArray();
        }
    }
}
