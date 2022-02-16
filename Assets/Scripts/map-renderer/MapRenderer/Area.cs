using System.Collections.Generic;
using UnityEngine;

namespace MapRenderer
{
    public class Area : Way
    {
        public Color color = Color.white;
        protected List<int> indices;
        //顶点数组
        protected Vector3[] Vertexes;
        private Vector2[] uvs;
        //网格过滤器
        protected MeshFilter _meshFilter;
        protected MeshRenderer _meshRenderer;

        
        public override void ElementUpdateRenderer()
        {
            base.ElementUpdateRenderer();
            if (elemenrMaterial == null)
            {
                elemenrMaterial = new Material(Shader.Find("Standard"));
                elemenrMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                elemenrMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                elemenrMaterial.SetInt("_ZWrite", 0);
                elemenrMaterial.DisableKeyword("_ALPHATEST_ON");
                elemenrMaterial.EnableKeyword("_ALPHABLEND_ON");
                elemenrMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                elemenrMaterial.renderQueue = 3000;
                elemenrMaterial.EnableKeyword("_EMISSION");
                color.a = 0.4f;
                elemenrMaterial.color = color;
                elemenrMaterial.SetColor("_EmissionColor", color);
            }
            if(_meshFilter==null) _meshFilter = GetComponent<MeshFilter>();
            if(_meshRenderer==null) _meshRenderer = GetComponent<MeshRenderer>();

            Vertexes = new Vector3[points.Count*2];
            for (int i = 0; i < points.Count; i++)
            {
                Vertexes[i] = Vertexes[points.Count + i] = points[i].Position;
            }

            //得到三角形的数量
            int trianglesCount = points.Count - 2;
            //三角形顶点ID数组
            indices = new List<int>();
            //三角形顶点索引,确保按照顺时针方向设置三角形顶点
            for (int i = 0; i < trianglesCount; i++)
            {
                indices.Add(0);
                indices.Add(i + 1);
                indices.Add(i + 2);
            }
            for (int i = 0; i < trianglesCount; i++)
            {
                indices.Add(points.Count + 0);
                indices.Add(points.Count + i + 2);
                indices.Add(points.Count + i + 1);
            }
            Mesh mesh = new Mesh();
            mesh.vertices = Vertexes;
            //mesh.uv = uvs;
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            _meshFilter.mesh = mesh;

            _meshRenderer.material = elemenrMaterial;

        }
        public override void OnDestory()
        {
            map.RemoveArea(this);
            base.OnDestory();
        }
    }
}