using GalaSoft.MvvmLight.Messaging;

namespace MapEditor.WpfShell.ViewModels
{
    internal class PropertyViewModel : BaseViewModel, IToolViewModel
    {
        public const string CONTENT_ID = "13EA69E6-8216-491A-9368-CAAB96527FFB";

        #region fields

        private bool m_IsActive;
        private bool m_IsSelected;
        private bool m_IsVisible;
        private string m_ContentId;
        private string m_Title;

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

        #endregion

        protected override void InitFields() 
        {
            base.InitFields();
            m_IsActive = false;
            m_IsSelected = false;
            m_IsVisible = false;
            m_ContentId = CONTENT_ID;
            m_Title = "Properties";
        }
        protected override void Subscribe()
        {
            Messenger.Default.Register<string>(this, OnSelectionChanged);
        }

        private void OnSelectionChanged(string strElementId) 
        {
            
        }
    }
}
