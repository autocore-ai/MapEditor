using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class TestFile : MonoBehaviour
{
    [DllImport("_Internal")]
    private static extern void clickSelectFileBtn();
    /// <summary>
    /// 点击Open按钮
    /// </summary>
    public static void ClickSelectFileBtn()
    {
        clickSelectFileBtn();
    }
    public Button button;
    public Text text;
    private void Start()
    {
        button.onClick.AddListener(clickSelectFileBtn);
    }

    public void GetBase64(string base64Str)
    {
        byte[] bs = Convert.FromBase64String(base64Str);
        Debug.Log(base64Str+"lenth:"+bs.Length);
        text.text = base64Str + "lenth:" + bs.Length;
    }
}
