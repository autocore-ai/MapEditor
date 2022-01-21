using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MapEditor.WpfShell.ViewModels
{
    internal class MessageViewModel : BaseViewModel, IToolViewModel 
    {
        public const string CONTENT_ID = "8539A9A1-B694-4843-8E7B-44A8CC75EB02";

        #region fields

        private object m_LockMessages;
        private bool m_IsActive;
        private bool m_IsSelected;
        private bool m_IsVisible;
        private string m_ContentId;
        private string m_Title;
        private ObservableCollection<string> m_ListMessage;

        #endregion

        #region Properties

        public bool IsActive 
        { 
            get 
            { 
                return m_IsActive;
            }
            set 
            {
                if (m_IsActive != value)
                {
                    m_IsActive = value;
                    RaisePropertyChanged(() => IsActive);
                }
            }
        }
        public bool IsSelected 
        { 
            get 
            { 
                return m_IsSelected;
            }
            set 
            {
                if (m_IsSelected != value)
                {
                    m_IsSelected = value;
                    RaisePropertyChanged(() => IsSelected);
                }
            }
        }
        public bool IsVisible 
        {
            get 
            { 
                return m_IsVisible;
            }
            set 
            {
                if (m_IsVisible != value) 
                { 
                    m_IsVisible = value;
                    RaisePropertyChanged(() => IsVisible);
                }
            }
        }

        public string ContentId 
        { 
            get 
            { 
                return m_ContentId;
            }
        }
        public string Title 
        { 
            get 
            { 
                return m_Title;
            }
        }
        public ObservableCollection<string> ListMessage 
        {
            get 
            {
                return m_ListMessage;
            }
        }

        #endregion

        #region Commands

        public ICommand ClearCommand 
        {
            get;
            protected set;
        }

        #endregion

        protected override void InitFields() 
        {
            base.InitFields();
            m_LockMessages = new object();
            m_IsActive = true;
            m_IsSelected = false;
            m_IsVisible = true;
            m_ContentId = CONTENT_ID;
            m_Title = "Messages";

            m_ListMessage = new ObservableCollection<string>();
        }
        protected override void InitCommands() 
        {
            ClearCommand = new RelayCommand(OnClear);
        }
        protected override void Subscribe() 
        {
            Messenger.Default.Register<string>(this, WellkownMessages.MESSAGE_TOKEN_LOG, OnMessageReceived);
        }

        private void OnMessageReceived(string strMsg) 
        {
            if (string.IsNullOrEmpty(strMsg)) 
            {
                return;
            }
            InvokeOnUIThread(() =>
            {
                lock (m_LockMessages)
                {
                    m_ListMessage.Add(string.Format("[{0}]: {1}", DateTime.Now.ToString("HH:mm:ss"), strMsg));
                    RaisePropertyChanged(() => ListMessage);
                }
            });
        }
        private void OnClear() 
        {
            InvokeOnUIThread(() => 
            {
                m_ListMessage.Clear();
            });
        }
    }
}
