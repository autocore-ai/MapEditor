using MapRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{
    //public Button buttonCopy;
    //public Text messageText;
    public Text textMousePos;
    public Text textMode;
    public Text textBezierMode;
    public Text textFPS;
    // Start is called before the first frame update
    void Start()
    {
        //buttonCopy?.onClick.AddListener(() =>
        //{
        //    if (messageText != null)
        //        GUIUtility.systemCopyBuffer = messageText.text;
        //});
    }
    int frameCount;
    float frameTime;
    private void Update()
    {
        frameCount++;
        frameTime +=Time.deltaTime;
        if (frameTime > 1)
        {
            ShowFPS(frameCount.ToString());
            frameCount = 0;
            frameTime = 0;
            ShowMousePos();
            ShowMode();
            ShowBezierMode();
        }

    }
    //public void ShowMessage(string str)
    //{
    //    messageText.text = str;
    //}
    public void ShowMousePos()
    {
        textMousePos.text=CameraManager.Instance.MousePos.x.ToString("0.0")+","+ CameraManager.Instance.MousePos.y.ToString("0.0") + ","+CameraManager.Instance.MousePos.z.ToString("0.0");
    }
    public void ShowMode()
    {
        textMode.text="EditMode:"+ EditManager.Instance.editMode.ToString();
    }
    public void ShowBezierMode()
    {
        textBezierMode.text = "BezierMode:" + EditManager.Instance.IsEditWithBezier.ToString();
    }
    public void ShowFPS(string fps)
    {
        textFPS.text = fps;
    }
}
 