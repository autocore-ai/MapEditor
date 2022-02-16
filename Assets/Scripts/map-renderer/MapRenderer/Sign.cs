using UnityEngine;

namespace MapRenderer
{
    public class Sign : Way
    {
        public Way targetWay;
        public Vector3 position;
        public float height;
        public float width=1;
        public float yaw;
        private Vector3 originScale=Vector3.one;

        private GameObject plane;
        public override void Start()
        {
            originScale=transform.localScale;
            base.Start();
            if (!map.signs.Contains(this)) map.signs.Add(this);
        }
        public override void UpdateElementData()
        {
            base.UpdateElementData();
            AddOrEditTag("type", "traffic_sign");
            AddOrEditTag("height", "0.5");
        }
        public override void ElementUpdateRenderer()
        {
            base.ElementUpdateRenderer();
            position = points[0].Position / 2 + points[1].Position / 2;
            float distance = Vector3.Distance(points[0].Position, points[1].Position);
            width = distance;
            float scale=width/originScale.x;
            transform.position = position;
            transform.localScale = new Vector3(scale, 0.4f* scale, 0.05f);
            transform.rotation = Quaternion.FromToRotation(Vector3.right, points[1].Position - points[0].Position);
        }
    }
}
