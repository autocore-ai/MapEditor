using GalaSoft.MvvmLight.Messaging;
using Grpc.Core;
using Grpc.Core.Interceptors;
using MapEditor.Grpc.Server;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MapEditor.WpfShell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex Mutex_App = new Mutex(true, "MapEditor.WpfShell");
        private Server m_Server;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (Mutex_App.WaitOne(0, false))
            {
                base.OnStartup(e);
                DispatcherUnhandledException += OnDispatcherUnhandledException;
                TaskScheduler.UnobservedTaskException += OnTaskUnobservedException;
                AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainException;
                m_Server = new Server() 
                { 
                    Services = { MapEditorGrpcService.BindService(MapEditorServer.Instance).Intercept(new IpAddressAuthenticator()) },
                    Ports = { new ServerPort("127.0.0.1", 55001, ServerCredentials.Insecure) }
                };
                m_Server.Start();
            }
            else 
            {
                MessageBox.Show("程序已经在运行！", "提示");
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (m_Server != null)
            {
                m_Server.ShutdownAsync();
                m_Server = null;
            }
            DispatcherUnhandledException -= OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException -= OnTaskUnobservedException;
            AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainException;
            base.OnExit(e);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e) 
        {
            string message = $"exception handled on ui thread : {e.Exception}";
            try
            {
                e.Handled = true;
                Messenger.Default.Send(message, WellkownMessages.MESSAGE_TOKEN_LOG);
            }
            catch (Exception)
            {
                Messenger.Default.Send(new object(), WellkownMessages.MESSAGE_TOKEN_MENU_EXIT);
            }
        }

        private void OnCurrentDomainException(object sender, UnhandledExceptionEventArgs e) 
        {
            //string message
            StringBuilder sbMessage = new StringBuilder();
            if (e.IsTerminating)
            {
                sbMessage.Append("UI ");
            }
            sbMessage.Append($"Exception{Environment.NewLine}");
            if (e.ExceptionObject is Exception)
            {
                sbMessage.Append(((Exception)e.ExceptionObject).Message);
            }
            else 
            {
                sbMessage.Append(e.ExceptionObject);
            }
            Messenger.Default.Send(sbMessage.ToString(), WellkownMessages.MESSAGE_TOKEN_LOG);
        }

        private void OnTaskUnobservedException(object sender, UnobservedTaskExceptionEventArgs e) 
        {
            string message = $"Task Exception : {e.Exception.Message}";
            Messenger.Default.Send(message, WellkownMessages.MESSAGE_TOKEN_LOG);
            e.SetObserved();
        }
    }
}
