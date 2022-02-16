using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using MapEditor;
using System.IO;
using System;
using MapRenderer;
using assets.OSMReader;

namespace ACGrpcServer
{
    public static class GRPCManager
    {
        public static string agentAddress = "127.0.0.1:55001";

        private static Logs logger
        {
            get
            {
                return new Logs();
            }
        }
        public static void SenLogInfo(string info)
        {
            GrpcClient.Instance.client.SendLogInfo(info);
        }

        public static Action<string> OnLoadFile;
        public static Action<string> OnSaveFile;
        public static void LoadOSMFromUrl(string path)
        {
            if (!File.Exists(path))
            {
                loadStatu = false;
                ErrorMessage = "No such file";
            }
            else
            {
                loadStatu = true;
                OnLoadFile.Invoke(path);
            }
        }
        public static void LoadPCDFromUrl(string path)
        {
            if (!File.Exists(path))
            {
                loadStatu = false;
                ErrorMessage = "No such file";
            }
            else
            {
                loadStatu = true;
                OnLoadFile.Invoke(path);
            }
        }
        public static void SaveOSMToURL(string path)
        {
            try
            {
                loadStatu = true;
                OnSaveFile.Invoke(path);
            }
            catch (Exception e)
            {
                loadStatu = false;
                ErrorMessage = e.ToString();
                throw;
            }
        }
        public static Action<string> OnAddOSMMap;
        public static void AddOSMMap(string url)
        {
            try
            {
                File.Create(url);
                string name = Path.GetFileNameWithoutExtension(url);
                OnAddOSMMap.Invoke(name);
            }
            catch 
            { 
            
            }
        }
        private static string message=string.Empty;
        public static string ErrorMessage
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
            }
        }

        private static bool loadStatu = true;
        public static bool LoadStatu
        {
            get
            {
                return loadStatu;
            }
            set
            {
                loadStatu = value;
            }
        }

        public static Action OnRedo;
        public static Action OnBack;
        public static Action OnExit;

        public static Action<int> OnStartAddElement;
        public static Action OnEndAddElement;

        public static Action<string> OnSeverElementSelected;
        public static Action<string> OnSetTrafficLight;
        public static Action<bool> OnSetBezierMode;

        public static void SendCurrentElement( MapElement ele)
        {
            if(ele is Lanelet)
            {
                GrpcClient.Instance.client.SetAddElementType(ElementType.Lanelet,true);
            }
            else if (ele is Line_WhiteLine)
            {
                GrpcClient.Instance.client.SetAddElementType(ElementType.WhiteLine, true);
            }
            else if (ele is Line_StopLine)
            {
                GrpcClient.Instance.client.SetAddElementType(ElementType.StopLine, true);
            }
        }
        public static void SendElementData(MapElement ele)
        {
            if (ele is Point) return;
            ElementData elementData = new ElementData();
            elementData.ElementId = ele.name;

            if (ele is Lanelet)
            {
                elementData.ElementType = ElementType.Lanelet;
            }
            else if (ele is Line_WhiteLine)
            {
                elementData.ElementType = ElementType.WhiteLine;
            }
            else if (ele is Line_StopLine)
            {
                elementData.ElementType = ElementType.StopLine;
            }
            foreach (OSMTag tag in ele.Tags)
            {
                elementData.Tags.Add(new Tag {K=tag.Key,V=tag.Value });
            }
            elementData.Tags.Add(new Tag { K = "k", V = "v" });
            GrpcClient.Instance.client.SendElementData(elementData);
        }

    }
}
