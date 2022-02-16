using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public MessagePanel messagePanel;
    public ToolPanel toolPanel;
    public ViewPanel viewPanel;
    public BZHelperPanel bZHelperPanel;
    public DragHelperPanel dragHelperPanel;
    private void Awake()
    {
        Instance = this;
        if(messagePanel==null) messagePanel=GetComponentInChildren<MessagePanel>();
        if (toolPanel == null) toolPanel = GetComponentInChildren<ToolPanel>();
        if (viewPanel == null) viewPanel = GetComponentInChildren<ViewPanel>();
        if (bZHelperPanel == null) bZHelperPanel = GetComponentInChildren<BZHelperPanel>();
        if (dragHelperPanel == null) dragHelperPanel = GetComponentInChildren<DragHelperPanel>();
    }
    void Start()
    {
        
    }
}
