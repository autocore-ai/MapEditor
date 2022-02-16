using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewPanel : MonoBehaviour
{
    public Button buttonSwitchProjection;
    public Button button_X;
    public Button button_Y;
    public Button button_Z;
    public GameObject goFOV;
    public GameObject goSize;
    public Slider sliderFov;
    public Slider sliderSize;
    public Text text_fov;
    public Text text_size;
    float camFOV = 60;
    float camSize = 20;
    // Start is called before the first frame update
    void Start()
    {
        sliderFov?.onValueChanged.AddListener(SetCameraFOV);
        sliderSize?.onValueChanged.AddListener(SetCameraSize);
        sliderFov.value = camFOV;
        sliderSize.value = camSize;
        buttonSwitchProjection?.onClick.AddListener(() =>
        {
            SwitchCameraProjection();
        });
        button_X?.onClick.AddListener(() => 
        {
            CameraManager.Instance.cameraMove.CameraAlignWithX(CameraManager.Instance.TargetPos);
        });
        button_Y?.onClick.AddListener(() => 
        {
            CameraManager.Instance.cameraMove.CameraAlignWithY(CameraManager.Instance.TargetPos);
        });
        button_Z?.onClick.AddListener(() => 
        {
            CameraManager.Instance.cameraMove.CameraAlignWithZ(CameraManager.Instance.TargetPos);
        });



        SwitchCameraProjection();
    }
    public void SetCameraFOV(float value)
    {
        camFOV = value;
        CameraManager.Instance.SetCameraFov(value);
        text_fov.text = value.ToString("0.00");
        CameraManager.Instance.OnCameraMove?.Invoke();
    }
    public void SetCameraSize(float value)
    {
        camSize = value;
        CameraManager.Instance.SetCameraSize(value);
        text_size.text = value.ToString("0.00");
        CameraManager.Instance.OnCameraMove?.Invoke();
    }
    void SwitchCameraProjection()
    {
        CameraManager.Instance.SwitchProjection(out bool isOrth);
        goSize.gameObject.SetActive(isOrth);
        goFOV.gameObject.SetActive(!isOrth);
        CameraManager.Instance.SetCameraFov(camFOV);
        CameraManager.Instance.SetCameraSize(camSize);
    }
    public void AdjustCameraSize(float scale)
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
