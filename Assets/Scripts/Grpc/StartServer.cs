using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACGrpcServer
{
    public class StartServer : MonoBehaviour
    {
        //private int grpcPort = PortServer.GenerateRandomPort(50000, 60000);
        private int grpcPort = 55001;
        private int chatPort = PortServer.GenerateRandomPort(6001, 10000);
        private const string logPath = "./log/log.txt";
        // Start is called before the first frame update
        void Start()
        {
            MyGrpcServer.StartGrpcServer(grpcPort);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
