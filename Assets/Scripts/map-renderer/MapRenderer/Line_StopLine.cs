using UnityEngine;

namespace MapRenderer
{
    public class Line_StopLine : Line
    {
        private Sign controlSign;
        public Sign ControlSign
        {
            get { return controlSign; }
            set 
            { 
                controlSign = value;
                if(controlSign != null)
                {
                    controlSign.targetWay = this;
                }
            }
        }
        public LineRenderer ConnectControlSign;
        public override void OnSelected()
        {
            base.OnSelected();
            if (ConnectControlSign == null)
            {
                if (ControlSign != null)
                {
                    ConnectControlSign = gameObject.AddComponent<LineRenderer>();
                    Vector3[] vector3s = new Vector3[2];
                    vector3s[0] = (points[0].Position + points[1].Position) * 0.5f;
                    vector3s[1] = ControlSign.position;
                    ConnectControlSign.SetPositions(vector3s);
                }
            }
            else
            {
                ConnectControlSign.enabled = true;
            }
        }
        public override void CancelSelect()
        {
            base.CancelSelect();
            if(ConnectControlSign!=null) ConnectControlSign.enabled = false;
        }
        public override void Start()
        {
            base.Start();
            color = Color.red;
        }
        public override void UpdateElementData()
        {
            AddOrEditTag("type", "stop_line");
            AddOrEditTag("subtype", "solid");
        }
    }
}
