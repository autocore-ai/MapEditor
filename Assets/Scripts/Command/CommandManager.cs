using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandManager : MonoBehaviour
{
    public static CommandManager Instance = null;
    //管理命令
    private Stack<BaseCommand> commandStack = new Stack<BaseCommand>();

    private void Awake()
    {
        Instance = this;
    }


    //增加命令  
    public void AddCommand(BaseCommand baseCommand)
    {
        commandStack.Push(baseCommand);
    }


    //移除命令 并且撤销一步操作
    public void RemoveCommand()
    {
        if (commandStack.Count > 0)
        {
            BaseCommand baseCommand = commandStack.Pop();
            baseCommand.RevocationCommand();
        }
    }
}
