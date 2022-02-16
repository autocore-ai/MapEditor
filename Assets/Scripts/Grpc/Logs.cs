
using System;
using System.IO;

namespace ACGrpcServer
{
    public class Logs
    {
        public static string[] LogPaths = { "./log" };
        public void Println(string param)
        {
            string sFilePath = LogPaths[0];
            string sFileName = DateTime.Now.ToString("yyyyMMdd") + ".txt";
            //文件的绝对路径
            sFileName = Path.Combine(sFilePath, sFileName);
            //验证路径是否存在,不存在则创建
            if (!Directory.Exists(LogPaths[0]))
            {
                Directory.CreateDirectory(sFilePath);
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(sFileName))
            //验证文件是否存在，有则追加，无则创建
            {
                fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            //日志内容
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ----> " + param);
            sw.Close();
            fs.Close();
        }
    }
}