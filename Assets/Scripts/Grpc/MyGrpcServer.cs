using Grpc.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapEditor;

namespace ACGrpcServer
{
    public static class MyGrpcServer
    {
        private static Logs logger
        {
            get
            {
                return new Logs();
            }
        }
        public static void StartGrpcServer(int grpcPort)
        {
            try
            {
                Server server = new()
                {
                    Services = { MapEditorGrpcService.BindService(new GrpcServer()) },
                    Ports = { new ServerPort("localhost", grpcPort, ServerCredentials.Insecure) },
                };
                server.Start();
                logger.Println("GrpcServer Start On localhost:" + grpcPort);
            }
            catch (System.Exception e)
            {
                throw e;
            }

        }
    }
}
