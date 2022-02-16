using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragHelper : HelperUI
{

    protected Vector3 totalOffset;
    DragHelperPanel panel;
    public Vector3 posDragStart;
    protected Vector3 screenPosDragStart;

    // Start is called before the first frame update
    void Start()
    {
        HelperUIInit();
    }
    public override void HelperUIInit()
    {
        base.HelperUIInit();
        if(panel == null)panel=GetComponentInParent<DragHelperPanel>();
        rectTransform.position = CameraManager.Instance.m_camera.WorldToScreenPoint(posDragStart);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        Vector3 helperPos = CameraManager.Instance.m_camera.ScreenToWorldPoint(rectTransform.position);
        Vector3 offsetDrag = helperPos - posDragStart;
        posDragStart = helperPos;
        panel.OnHelperDrag.Invoke(offsetDrag);
        totalOffset += offsetDrag;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        totalOffset = Vector3.zero;
        screenPosDragStart = CameraManager.Instance.m_camera.WorldToScreenPoint(posDragStart);
    }
    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        EditManager.Instance.SaveMoveCommand(totalOffset);
    }
}
