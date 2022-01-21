namespace MapEditor.Grpc 
{
    /// <summary>
    /// from ui(MapEdition) to server(EditMapAction)
    /// </summary>
    public class MapEdition 
    {
        public EditType EditType 
        {
            get;
            set;
        }

        public MapEdition() 
        { 
        }
        public MapEdition(EditType editType) 
        { 
            EditType = editType;
        }
        public override string ToString() 
        {
            return $"MapEdition EditType : {EditType}"; //base.ToString();
        }

        public static implicit operator EditMapAction(MapEdition edition) 
        {
            return new EditMapAction() 
            { 
                EditType = (EditMapAction.Types.EditMapType)edition.EditType
            };
        }
        public static implicit operator MapEdition(EditMapAction action) 
        {
            return new MapEdition((EditType)action.EditType);
        }
    }

    public enum EditType : int 
    { 
        Exit = 0,
        Back = 1,
        Redo = 2
    }
}
