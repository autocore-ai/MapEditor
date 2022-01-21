namespace MapEditor.Grpc 
{
    /// <summary>
    /// from ui(ElementAddition) to server(AddElementAction)
    /// </summary>
    public class ElementAddition 
    {
        public AddingElementType ElementType 
        {
            get;
            set;
        }

        public bool IsAdd 
        {
            get;
            set;
        }

        public ElementAddition() 
        { 
        }
        public ElementAddition(AddingElementType addingType, bool isAdd) 
        { 
            ElementType = addingType;
            IsAdd = isAdd;
        }

        public override string ToString() 
        {
            return $"ElementAddion ElementType : {ElementType}, IsAdding : {IsAdd}"; //base.ToString();
        }

        public static implicit operator AddElementAction(ElementAddition addtion) 
        {
            return new AddElementAction()
            {
                ElementType = (AddElementAction.Types.AddElementType)addtion.ElementType,
                IsAdd = addtion.IsAdd
            };
        }
        public static implicit operator ElementAddition(AddElementAction action) 
        { 
            return new ElementAddition((AddingElementType)action.ElementType, action.IsAdd);
        }
    }

    public enum AddingElementType : int 
    {
        Lanelet = 0,
        WhiteLine = 1,
        StopLine = 2
    }
}
