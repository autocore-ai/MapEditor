using System.IO;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Xml;
using assets.OSMReader;
using AutoCore.MapToolbox.PCL;
using ACGrpcServer;
using Packages.BezierCurveEditorPackage.Scripts;
using UnityEngine.EventSystems;

namespace MapRenderer
{
    public class MapManager : MonoBehaviour
    {
        public PCDImporter PCDImporter;

        public OSMManager osmManager;

        public static MapManager Instance;

        public Action<string> OnGetOSM;

        public Action<byte[]> OnGetOpenDrive;

        public List<Map> mapList;

        public float PointDiameter;

        public Material laneletMat;
        public Material LineMat;
        public Material StopLineMat;


        private Map currentMap;
        public Map CurrentMap
        {
            get
            {
                if (currentMap == null)
                    currentMap = GetOrCreateMap("newMap");
                return currentMap;
            }
            set
            {
                currentMap = value;
            }
        }

        public GameObject goTrafficLight;

        public GameObject goLine;

        public GameObject goStructure;

        public GameObject goArea;

        public GameObject goPoint;

        public GameObject goLaneLet;

        private void Awake()
        {
            Instance = this;
            mapList = new List<Map>();
            Map[] maps = GetComponentsInChildren<Map>();
            foreach (Map item in maps)
            {
                mapList.Add(item);
            }
            if (PCDImporter == null) PCDImporter = GetComponent<PCDImporter>(); 
        }
        private void Start()
        {
        }
        public void SetFileUrl(string path)
        {
            isLoadMap = true;
            url = path;
        }
        void Exit()
        {
            Application.Quit();
        }
        void Back()
        {
            Debug.Log("back");
        }
        void ReDo()
        {
            Debug.Log("redo");

        }
        public void CreateMap(string name)
        {
            CurrentMap = GetOrCreateMap(name);
        }
        public void SaveMapToPath(string path)
        {
            Debug.Log(path+"");
            if (osmManager == null) osmManager = GetComponent<OSMManager>();
            osmManager.SaveMapXMLToPath(CurrentMap,path);
        }

        public void LoadMapFromPath(string path)
        {
            if (osmManager == null) osmManager = GetComponent<OSMManager>();
            osmManager.SaveMapXMLToPath(CurrentMap, path);
        }
        public bool isLoadMap = false;
        private string url = string.Empty;

        public void LoadMap(string path)
        {
            isLoadMap = true;
            url = path;
        }

        private void Update()
        {
            //if (laneLR.enabled)
            //{
            //    offset -= Time.deltaTime * 0.5f;
            //    laneMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
            //}
            if (isLoadMap)
            {
                isLoadMap = false;
                LoadMapFromURL(url);
            }
        }
        public Map GetOrCreateMap(string mapName)
        {
            foreach (Map map in mapList)
            {
                if (map.name == mapName)
                {
                    return map;
                }
            }
            Map newMap = new GameObject(mapName).AddComponent<Map>();
            mapList.Add(newMap);
            newMap.transform.SetParent(transform);
            newMap.goPoint = goPoint;
            newMap.goWhiteLine = goLine;
            newMap.goArea = goArea;
            newMap.goTrafficLight = goTrafficLight;
            newMap.goLaneLet = goLaneLet;
            newMap.goStructure = goStructure;
            newMap.Init();
            return newMap;
        }
        public void LoadMapFromURL(string url)
        {
            StartCoroutine(GetDataFromURL(url));
        }
        private IEnumerator GetDataFromURL(string url)
        {
            if (url.EndsWith(".osm"))
            {
                Debug.Log("osm");
                UnityWebRequest www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    // Show results as text
                    string content = www.downloadHandler.text;
                    Debug.Log(content);
                    OnGetOSM.Invoke(content);
                }
            }
            else if (url.EndsWith(".xodr"))
            {
                Debug.Log("xodr");
                UnityWebRequest www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    // Show results as text
                    byte[] content = www.downloadHandler.data;
                    OnGetOpenDrive.Invoke(content);
                }
            }
            if (url.EndsWith("pcd"))
            {
                //GRPCManager.SenLogInfo("load pcd at"+url);
                PCDImporter.ImportPCD(url, "pcd");
                yield return 0;
            }
            yield return null;
        }
    }

}

