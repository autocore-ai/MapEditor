using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolPanel : MonoBehaviour
{
    public bool isOpenTool = false;
    public Button buttonFile;
    public Button buttonHelp;
    public Button buttonLoadPCD;
    public Button buttonLoadOSM;
    public Button buttonSave;
    public Button buttonSaveAs;
    public GameObject objFile;
    public GameObject objHelp;

    // Start is called before the first frame update
    void Start()
    {
        buttonFile?.onClick.AddListener(() =>
        {
            if (objFile.activeSelf) ClosePanelTool();
            else
            {
                isOpenTool = true;
                CloseAllTool();
                objFile.SetActive(isOpenTool);
            }
        });
        buttonHelp?.onClick.AddListener(() =>
        {
            if (objHelp.activeSelf) ClosePanelTool();
            else
            {
                isOpenTool = true;
                CloseAllTool();
                objHelp.SetActive(isOpenTool);
            }
        });
        buttonLoadPCD?.onClick.AddListener(() =>
        {
            ClosePanelTool();
            FileManager.Instance.LoadPCDFile();
        });
        buttonLoadOSM?.onClick.AddListener(() =>
        {
            ClosePanelTool();

            FileManager.Instance.LoadOSMFile();
        });
        buttonSave?.onClick.AddListener(() =>
        {
            ClosePanelTool();
        });
        buttonSaveAs?.onClick.AddListener(() =>
        {
            ClosePanelTool();
            FileManager.Instance.OpenSaveFileDialog();
        });
        ClosePanelTool();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void SwitchOpenTool()
    {
        isOpenTool = !isOpenTool;
        if (isOpenTool == false)
        {
            CloseAllTool();
        }
    }
    public void ClosePanelTool()
    {
        isOpenTool = false;
        CloseAllTool();
    }
    private void CloseAllTool()
    {
        objFile.SetActive(false);
        objHelp.SetActive(false);
    }
}
