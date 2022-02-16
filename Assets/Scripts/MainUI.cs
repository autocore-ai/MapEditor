using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public InputField inputFieldCount;
    public Button buttonParticle;
    public Button buttonMesh;
    public Text textMessage;
    public Text fps;
    // Start is called before the first frame update
    void Start()
    {
        buttonParticle?.onClick.AddListener(()=> 
        {
            int count = int.Parse(inputFieldCount.text);
        });
        buttonMesh?.onClick.AddListener(() =>
        {
            int count = int.Parse(inputFieldCount.text);
        }); 
    }
    int frame=0;
    float timeTemp;
    // Update is called once per frame
    void Update()
    {
        frame +=1;
        timeTemp += Time.deltaTime;
        if (timeTemp >= 1)
        {
            fps.text = frame.ToString();
            frame = 0;
            timeTemp = 0;
        }
    }
    public void SetMessage(string str)
    {
        textMessage.text = str;
    }
}
