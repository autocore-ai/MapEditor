using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MapRenderer
{
    public class Line_WhiteLine : Line
    {
        public override void Start()
        {
            base.Start();
            color = Color.white;

        }
        public override void ElementEdit()
        {
            base.ElementEdit();
        }
        public override void UpdateElementData()
        {
            base.UpdateElementData();
            AddOrEditTag("type", "line_thin");
            AddOrEditTag("subtype", "solid");
        }
    }
}
