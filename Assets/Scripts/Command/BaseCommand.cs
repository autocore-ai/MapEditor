
public class BaseCommand
{
    //命令描述
    private string _commandDescribe;
    public string CommandDescribe
    {
        set
        {
            _commandDescribe = value;
        }
        get
        {
            return _commandDescribe;
        }
    }

    //执行命令
    public virtual void ExecuteCommand()
    {
    }

    //撤销命令
    public virtual void RevocationCommand()
    {
    }
}