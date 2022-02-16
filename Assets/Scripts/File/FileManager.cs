using MapRenderer;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    public static FileManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    public string tempFilePath;
    public FileInfo CurrentFile;
    private void Start()
    {
        if (CurrentFile == null)
        {
            tempFilePath = Path.Combine(Application.streamingAssetsPath, "Temp.osm");
            SaveFile2Path(tempFilePath, "", true);
            CurrentFile = new FileInfo(tempFilePath);
        }
    }
    /// <summary>
    /// 打开windows保存窗口
    /// </summary>
    /// <returns>路径名（带名字）</returns>
    public void LoadPCDFile()
    {
        FileOpenDialog FOD = new FileOpenDialog();
        FOD.structSize = Marshal.SizeOf(FOD);
        FOD.filter = "\0点云文件 (*.pcd)\0*.pcd";
        FOD.file = new string(new char[256]);
        FOD.maxFile = FOD.file.Length;
        FOD.fileTitle = new string(new char[64]);
        FOD.maxFileTitle = FOD.fileTitle.Length;
        FOD.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
        FOD.title = "保存项目";
        FOD.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetSaveFileName(FOD))
        {
            Debug.Log(FOD.file);
        }
        else
        {
            Debug.Log("no file");
        }
    }
    public void LoadOSMFile()
    {
        FileOpenDialog FOD = new FileOpenDialog();
        FOD.structSize = Marshal.SizeOf(FOD);
        FOD.filter = "\0osm (*.osm)\0*.osm";
        FOD.file = new string(new char[256]);
        FOD.maxFile = FOD.file.Length;
        FOD.fileTitle = new string(new char[64]);
        FOD.maxFileTitle = FOD.fileTitle.Length;
        FOD.initialDir = Application.streamingAssetsPath;//默认路径
        FOD.title = "选择OSM文件";

        //openFileName.defExt = "osm";//是什么文件类型就修改此处
        FOD.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

        if (LocalDialog.GetOFN(FOD))
        {
            MapManager.Instance.LoadMapFromURL(FOD.file);
        }
        else
        {
            Debug.Log("no file");
        }
    }

    public void OpenSaveFileDialog() 
    {
        FileOpenDialog FOD = new FileOpenDialog();
        FOD.structSize = Marshal.SizeOf(FOD);
        FOD.filter = "\0osm (*.osm)";//是什么文件类型就修改此处
        FOD.file = new string(new char[256]);
        FOD.maxFile = FOD.file.Length;
        FOD.fileTitle = new string(new char[64]);
        FOD.maxFileTitle = FOD.fileTitle.Length;
        FOD.initialDir = Application.streamingAssetsPath;//默认路径
        FOD.title = "Save Txt";
        FOD.defExt = "txt";//是什么文件类型就修改此处
        FOD.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (LocalDialog.GetSFN(FOD))
        {
            StartCoroutine(WaitSaveFile(FOD.file));
        }
    }
    IEnumerator WaitSaveFile(string fileName)
    {
        FileInfo fI = new FileInfo(tempFilePath);
        fI.CopyTo(fileName, true);
        yield return fI;
    }

    private void CheckFileExist(string path)
    {
        string dic = Path.GetDirectoryName(path);
        if (!Directory.Exists(dic))
        {
            Directory.CreateDirectory(dic);
        }
        if (!File.Exists(path))
        {
            File.CreateText(path).Dispose();
        }
    }
    public void SaveFile2Path(string path, string content, bool isCover = false)
    {
        CheckFileExist(path);
        if (isCover)
        {
            File.WriteAllText(path, content);
        }
        else
        {
            File.AppendAllText(path, content + "\n");
        }
    }
}
