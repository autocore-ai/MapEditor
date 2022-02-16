using Grpc.Core;
using MapEditor;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace ACGrpcServer
{
    public class GrpcClient : MonoBehaviour
    {
        public static GrpcClient Instance { get; private set; }
        private void Awake()
        {
            Instance = this;
        }
        public MapEditorClient client;
        // Start is called before the first frame update
        void Start()
        {
            Channel channel = new Channel("127.0.0.1:55001", ChannelCredentials.Insecure);
            client = new MapEditorClient(new MapEditorGrpcService.MapEditorGrpcServiceClient(channel));
            StartCoroutine(StartSubFileAction());
            StartCoroutine(StartSubElementAdd());
            StartCoroutine(StartSubElementSelected());
            StartCoroutine(StartSubMapEdit());
            StartCoroutine(StartSubSetTrafficLight());
            StartCoroutine(StartSubSetBezierMode());
        }
        IEnumerator StartSubFileAction()
        {
            yield return client.SubListFileAction();
        }
        IEnumerator StartSubElementAdd()
        {
            yield return client.SubElementAdd();
        }
        IEnumerator StartSubElementSelected()
        {
            yield return client.SubElementSelected();
        }
        IEnumerator StartSubMapEdit()
        {
            yield return client.SubMapEdit();
        }
        IEnumerator StartSubSetTrafficLight()
        {
            yield return client.SubSetTrafficLightTarget();
        }
        IEnumerator StartSubSetBezierMode()
        {
            yield return client.SubSetBezierMode();
        }
    }
}
