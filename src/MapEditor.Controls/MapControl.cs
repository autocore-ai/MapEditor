using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MapEditor.Controls
{
    public partial class MapControl : UserControl
    {
        #region user32
        [DllImport("User32.dll")]
        static extern bool MoveWindow(IntPtr handle, int x, int y, int width, int height, bool redraw);

        internal delegate int WindowEnumProc(IntPtr hwnd, IntPtr lparam);
        [DllImport("user32.dll")]
        internal static extern bool EnumChildWindows(IntPtr hwnd, WindowEnumProc func, IntPtr lParam);

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        #endregion

        private bool m_IsUnityLoaded;
        private Process process;
        private IntPtr unityHWND = IntPtr.Zero;

        private const int WM_ACTIVATE = 0x0006;
        private readonly IntPtr WA_ACTIVE = new IntPtr(1);
        private readonly IntPtr WA_INACTIVE = new IntPtr(0);

        internal event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// 是否已加载Unity控件
        /// </summary>
        public bool IsUnityLoaded 
        {
            get 
            { 
                return m_IsUnityLoaded;
            }
        }

        public MapControl()
        {
            InitializeComponent();
        }

        public bool LoadUnityControl(string fileName) 
        {
            if (m_IsUnityLoaded) 
            {
                UnloadUnityControl();
            }
            try
            {
                process = new Process();
                process.StartInfo.FileName = fileName;
                process.StartInfo.Arguments = "-parentHWND " + panel1.Handle.ToInt32() + " " + Environment.CommandLine;
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                process.WaitForInputIdle();
                EnumChildWindows(panel1.Handle, WindowEnum, IntPtr.Zero);
                m_IsUnityLoaded = true;
            }
            catch (Exception ex)
            {
                m_IsUnityLoaded = false;
                OnError(ex);
            }
            return m_IsUnityLoaded;
        }

        public void UnloadUnityControl() 
        {
            if (!m_IsUnityLoaded) 
            {
                return;
            }
            try
            {
                process.CloseMainWindow();
                Thread.Sleep(1000);
                while (!process.HasExited) 
                {
                    process.Kill();
                }
                process.Dispose();
                process = null;
                m_IsUnityLoaded = false;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public new void Resize(int width, int height) 
        {
            try
            {
                float fDpi;
                using (Graphics graphics = CreateGraphics())
                {
                    fDpi = graphics.DpiX;
                }
                int iWidth = (int)(width * fDpi / 96.0);
                int iHeight = (int)(height * fDpi / 96.0);
                panel1.Width = iWidth;
                panel1.Height = iHeight;
                MoveWindow(unityHWND, 0, 0, panel1.Width, panel1.Height, true);
                ActivateUnityWindow();
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        internal void ActivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_ACTIVE, IntPtr.Zero);
        }

        internal void DeactivateUnityWindow()
        {
            SendMessage(unityHWND, WM_ACTIVATE, WA_INACTIVE, IntPtr.Zero);
        }

        private int WindowEnum(IntPtr hwnd, IntPtr lparam)
        {
            unityHWND = hwnd;
            ActivateUnityWindow();
            return 0;
        }

        private void OnError(Exception errorExcetion) 
        {
            if (Error != null) 
            { 
                Error(this, new ErrorEventArgs(errorExcetion));
            }
        }
    }
}
