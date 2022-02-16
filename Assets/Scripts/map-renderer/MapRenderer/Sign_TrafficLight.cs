using UnityEngine;

namespace MapRenderer
{

    public class Sign_TrafficLight : Sign
    {
        public override void ElementUpdateRenderer()
        {
            base.ElementUpdateRenderer();
        }
        public override void Start()
        {
            base.Start();
            AddOrEditTag("type", "traffic_light");
            AddOrEditTag("subtype", "red_yellow_green");
            AddOrEditTag("height", "0.5");
        }
        public void SetTrafficLightHeight(float height)
        {
            AddOrEditTag("height", height.ToString());
        }
    }
}
