using MapRenderer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand : BaseCommand
{
    private List<MapElement> targets;
    private Vector3 offset;
    //我们在构造函数里直接传进来我们改变状态前的Transform信息
    public MoveCommand(List<MapElement> target, Vector3 pos)
    {
        this.targets = target;
        this.offset = pos;
    }
    public override void ExecuteCommand()
    {
        base.ExecuteCommand();
    }
    public override void RevocationCommand()
    {
        foreach (MapElement target in targets)
        {
            target.MoveElement(-offset);
        }
    }
}
