using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MapEditor.Grpc;
using MapEditor.Grpc.Server;
using MapEditor.Models;
using MapEditor.WpfShell.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace MapEditor.WpfShell
{
    internal class MainViewModel : BaseViewModel 
    {
        #region fields

        // toolbar begin
        // access modifier should be set to public

        public bool m_IsLaneletAdding;
        public bool m_IsWhiteLineAdding;
        public bool m_IsStopLineAdding;

        // toolbar end

        private MapEditorViewModel m_CurrentEditor;
        private MessageViewModel m_MessageVM;
        private PropertyViewModel m_PropertyVM;
        private ObservableCollection<MapEditorViewModel> m_ListEditor;
        private ObservableCollection<IToolViewModel> m_ListTool;

        #endregion

        #region Properties

        public bool IsLaneletAdding 
        { 
            get 
            { 
                return m_IsLaneletAdding;
            }
            set 
            {
                if (m_CurrentEditor == null) 
                {
                    return;
                }
                if (m_IsLaneletAdding != value)
                {
                    Debug($"IsLaneletAdding now changing to {value}");
                    ChangeAddingState(value, () => m_IsLaneletAdding, () => IsLaneletAdding, m_CurrentEditor.OnBeginAddLanelet, m_CurrentEditor.OnEndAddLanelet);
                    Debug($"IsLaneletAdding now changed to {m_IsLaneletAdding}");
                    RaisePropertyChanged(() => IsLaneletAdding);
                }
            }
        }
        public bool IsWhiteLineAdding 
        { 
            get 
            { 
                return m_IsWhiteLineAdding;
            }
            set 
            {
                if (m_CurrentEditor == null)
                {
                    return;
                }
                if (m_IsWhiteLineAdding != value)
                {
                    Debug($"IsWhiteLineAdding now changing to {value}");
                    ChangeAddingState(value, () => m_IsWhiteLineAdding, () => IsWhiteLineAdding, m_CurrentEditor.OnBeginAddWhiteLine, m_CurrentEditor.OnEdnAddWhiteLine);
                    Debug($"IsWhiteLineAdding now changed to {m_IsWhiteLineAdding}");
                    RaisePropertyChanged(() => IsWhiteLineAdding);
                }
            }
        }
        public bool IsStopLineAdding 
        { 
            get 
            { 
                return m_IsStopLineAdding;
            }
            set 
            {
                if (m_CurrentEditor == null)
                {
                    return;
                }
                if (m_IsStopLineAdding != value) 
                {
                    Debug($"IsStopLineAdding now changing to {value}");
                    ChangeAddingState(value, () => m_IsStopLineAdding, () => IsStopLineAdding, m_CurrentEditor.OnBeginAddStopLine, m_CurrentEditor.OnEndAddStopLine);
                    Debug($"IsStopLineAdding now changing to {m_IsStopLineAdding}");
                    RaisePropertyChanged(() => IsStopLineAdding);
                }
            }
        }

        public MapEditorViewModel CurrentEditor 
        { 
            get 
            { 
                return m_CurrentEditor;
            }
            set 
            { 
                m_CurrentEditor = value;
            }
        }
        public MessageViewModel MessageVM 
        {
            get 
            { 
                return m_MessageVM;
            }
        }
        public PropertyViewModel PropertyVM 
        {
            get 
            { 
                return m_PropertyVM;
            }
        }
        public IEnumerable<MapEditorViewModel> ListEditor 
        {
            get 
            { 
                return m_ListEditor;
            }
        }
        public IEnumerable<IToolViewModel> ListTool 
        {
            get 
            { 
                return m_ListTool;
            }
        }

        #endregion

        #region ICommands

        public ICommand LoadPCDCommand 
        {
            get;
            private set;
        }
        public ICommand LoadOSMCommand 
        {
            get;
            private set;
        }
        public ICommand SaveOSMCommand 
        {
            get;
            private set;
        }
        public ICommand ExicCommand 
        {
            get;
            private set;
        }

        public ICommand EditBackCommand 
        {
            get;
            private set;
        }
        public ICommand EditRedoCommand 
        {
            get;
            private set;
        }
        public ICommand DeleteElementCommand 
        {
            get;
            private set;
        }

        public ICommand AddOsmMapCommand 
        {
            get;
            private set;
        }

        #endregion

        protected override void InitFields() 
        {
            base.InitFields();
            m_ListEditor = new ObservableCollection<MapEditorViewModel>();
            MapEditorViewModel mapEditorVM = new MapEditorViewModel();
            m_ListEditor.Add(mapEditorVM);
            m_ListTool = new ObservableCollection<IToolViewModel>();
            MessageViewModel messageVM = new MessageViewModel();
            m_ListTool.Add(messageVM);
            PropertyViewModel propertyVM = new PropertyViewModel();
            m_ListTool.Add(propertyVM);
            m_CurrentEditor = mapEditorVM;
            m_MessageVM = messageVM;
            m_PropertyVM = propertyVM;
        }
        protected override void InitCommands() 
        {
            // group file
            LoadPCDCommand = new RelayCommand(() => { m_CurrentEditor?.OnLoadPCD(); });
            LoadOSMCommand = new RelayCommand(() => { m_CurrentEditor?.OnLoadOSM(); });
            SaveOSMCommand = new RelayCommand(() => { m_CurrentEditor?.OnSaveOSM(); });
            ExicCommand = new RelayCommand(OnExit);

            AddOsmMapCommand = new RelayCommand(() => { m_CurrentEditor?.OnAddOSM(); });
        }
        protected override void Subscribe() 
        {
            Messenger.Default.Register<bool>(this, WellkownMessages.MESSAGE_APPLICATION_BUSY, b => { InvokeOnUIThread(() => IsBusy = b); });
            MapEditorService.Instance.ReportLogInfo += OnReportLogInfo;
            MapEditorService.Instance.SelectionChanged += OnSelectionChanged;
        }
        protected override void UnSubscribe() 
        {
            MapEditorService.Instance.ReportLogInfo -= OnReportLogInfo;
            MapEditorService.Instance.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, EventArgs<IElementInfo> e)
        {
            Messenger.Default.Send(e.Value, WellkownMessages.MESSAGE_TOKEN_SELECTIONCHANGE);
        }
        private void OnReportLogInfo(object sender, EventArgs<Models.RenderLogInfo> e)
        {
            Messenger.Default.Send(e.Value.ToString(), WellkownMessages.MESSAGE_TOKEN_LOG);
        }

        protected override void LoadData() 
        {
            InvokeOnUIThread(() => 
            { 
                Messenger.Default.Send<string>("Application loaded", WellkownMessages.MESSAGE_TOKEN_LOG);
            });
        }
        public override void Cleanup() 
        {
            base.Cleanup();
            if (m_ListEditor != null) 
            {
                foreach (MapEditorViewModel mapEditor in m_ListEditor)
                {
                    mapEditor.Cleanup();
                }
            }
            if (m_ListTool != null) 
            {
                foreach (IToolViewModel toolView in m_ListTool)
                {
                    toolView.Cleanup();
                }
            }
        }

        private MemberInfo GetMemberExpression<T>(Expression<Func<T>> fieldExpression) 
        {
            if (fieldExpression == null)
            {
                throw new ArgumentNullException("fieldExpression");
            }
            var memberExpression = fieldExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentNullException("propertyExpression is not a member");
            }
            var memberInfo = memberExpression.Member as MemberInfo; ;
            return memberInfo;
        }
        private FieldInfo GetFieldInfo<T>(Expression<Func<T>> fieldExpression) 
        {
            var memberInfo = GetMemberExpression(fieldExpression);
            if (memberInfo == null)
            {
                throw new ArgumentException("propertyExpression is not a member");
            }
            FieldInfo fieldInfo = typeof(MainViewModel).GetField(memberInfo.Name);
            return fieldInfo;
        }
        private void ChangeAddingState<T>(bool value, Expression<Func<T>> fieldExpression, Expression<Func<T>> fieldIgnore, Func<bool> actionBegin, Func<bool> actionEnd) 
        {
            FieldInfo fieldInfo = GetFieldInfo(fieldExpression);
            if (fieldInfo == null) 
            {
                return;
            }
            try
            {
                fieldInfo.SetValue(this, value);
            }
            catch (Exception ex)
            {
                OnError(ex);
                return;
            }
            if (value)
            {
                if (ResetAddingState(fieldIgnore))
                {
                    actionBegin();
                }
                else
                {
                    fieldInfo.SetValue(this, false);
                }
            }
            else 
            {
                actionEnd();
            }
        }
        private bool ResetAddingState<T>(Expression<Func<T>> fieldIgnore) 
        {
            bool reseted = true;
            MemberInfo memberInfo = GetMemberExpression(fieldIgnore);
            if (memberInfo == null) 
            {
                return false;
            }
            string fieldName = memberInfo.Name;
            if (fieldName != nameof(IsLaneletAdding)) 
            {
                IsLaneletAdding = false;
                reseted &= !m_IsLaneletAdding;
            }
            if (fieldName != nameof(IsWhiteLineAdding)) 
            {
                IsWhiteLineAdding = false;
                reseted &= !m_IsWhiteLineAdding;
            }
            if (fieldName != nameof(IsStopLineAdding)) 
            {
                IsStopLineAdding = false;
                reseted &= !m_IsStopLineAdding;
            }
            return reseted;
        }

        private void OnExit() 
        {
            MapEditorService.Instance.AddMapEdition(new MapEdition(EditType.Exit));
            Application.Current.MainWindow.Close(); // Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose (setted in App.xaml)
        }
    }
}
