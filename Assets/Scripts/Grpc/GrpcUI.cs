using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ACGrpcServer
{
    public class GrpcUI : MonoBehaviour
    {
        public Text text;
        // Start is called before the first frame update
        void Start()
        {
            GRPCManager.OnLoadFile += SetLoadFileText;
        }
        public void SetLoadFileText(string content)
        {
            text.text = content;
        }
    }
}
