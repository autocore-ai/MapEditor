using Grpc.Core;
using MapEditor.Models;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MapEditor.Grpc.Server
{
    public class MapEditorServer : MapEditorGrpcService.MapEditorGrpcServiceBase 
    {
        public static readonly MapEditorServer Instance = new MapEditorServer();
        private static readonly Empty Empty = new Empty();

        private MapEditorServer() 
        { 
        }

        public override Task<Empty> SendLogInfo(LogInfo request, ServerCallContext context)
        {
            MapEditorService.Instance.AddRenderLogInfo(new RenderLogInfo((int)request.LogLevel, request.LogMessage));
            return Task.FromResult<Empty>(Empty);
        }

        public override Task<Empty> SetAddingElementType(AddElementAction request, ServerCallContext context)
        {
            MapEditorService.Instance.AddElementAddition(request);
            return Task.FromResult(Empty);
        }

        public override async Task SubscribeElementAddition(Empty request, IServerStreamWriter<AddElementAction> responseStream, ServerCallContext context)
        {
            var peer = context.Peer;
            string strMessageDebug = $"{peer} subscribe ElementAddition message";
            MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, strMessageDebug));

            context.CancellationToken.Register(() => { MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, $"{peer} cancel subscribe ElementAddition message")); });
            try
            {
                await MapEditorService.Instance.GetElementAdditionAsObservable().ToAsyncEnumerable().ForEachAwaitAsync(async (e) => await responseStream.WriteAsync(e), context.CancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, $"{peer} cancel subscribe ElementAddition message"));
            }
        }

        public override async Task SubscribeFileOperation(Empty request, IServerStreamWriter<OperateFileAction> responseStream, ServerCallContext context)
        {
            var peer = context.Peer;
            string strMessageDebug = $"{peer} subscribe FileOperation message";
            MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, strMessageDebug));

            context.CancellationToken.Register(() => { MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, $"{peer} cancel subscribe FileOperation message")); });
            try
            {
                await MapEditorService.Instance.GetFileOperationAsObservable().ToAsyncEnumerable().ForEachAwaitAsync(async (f) => await responseStream.WriteAsync(f), context.CancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, $"{peer} cancel subscribe FileOperation message"));
            }
        }

        public override async Task SubscribeMapEdition(Empty request, IServerStreamWriter<EditMapAction> responseStream, ServerCallContext context)
        {
            var peer = context.Peer;
            string strMessageDebug = $"{peer} subscribe MapEdition message";
            MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, strMessageDebug));

            context.CancellationToken.Register(() => { MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, $"{peer} cancel subscribe MapEdition message")); });
            try
            {
                await MapEditorService.Instance.GetMapEditionAsObservable().ToAsyncEnumerable().ForEachAwaitAsync(async (m) => await responseStream.WriteAsync(m), context.CancellationToken).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                MapEditorService.Instance.AddRenderLogInfo(new Models.RenderLogInfo(1, $"{peer} cancel subscribe MapEdition message"));
            }
        }
    }
}
