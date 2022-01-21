namespace MapEditor.Models
{
    /// <summary>
    /// LogInfo of Render
    /// </summary>
    public class RenderLogInfo 
    {
        public LogLevel Level 
        { 
            get;
            set;
        }
        public string Message 
        { 
            get;
            set;
        }

        public RenderLogInfo(int level, string message) 
            : this((LogLevel)level, message) 
        { 
        }
        public RenderLogInfo(LogLevel level, string message) 
        {
            Level = level;
            Message = message;
        }

        public override string ToString() 
        {
            return $"[{Level}] : {Message}";
        }
    }

    public enum LogLevel : int 
    { 
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
    }
}
