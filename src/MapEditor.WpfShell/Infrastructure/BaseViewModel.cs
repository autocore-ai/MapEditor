using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MapEditor.WpfShell 
{
    public class BaseViewModel : ViewModelBase 
    {
        #region fields

        protected bool m_Disposed;
        protected bool m_IsBusy;
        protected string m_BusyContent;

        #endregion

        #region Properties

        public bool IsBusy 
        {
            get 
            { 
                return m_IsBusy;
            }
            set 
            { 
                m_IsBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }
        public string BusyContent 
        { 
            get 
            { 
                return m_BusyContent;
            }
            set 
            { 
                m_BusyContent = value;
                RaisePropertyChanged(() => BusyContent);
            }
        }

        #endregion

        public BaseViewModel() 
            : base()
        {
            Initialize();
        }

        public void Initialize() 
        {
            InitFields();
            InitCommands();
            Subscribe();
            Task.Factory.StartNew(() => 
            { 
                LoadData();
            });
        }

        protected void OnError(Exception ex) 
        {
            ReportMessage(ex.ToString());
        }
        protected void ReportMessage(string strFormated) 
        {
            Messenger.Default.Send<string>(strFormated, WellkownMessages.MESSAGE_TOKEN_LOG);
        }

        protected void Debug(string strDebug) 
        {
#if DEBUG
            System.Diagnostics.Debug.Print(strDebug);
#else
#endif
        }

        public override void Cleanup()
        {
            base.Cleanup();
            UnSubscribe();
        }

        /// <summary>
        /// 1.Execute when constuct
        /// </summary>
        protected virtual void InitFields() 
        {
            m_IsBusy = false;
            m_BusyContent = null;
        }
        /// <summary>
        /// Subscribe messages / events
        /// </summary>
        protected virtual void Subscribe() 
        { 
        }
        /// <summary>
        /// UnSubscribe messages / events
        /// </summary>
        protected virtual void UnSubscribe() 
        { 
        }
        /// <summary>
        /// 2.Execute after fields
        /// </summary>
        protected virtual void InitCommands() 
        { 
        }
        /// <summary>
        /// 3.Async execute load after commands
        /// </summary>
        protected virtual void LoadData() 
        { 
        }

        /// <summary>
        /// Run action on ui thread
        /// </summary>
        public static void InvokeOnUIThread(Action action, bool isAnsy = false) 
        {
            Dispatcher dispatcher = (Application.Current != null && Application.Current.Dispatcher != null) ?
                Application.Current.Dispatcher : Dispatcher.CurrentDispatcher;
            if (dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                if (isAnsy)
                {
                    dispatcher.Invoke(action, null);
                }
                else
                {
                    dispatcher.BeginInvoke(action, null);
                }
            }
        }
    }
}
