using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;

[RequireComponent(typeof(Camera))]
public class CameraMove : MonoBehaviour
{
    /// <summary>
    /// 平移的指数增强因子，可通过鼠标滚轮控制
    /// </summary>
    [Range(0.001f, 10f)]
    public float boost = 3.5f;
    /// <summary>
    /// 位置插值速度
    /// </summary>
    [Range(0.001f, 1f)]
    public float positionLerpTime = 0.2f;

    /// <summary>
    /// 鼠标灵敏度
    /// </summary>
    public AnimationCurve mouseSensitivityCurve = new AnimationCurve(new Keyframe(0f, 0.5f, 0f, 5f), new Keyframe(1f, 2.5f, 0f, 0f));

    /// <summary>
    /// 旋转插值速度
    /// </summary>
    public float rotationLerpTime = 0.01f;

    public Vector3 transRotation;

    private void Start()
    {
        transRotation = transform.rotation.eulerAngles;
    }



    Vector3 GetInputTranslationDirection()
    {
        Vector3 direction = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            direction += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            direction += Vector3.left;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction += Vector3.back;
        }
        if (Input.GetKey(KeyCode.D))
        {
            direction += Vector3.right;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            direction += Vector3.down;
        }
        if (Input.GetKey(KeyCode.E))
        {
            direction += Vector3.up;
        }
        return direction;
    }

    private Vector3 cameraTargetPos;
    private Vector3 cameraTargetRot;
    private float moveTime = 0.8f;
    private float tempTime = 0;
    private bool isMove = false;
    private bool isRotate = false;
    private bool isPressALT = false;
    public void SetCameraTargetPos(Vector3 pos)
    {
        cameraTargetPos = pos;
    }
    public void MoveCameraToTarget(Vector3 target)
    {
        cameraTargetPos = target;
        isMove = true;
    }
    public void RotateCameraToTarget(Vector3 target)
    {
        cameraTargetRot = target;
        isRotate = true;
    }
    private void Update()
    {
        CameraManager.Instance.IsMovingCamera = false;
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            Cursor.lockState = CursorLockMode.Locked;
            isPressALT = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            isPressALT = false;
        }
        if (isMove || isRotate)
        {
            CameraManager.Instance.IsMovingCamera = true;
            tempTime += Time.deltaTime;
            if (isMove) transform.position = Vector3.Lerp(transform.position, cameraTargetPos, tempTime / moveTime);
            if (isRotate) transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(cameraTargetRot), tempTime / moveTime);
            if (tempTime > moveTime)
            {
                tempTime = 0;
                isMove = false;
                isRotate = false;
            }
            CameraManager.Instance.OnCameraMove.Invoke();
        }
        if (isPressALT)
        {
            CameraManager.Instance.IsMovingCamera = true;
            if (Input.GetMouseButton(0))
            {
                transform.RotateAround(cameraTargetPos, Vector3.up, Input.GetAxis("Mouse X") * 5);
                transform.RotateAround(cameraTargetPos, transform.right, -Input.GetAxis("Mouse Y") * 5);
                CameraManager.Instance.OnCameraMove.Invoke();
            }
            if (Input.GetMouseButton(1))
            {
                float distance = Vector3.Distance(cameraTargetPos, transform.position);
                if (distance > 0.1)
                {
                    transform.Translate(0.05f*Input.GetAxis("Mouse X")*distance * Vector3.forward);
                }
                CameraManager.Instance.OnCameraMove.Invoke();
            }
        }

        // 处理旋转
        if (!isPressALT&&Input.GetMouseButton(1))
        {
            Vector2 mouseMovement = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y"));

            float mouseSensitivityFactor = mouseSensitivityCurve.Evaluate(mouseMovement.magnitude);

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + mouseSensitivityFactor * new Vector3(mouseMovement.y, mouseMovement.x, 0));

            if(mouseMovement!=Vector2.zero) CameraManager.Instance.OnCameraMove.Invoke();

            CameraManager.Instance.IsMovingCamera = true;
        }
        Vector3 translation = Vector3.zero;
        translation = GetInputTranslationDirection();
        //处理移动
        if (Input.GetMouseButton(2))
        {
            CameraManager.Instance.IsMovingCamera = true;
            translation -= new Vector3(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"), 0) * 10;
        }
        translation *= Mathf.Pow(2.0f, boost) * Time.deltaTime;
        transform.Translate(translation);
        if(translation!=Vector3.zero) CameraManager.Instance.OnCameraMove.Invoke();
        //interpolating.ChangeFieldView(-Input.mouseScrollDelta.y);
        CameraManager.Instance.ChangeCameraSize(-Input.mouseScrollDelta.y);
    }
    public void CameraFocusAtTarget(Vector3 targetPos)
    {
        MoveCameraToTarget(targetPos - transform.forward * 10);
    }
    public void CameraAlignWithX(Vector3 targetPos)
    {
        float dis = Vector3.Distance(transform.position, targetPos);
        bool isBigger = targetPos.x < transform.position.x;
        Vector3 pos = targetPos + dis * (isBigger ? new Vector3(1, 0, 0) : new Vector3(-1, 0, 0));
        Vector3 rot = isBigger ? new Vector3(0, -90, 0) : new Vector3(0, 90, 0);

        MoveCameraToTarget(pos);
        RotateCameraToTarget(rot);

    }
    public void CameraAlignWithY(Vector3 targetPos)
    {
        float dis = Vector3.Distance(transform.position, targetPos);
        bool isBigger = targetPos.y < transform.position.y;
        Vector3 pos = targetPos + dis * new Vector3(0, 1, 0);
        Vector3 rot = new Vector3(90, 0, 0);

        MoveCameraToTarget(pos);
        RotateCameraToTarget(rot);
    }
    public void CameraAlignWithZ(Vector3 targetPos)
    {
        float dis = Vector3.Distance(transform.position, targetPos);
        bool isBigger = targetPos.z < transform.position.z;
        Vector3 pos = targetPos + dis * (isBigger ? new Vector3(0, 0, 1) : new Vector3(0, 0, -1));
        Vector3 rot = isBigger ? new Vector3(0, 180, 0) : new Vector3(0, 0, 0);

        MoveCameraToTarget(pos);
        RotateCameraToTarget(rot);
    }
}