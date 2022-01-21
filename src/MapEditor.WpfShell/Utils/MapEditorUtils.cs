using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace MapEditor.WpfShell.Utils
{
    internal static class MapEditorUtils
    {
        /// <summary>
        /// Check port of localhost, is in use or not
        /// </summary>
        public static bool CheckLocalPortInUse(int port) 
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
            bool isInUse = ipEndPoints != null && ipEndPoints.Length > 0 && ipEndPoints.Any(p => p.Port == port);
            return isInUse;
        }
    }
}
