using MapEditor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace MapEditor.Grpc.Server
{
    public class MapEditorService
    {
        public static readonly MapEditorService Instance = new MapEditorService();

        private event Action<FileOperation> FileOperating;
        private event Action<MapEdition> MapEditing;
        private event Action<ElementAddition> ElementAdding;

        public event EventHandler<EventArgs<RenderLogInfo>> ReportLogInfo;
        public event EventHandler<EventArgs<IElementInfo>> SelectionChanged;

        internal IObservable<FileOperation> GetFileOperationAsObservable() 
        {
            var oldOperation = new List<FileOperation>(1).ToObservable();
            var newOperation = Observable.FromEvent<FileOperation>((f) => FileOperating += f, (f) => FileOperating -= f);
            return oldOperation.Concat(newOperation);
        }
        internal IObservable<MapEdition> GetMapEditionAsObservable() 
        { 
            var oldEdition = new List<MapEdition>(1).ToObservable();
            var newEdition = Observable.FromEvent<MapEdition>((m) => MapEditing += m, (m) => MapEditing -= m); ;
            return oldEdition.Concat(newEdition);
        }
        internal IObservable<ElementAddition> GetElementAdditionAsObservable() 
        { 
            var oldAddition = new List<ElementAddition>(1).ToObservable();
            var newAddition = Observable.FromEvent<ElementAddition>((e) => ElementAdding += e, (e) => ElementAdding -= e);
            return oldAddition.Concat(newAddition);
        }

        public void AddFileOperation(FileOperation fileOperation) 
        {
            FileOperating?.Invoke(fileOperation);
        }
        public void AddMapEdition(MapEdition mapEdition) 
        { 
            MapEditing?.Invoke(mapEdition);
        }
        public void AddElementAddition(ElementAddition elementAddition) 
        {
            ElementAdding?.Invoke(elementAddition);
        }

        internal void AddRenderLogInfo(RenderLogInfo logInfo) 
        {
            ReportLogInfo?.Invoke(this, new EventArgs<RenderLogInfo>(logInfo));
        }
        internal void ChangeSelectedElement(IElementInfo elementInfo) 
        { 
            SelectionChanged?.Invoke(this, new EventArgs<IElementInfo>(elementInfo));
        }

        private MapEditorService() 
        { 
        }
    }
}
