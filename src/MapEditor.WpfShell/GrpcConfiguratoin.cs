using System;
using System.IO;

namespace MapEditor.WpfShell
{
    public class GrpcConfiguratoin
    {
        public static readonly GrpcConfiguratoin Default = new GrpcConfiguratoin();

        #region fields

        private bool m_IsSslMode;
        private string m_ServerCert;
        private string m_ClientCert;
        private string m_PrivateKey;

        private string m_Host;
        private int m_Port;

        #endregion

        #region Properties

        public bool IsSslMode 
        { 
            get 
            { 
                return m_IsSslMode;
            }
            protected set 
            { 
                m_IsSslMode = value;
            }
        }
        public string ServerCert 
        { 
            get 
            { 
                return m_ServerCert;
            }
            protected set 
            { 
                m_ServerCert = value;
            }
        }
        public string ClientCert
        {
            get 
            {
                return m_ClientCert;
            }
            protected set 
            { 
                m_ClientCert = value;
            }
        }
        public string PrivateKey 
        { 
            get 
            { 
                return m_PrivateKey;
            }
            protected set 
            { 
                m_PrivateKey = value;
            }
        }
        
        public string Host 
        { 
            get 
            { 
                return m_Host;
            }
            protected set 
            { 
                m_Host = value;
            }
        }
        public int Port 
        { 
            get 
            { 
                return m_Port;
            }
            protected set 
            { 
                m_Port = value;
            }
        }

        #endregion

        public GrpcConfiguratoin() 
            : this("127.0.0.1", 45001, false, string.Empty, string.Empty, string.Empty) 
        { 
        }
        public GrpcConfiguratoin(string host, int port) 
            : this(host, port, false, string.Empty, string.Empty, string.Empty) 
        { 
        }
        public GrpcConfiguratoin(string host, int port, bool isSslMode, string serverCert, string clientCert, string privateKey) 
        { 
            m_Host = host;
            m_Port = port;
            m_IsSslMode = isSslMode;
            m_ServerCert = serverCert;
            m_ClientCert = clientCert;
            m_PrivateKey = privateKey;
        }

        public static GrpcConfiguratoin ParseFromXmlFile(string strFileName) 
        {
            if (!File.Exists(strFileName)) 
            { 
                return Default;
            }
            using (FileStream fs = File.OpenRead(strFileName))
            {
                using (StreamReader sr = new StreamReader(fs))
                {
                    return ParseFromXml(sr.ReadToEnd());
                }
            }
        }
        public static GrpcConfiguratoin ParseFromXml(string strXml) 
        {
            GrpcConfiguratoin result;
            try
            {
                // TODO: Parse xml format
                string host = "";
                int port = 45001;
                bool isSslMode = false;
                string serverCert = string.Empty;
                string clientCert = string.Empty;
                string privateKey = string.Empty;
                result = new GrpcConfiguratoin(host, port, isSslMode, serverCert, clientCert, privateKey);
            }
            catch (Exception)
            {
                result = Default;
            }
            return result;
        }
        public static GrpcConfiguratoin ParseFromJsonFile(string strJsonFile) 
        {
            if (!File.Exists(strJsonFile)) 
            {
                return Default;
            }
            using (FileStream fs = File.OpenRead(strJsonFile))
            {
                using (StreamReader sr = new StreamReader(fs)) 
                { 
                    return ParseFromJson(sr.ReadToEnd());
                }
            }
            throw new NotImplementedException();
        }
        public static GrpcConfiguratoin ParseFromJson(string strJson) 
        {
            GrpcConfiguratoin result;
            try
            {
                // TODO: Parse json format
                string host = "";
                int port = 45001;
                bool isSslMode = false;
                string serverCert = string.Empty;
                string clientCert = string.Empty;
                string privateKey = string.Empty;
                result = new GrpcConfiguratoin(host, port, isSslMode, serverCert, clientCert, privateKey);
            }
            catch (Exception)
            {
                result = Default;
            }
            return result;
        }
    }
}
