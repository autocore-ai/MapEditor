using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DragHelperPanel : MonoBehaviour
{
    public Vector3 dragOffset;
    public DragHelper dragHelper;
    public bool isShowHelper = false;
    public Vector3 posDrag;
    public Action<Vector3> OnHelperDrag;
    private void Awake()
    {
        dragHelper = GetComponentInChildren<DragHelper>();
    }
    private void Start()
    {
        HideDragHelper();
        OnHelperDrag += EditManager.Instance.MoveSelectedElements;
    }
    public void PanelInit(Vector3 pos)
    {
        posDrag = pos;
        isShowHelper = true;
        dragHelper.gameObject.SetActive(isShowHelper);
        dragHelper.posDragStart = pos;
        dragHelper.HelperUIInit();
    }
    public void HideDragHelper()
    {
        isShowHelper = false;
        dragHelper.gameObject.SetActive(isShowHelper);
    }
    public void PanelUpdate()
    {
        if (isShowHelper)
        {
            dragHelper.HelperUIInit();
        }
    }
}
