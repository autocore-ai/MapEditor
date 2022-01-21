using MapEditor.WpfShell.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MapEditor.WpfShell.Selectors
{
    internal class PanesStyleSelector : StyleSelector 
    {
        public Style MapEditorStyle 
        {
            get;
            set;
        }
        public Style ToolPaneStyle 
        {
            get;
            set;
        }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            if (item is MapEditorViewModel) 
            { 
                return MapEditorStyle;
            }
            if (item is IToolViewModel) 
            {
                return ToolPaneStyle;
            }
            return base.SelectStyle(item, container);
        }
    }
}
