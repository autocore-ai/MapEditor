using Packages.BezierCurveEditorPackage.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BZHelperPanel : MonoBehaviour
{
    public GameObject gOBZPointHelper;
    public List<BZPointHelper> bZPointHelpers = new List<BZPointHelper>();
    public BezierCurve bezierCurve;
    public Action OnBezierEdit;
    public bool IsShowHelper=false;

    public void PanelInit(BezierCurve curve = null)
    {
        bezierCurve = curve;
        if (bezierCurve == null || bezierCurve.pointCount == 0) 
        {
            IsShowHelper = false;
            return; 
        }
        while ( bZPointHelpers.Count<bezierCurve.pointCount)
        {
            BZPointHelper newPoint = Instantiate(gOBZPointHelper,transform).GetComponent<BZPointHelper>();
            bZPointHelpers.Add(newPoint);
        } 
        while (bZPointHelpers.Count > bezierCurve.pointCount)
        {
            BZPointHelper lastPoint = bZPointHelpers[bZPointHelpers.Count - 1];
            bZPointHelpers.Remove(lastPoint);
            Destroy(lastPoint.gameObject);
        }
        for (int i = 0; i < bezierCurve.pointCount; i++)
        {
            bZPointHelpers[i].targetPoint = bezierCurve[i];
        }
        HelperUI[] helpers = GetComponentsInChildren<HelperUI>();
        foreach (BZPointHelper helper in bZPointHelpers)
        {
            helper.HelperUIInit();
        }
        IsShowHelper = true;
    }
    public void PanelClear()
    {
        bezierCurve = null;
        while (bZPointHelpers.Count > 0)
        {
            BZPointHelper lastPoint = bZPointHelpers[0];
            bZPointHelpers.Remove(lastPoint);
            Destroy(lastPoint.gameObject);
        }
        IsShowHelper=false;
    }
    public void PanelUpdate()
    {
        if (!IsShowHelper) return;
        if (bezierCurve == null)
        {
            while (bZPointHelpers.Count > 0)
            {
                Destroy(bZPointHelpers[0].gameObject);
            }
            return;
        }
        //if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();
        for (int i = 0; i < bezierCurve.pointCount; i++)
        {
            bZPointHelpers[i].targetPoint = bezierCurve[i];
        }
        foreach (BZPointHelper helper in bZPointHelpers)
        {
            helper.HelperUIInit();
        }

    }
    public void PanelUpdate(BezierCurve curve)
    {
        bezierCurve=curve;
        PanelUpdate();
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
