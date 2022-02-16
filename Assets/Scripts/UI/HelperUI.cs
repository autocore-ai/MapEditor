using MapRenderer;
using Packages.BezierCurveEditorPackage.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HelperUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// 点击点与UI的的偏移量
    /// </summary>
    private Vector2 offsetPos;


    protected RectTransform rectTransform;
    public virtual void HelperUIInit()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
    }
    public virtual void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = eventData.position - offsetPos;
        rectTransform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        offsetPos = eventData.position - (Vector2)transform.position;
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
    }
}
