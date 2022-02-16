using Packages.BezierCurveEditorPackage.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BZHandlerHelper : HelperUI
{
    public BZPointHelper bZPointHelper;

    public override void HelperUIInit()
    {
        base.HelperUIInit();
        if(bZPointHelper==null) bZPointHelper =GetComponentInParent<BZPointHelper>();
        rectTransform.position = CameraManager.Instance.m_camera.WorldToScreenPoint(bZPointHelper.targetPoint.position+ bZPointHelper.targetPoint.handle2);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        bZPointHelper.SetHandle(CameraManager.Instance.m_camera.ScreenToWorldPoint(rectTransform.position));

        UIManager.Instance.bZHelperPanel.OnBezierEdit.Invoke();
    }
    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
    }

}
