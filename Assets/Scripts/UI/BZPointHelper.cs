using MapRenderer;
using Packages.BezierCurveEditorPackage.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BZPointHelper : HelperUI
{
    public BezierPoint targetPoint;
    public BZHandlerHelper handlerHelper;
    LineRenderer lineRenderer;

    protected Vector3 screenPosDragStart;
    public override void HelperUIInit()
    {
        base.HelperUIInit();
        if(handlerHelper==null) handlerHelper=GetComponentInChildren<BZHandlerHelper>();
        rectTransform.position =CameraManager.Instance.m_camera.WorldToScreenPoint(targetPoint.position);
        handlerHelper.HelperUIInit();
        UpdateLineRenderer();
    }
    private void UpdateLineRenderer()
    {
        if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, targetPoint.position);
        lineRenderer.SetPosition(1, targetPoint.position + targetPoint.handle2);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        Vector3 groundPos = MapElement.Absorb2Ground(CameraManager.Instance.m_camera.ScreenToWorldPoint(rectTransform.position));
        targetPoint.position = groundPos; 
        UIManager.Instance.bZHelperPanel.OnBezierEdit.Invoke();
        UpdateLineRenderer();
    }
    public void SetHandle(Vector3 handlePos)
    {
        Vector3 groundPos = MapElement.Absorb2Ground(handlePos)-CameraManager.Instance.m_camera.ScreenToWorldPoint(rectTransform.position); 
        UpdateLineRenderer();
        targetPoint.handle2= groundPos;
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        screenPosDragStart= CameraManager.Instance.m_camera.WorldToScreenPoint(targetPoint.position);
    }

}
