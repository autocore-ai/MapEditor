using AutoCore.MapToolbox.PCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;

public class PCDImporter : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject m_cloudPoint;

    public Material m_material;

    internal void ImportPCD(string path, string pcdName)
    {
        using (var reader = new PointCloudReader(path))
        {
            if (reader.PointXYZRGBAs.IsCreated)
            {
                CreatePCDGameObject(pcdName,reader.PointXYZRGBAs.CoordinateRosToUnity().ToUnityColor().PointsCell(new int2(100)).ToMeshes());
            }
            else if (reader.PointXYZIs.IsCreated)
            {
                var colored = reader.PointXYZIs.IntensityToColor();
                CreatePCDGameObject(pcdName, colored.CoordinateRosToUnity().PointsCell(new int2(100)).ToMeshes());
                colored.Dispose();
            }
        }
    }

    private void CreatePCDGameObject(string name, List<(Vector3, Mesh)> meshes)
    {
        Destroy(m_cloudPoint);
        m_cloudPoint = new GameObject(name);
        m_cloudPoint.transform.SetParent(transform);
        m_cloudPoint.layer = LayerMask.NameToLayer("Ground");
        for (int i = 0; i < meshes.Count; i++)
        {
            var mesh = meshes[i];
            mesh.Item2.name = "points" + i;
            var chunk = new GameObject();
            chunk.layer = LayerMask.NameToLayer("Ground");
            chunk.name = i.ToString();
            chunk.transform.parent = m_cloudPoint.transform;
            chunk.transform.localPosition = mesh.Item1;
            chunk.AddComponent<MeshFilter>().mesh = mesh.Item2;
            chunk.AddComponent<MeshRenderer>().sharedMaterial = m_material;
            var terrainData = mesh.Item2.GetTerrainData();
            var terrain = chunk.AddComponent<Terrain>();
            terrain.terrainData = terrainData;
            chunk.AddComponent<TerrainCollider>().terrainData = terrainData;
            terrain.enabled = false;
        }
    }
}
