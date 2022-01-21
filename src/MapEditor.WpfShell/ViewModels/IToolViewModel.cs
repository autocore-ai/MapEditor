using GalaSoft.MvvmLight;

namespace MapEditor.WpfShell.ViewModels
{
    internal interface IToolViewModel : ICleanup
    {
        #region Properties

        bool IsActive 
        {
            get;
            set;
        }
        bool IsSelected 
        {
            get;
            set;
        }
        bool IsVisible 
        {
            get;
            set;
        }
        string ContentId 
        {
            get;
        }
        string Title 
        {
            get;
        }

        #endregion
    }
}
