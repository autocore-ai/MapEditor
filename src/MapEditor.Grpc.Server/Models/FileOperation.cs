namespace MapEditor.Grpc 
{
    public class FileOperation 
    {
        public FileOperateType Operation 
        {
            get;
            set;
        }

        public string FilePath 
        {
            get;
            set;
        }

        public FileOperation() 
        { 
        }
        public FileOperation(FileOperateType operation, string fileName) 
        { 
            Operation = operation;
            FilePath = fileName;
        }

        public override string ToString() 
        {
            return $"FileOperation Type : {Operation}, FilePath : {FilePath}";//base.ToString();
        }

        public static implicit operator OperateFileAction(FileOperation operation) 
        {
            return new OperateFileAction()
            {
                OperateType = (OperateFileAction.Types.OperateFileType)operation.Operation,
                FilePath = operation.FilePath
            };
        }
        public static implicit operator FileOperation(OperateFileAction action) 
        { 
            return new FileOperation((FileOperateType)action.OperateType, action.FilePath);
        }
    }

    public enum FileOperateType : int 
    {
        LoadPCD = 0,
        LoadOSM = 1,
        SaveOSM = 2
    }
}
