
using Grpc.Core;
using Grpc.Core.Interceptors;
using MapEditor;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace ACGrpcServer
{

    public class GrpcServer : MapEditorGrpcService.MapEditorGrpcServiceBase
    {

        private static Logs logger
        {
            get
            {
                return new Logs();
            }
        }
        public override Task SubscribeFileOperation(Empty request, IServerStreamWriter<OperateFileAction> responseStream, ServerCallContext context)
        {
            return base.SubscribeFileOperation(request, responseStream, context);
        }
       
    }
}
