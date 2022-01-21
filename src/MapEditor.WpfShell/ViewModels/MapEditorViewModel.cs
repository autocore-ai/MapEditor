using GalaSoft.MvvmLight.Messaging;
using MapEditor.Grpc.Server;
using Microsoft.Win32;
using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace MapEditor.WpfShell.ViewModels
{
    /// <summary>
    /// TODO: kill unity when close
    /// </summary>
    internal class MapEditorViewModel : BaseViewModel 
    {
        const string TITLE_EMPTY_FILE = "(未保存)";

        #region fields

        private bool m_IsModified;
        private bool m_IsActive;
        private string m_FileName;
        private string m_MapUri;
        private string m_Title;
        private string m_ContentId;

        #endregion

        #region Properties

        public bool IsModified 
        {
            get 
            {
                return m_IsModified;
            }
            set 
            {
                m_IsModified = value;
                RaisePropertyChanged(() => IsModified);
            }
        }
        public bool IsActive 
        { 
            get 
            { 
                return m_IsActive;
            }
            set 
            { 
                m_IsActive = value;
                RaisePropertyChanged(() => IsActive);
            }
        }
        public string FileName 
        {
            get 
            {
                return m_FileName;
            }
            set 
            {
                m_FileName = value;
                RaisePropertyChanged(() => FileName);
                SetTitle(value);
            }
        }
        public string MapUri 
        {
            get 
            {
                return m_MapUri;
            }
            private set 
            {
                m_MapUri = value;
                RaisePropertyChanged(() => MapUri);
            }
        }
        public string Title 
        { 
            get 
            { 
                return m_Title;
            }
            private set 
            { 
                m_Title = value;
                RaisePropertyChanged(() => Title);
            }
        }
        public string ContentId 
        { 
            get 
            { 
                return m_ContentId;
            }
        }

        #endregion

        #region ICommands

        //public ICommand LoadPCDCommand
        //{
        //    get;
        //    private set;
        //}
        //public ICommand LoadOSMCommand
        //{
        //    get;
        //    private set;
        //}
        //public ICommand SaveOSMCommand
        //{
        //    get;
        //    private set;
        //}
        //public ICommand ExicCommand
        //{
        //    get;
        //    private set;
        //}

        #endregion

        protected override void InitFields() 
        {
            base.InitFields();
            m_ContentId = Guid.NewGuid().ToString("N");
            m_FileName = null;
            m_Title = TITLE_EMPTY_FILE;
            m_IsModified = false;
            m_IsActive = false;
            m_MapUri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["Tool_File_Path"].TrimStart("\\/".ToCharArray()));
        }
        protected override void Subscribe() 
        {
            Messenger.Default.Register<object>(this, WellkownMessages.MESSAGE_TOKEN_MENU_EXIT, o => { OnExit(); });
            Messenger.Default.Register<object>(this, WellkownMessages.MESSAGE_TOKEN_TOOL_ADDOSM, o => { OnAddOSM(); });
        }
        public override void Cleanup() 
        {
            base.Cleanup();
            MapUri = null;
        }

        private void SetTitle(string fileName) 
        {
            if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName)) 
            {
                Title = TITLE_EMPTY_FILE;
                return;
            }
            FileInfo fileInfo = new FileInfo(fileName);
            Title = fileInfo.Name;
        }

        internal void OnLoadPCD() 
        {
            if (!m_IsActive) 
            {
                return;
            }
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "pcd files|*.pcd|all files|*.*",
            };
            if (ofd.ShowDialog() == true)
            {
                Task.Factory.StartNew<bool>(() =>
                {
                    bool bResult;
                    try
                    {
                        MapEditorService.Instance.AddFileOperation(new Grpc.FileOperation(Grpc.FileOperateType.LoadPCD, ofd.FileName));
                        bResult = true;
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                        bResult = false;
                    }
                    return bResult;

                }).ContinueWith(b =>
                {
                    InvokeOnUIThread(() =>
                    {
                        IsModified = m_IsModified || b.Result;
                    });
                });
            }
        }
        internal void OnLoadOSM() 
        {
            if (!m_IsActive)
            {
                return;
            }
            if (m_IsModified)
            {
                // TODO: Notify save
            }
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "osm files|*.osm|all files|*.*",
            };
            if (openFileDialog.ShowDialog() == true) 
            {
                Task.Factory.StartNew<bool>(() => 
                {
                    bool bResult;
                    try
                    {
                        MapEditorService.Instance.AddFileOperation(new Grpc.FileOperation(Grpc.FileOperateType.LoadOSM, openFileDialog.FileName));
                        bResult = true;
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                        bResult = false;
                    }
                    return bResult;
                }).ContinueWith(b => 
                {
                    if (b.Result) 
                    {
                        InvokeOnUIThread(() => 
                        {
                            FileName = openFileDialog.FileName;
                        });
                    }
                });
            }
            m_IsModified = false;
        }
        internal void OnSaveOSM() 
        {
            if (!m_IsActive)
            {
                return;
            }
            SaveFileDialog sfd = new SaveFileDialog() 
            {
                Filter = "osm files|*.osm|all files|*.*",
            };
            if (sfd.ShowDialog() == true) 
            {
                Task.Factory.StartNew<bool>(() => 
                {
                    bool bResult;
                    try
                    {
                        MapEditorService.Instance.AddFileOperation(new Grpc.FileOperation(Grpc.FileOperateType.SaveOSM, sfd.FileName));
                        bResult = true;
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                        bResult = false;
                    }
                    return bResult;
                }).ContinueWith(b => 
                {
                    if (b.Result) 
                    {
                        InvokeOnUIThread(() => 
                        {
                            IsModified = false;
                            OnExit();
                        });
                    }
                });
            }
        }
        private void OnExit() 
        {
            Messenger.Default.Send(new object(), WellkownMessages.MESSAGE_TOKEN_MENU_EXIT);
            // TODO: Notify save if modified; Send exit command; close grpc; exit app
            MapUri = null;
        }

        internal bool OnBeginAddLanelet() 
        {
            MapEditorService.Instance.AddElementAddition(new Grpc.ElementAddition(Grpc.AddingElementType.Lanelet, true));
            return true;
        }
        internal bool OnEndAddLanelet() 
        {
            MapEditorService.Instance.AddElementAddition(new Grpc.ElementAddition(Grpc.AddingElementType.Lanelet, false));
            return true;
        }
        internal bool OnBeginAddWhiteLine()
        {
            MapEditorService.Instance.AddElementAddition(new Grpc.ElementAddition(Grpc.AddingElementType.WhiteLine, true));
            return true;
        }
        internal bool OnEdnAddWhiteLine()
        {
            MapEditorService.Instance.AddElementAddition(new Grpc.ElementAddition(Grpc.AddingElementType.WhiteLine, false));
            return true;
        }
        internal bool OnBeginAddStopLine()
        {
            MapEditorService.Instance.AddElementAddition(new Grpc.ElementAddition(Grpc.AddingElementType.StopLine, true));
            return true;
        }
        internal bool OnEndAddStopLine()
        {
            MapEditorService.Instance.AddElementAddition(new Grpc.ElementAddition(Grpc.AddingElementType.StopLine, false));
            return true;
        }

        internal void OnAddOSM() 
        { 
        }
    }
}
