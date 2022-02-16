using Grpc.Core;
using MapEditor;
using MapRenderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ACGrpcServer
{
    public class MapEditorClient
    {

        readonly MapEditorGrpcService.MapEditorGrpcServiceClient client;
        public MapEditorClient(MapEditorGrpcService.MapEditorGrpcServiceClient client)
        {
            this.client = client;
        }
        public async Task SubListFileAction()
        {
            try
            {
                Empty empty = new Empty();
                using (var call = client.SubscribeFileOperation(empty))
                {
                    var responseStream = call.ResponseStream;
                    while (await responseStream.MoveNext())
                    {
                        OperateFileAction action = responseStream.Current;
                        SendLogInfo("sub"+action.FilePath);
                        switch (action.OperateType)
                        {
                            case OperateFileAction.Types.OperateFileType.LoadOsm:
                                GRPCManager.LoadOSMFromUrl(action.FilePath);
                                break;
                            case OperateFileAction.Types.OperateFileType.LoadPcd:
                                GRPCManager.LoadPCDFromUrl(action.FilePath);
                                break;
                            case OperateFileAction.Types.OperateFileType.SaveOsm:
                                GRPCManager.SaveOSMToURL(action.FilePath);
                                break;
                        }
                    }
                }
            }
            catch (RpcException e)
            {
                throw;
            }
        }
        public async Task SubMapEdit()
        {
            try
            {
                Empty empty = new Empty();
                using (var call = client.SubscribeMapEdition(empty))
                {
                    var responseStream = call.ResponseStream;
                    while (await responseStream.MoveNext())
                    {
                        EditMapAction action = responseStream.Current;
                        SendLogInfo("sub" + action.EditType.ToString());
                        switch (action.EditType)
                        {
                            case EditMapAction.Types.EditMapType.Back:
                                GRPCManager.OnBack.Invoke();
                                break;
                            case EditMapAction.Types.EditMapType.Exit:
                                GRPCManager.OnExit.Invoke();
                                break;
                            case EditMapAction.Types.EditMapType.Redo:
                                GRPCManager.OnRedo.Invoke();
                                break;
                        }
                    }
                }
            }
            catch (RpcException e)
            {
                throw;
            }
        }

        public async Task SubElementAdd()
        {
            try
            {
                Empty empty = new Empty();
                using (var call = client.SubscribeElementAddition(empty))
                {
                    var responseStream = call.ResponseStream;
                    while (await responseStream.MoveNext())
                    {
                        AddElementAction action = responseStream.Current;
                        if (action.IsAdd)
                        {
                            switch (action.ElementType)
                            {
                                case ElementType.Lanelet:
                                    GRPCManager.OnStartAddElement.Invoke(0);
                                    break;
                                case ElementType.WhiteLine:
                                    GRPCManager.OnStartAddElement.Invoke(1);
                                    break;
                                case ElementType.StopLine:
                                    GRPCManager.OnStartAddElement.Invoke(2);
                                    break;
                                case ElementType.TrafficLight:
                                    GRPCManager.OnStartAddElement.Invoke(3);
                                    break;
                                default: break;
                            }
                        }
                        else
                        {
                            GRPCManager.OnEndAddElement.Invoke();
                        }
                    }
                }
            }
            catch (RpcException e)
            {
                throw;
            }

        }

        public async Task SubElementSelected()
        {
            try
            {
                Empty empty = new Empty();
                using (var call = client.SubscribeElementSelected(empty))
                {
                    var responseStream = call.ResponseStream;
                    while (await responseStream.MoveNext())
                    {
                        ElementId id = responseStream.Current;
                        GRPCManager.OnSeverElementSelected.Invoke(id.Id);
                    }
                }
            }
            catch (RpcException e)
            {
                throw;
            }
        }

        public async Task SubSetTrafficLightTarget()
        {
            try
            {
                Empty empty = new Empty();
                using (var call = client.SubScribeStartSetTrafficLightTarget(empty))
                {
                    var responseStream = call.ResponseStream;
                    while (await responseStream.MoveNext())
                    {
                        SetTrafficLightAction tl = responseStream.Current;
                        GRPCManager.OnSetTrafficLight.Invoke(tl.TrafficLightId.Id);
                    }
                }
            }
            catch (RpcException e)
            {
                throw;
            }
        }

        public async Task SubSetBezierMode()
        {
            try
            {
                Empty empty = new Empty();
                using (var call = client.SubscribeModifySharpeAction(empty))
                {
                    var responseStream = call.ResponseStream;
                    while (await responseStream.MoveNext())
                    {
                        ModifySharpeAction action = responseStream.Current;
                        GRPCManager.OnSetBezierMode.Invoke(action.IsModifying);
                    }
                }
            }
            catch (RpcException e)
            {
                throw;
            }
        }
        public void SetAddElementType(ElementType elementType,bool add)
        {
            try
            {
                AddElementAction addAction = new AddElementAction();
                addAction.ElementType = elementType;
                addAction.IsAdd = add;
                client.SetAddingElementType(addAction);
            }
            catch { throw; }
        }
        public void SendLogInfo(string message,LogInfo.Types.LogLevel type=LogInfo.Types.LogLevel.Info)
        {
            try
            {
                LogInfo info = new LogInfo();
                info.LogLevel = type;
                info.LogMessage = message;
                client.SendLogInfo(info);
            }
            catch { throw; }
        }
        
        public void SendElementTree()
        {
            try 
            {
                ElementsTree elementTree = new ElementsTree();
                elementTree.MapName = "name";
                Elements laneletElements = new Elements(){ ElementType = ElementType.Lanelet};
                foreach (Lanelet lanelet in MapManager.Instance.CurrentMap.lanelets)
                {
                    laneletElements.ElementIds.Add(lanelet.name);
                }
                Elements trafficLightElements = new Elements() { ElementType = ElementType.WhiteLine };
                Elements whiteLineElements = new Elements() { ElementType = ElementType.WhiteLine };
                Elements stopLineElements = new Elements() { ElementType = ElementType.StopLine };

                foreach (Way way in MapManager.Instance.CurrentMap.ways)
                {
                    if (way is Line_StopLine) stopLineElements.ElementIds.Add(way.name);
                    else if (way is Line_WhiteLine) whiteLineElements.ElementIds.Add(way.name);
                    else if (way is Sign_TrafficLight) trafficLightElements.ElementIds.Add(way.name);
                }

                elementTree.AllElement.Add(laneletElements);
                elementTree.AllElement.Add(whiteLineElements);
                elementTree.AllElement.Add(laneletElements);
                elementTree.AllElement.Add(stopLineElements);
                client.SendElementTree(elementTree);
            }
            catch { throw; }
        }
        public void SendElementData(ElementData data)
        {
            try
            {
                 client.SendElementData(data);
            }
            catch { throw; }
        }

    }
}
