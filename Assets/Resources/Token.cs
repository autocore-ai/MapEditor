using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
public class Token : MonoBehaviour
{
    [DllImport("__Internal")]
    //方法名与参数返回值要与jslib里的方法名一模一样
    public static extern void ProvideCallback(Action<string> action);
    void Start()
    {
        ProvideCallback(Callback);
    }
    // 需要加MonoPInvokeCallback 标记
    [MonoPInvokeCallback(typeof(Action<string>))]
    public static void Callback(string arg)
    {
        Debug.Log(arg.ToString());
    }
}