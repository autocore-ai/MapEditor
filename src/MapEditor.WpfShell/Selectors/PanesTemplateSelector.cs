using MapEditor.WpfShell.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace MapEditor.WpfShell.Selectors 
{
    internal class PanesTemplateSelector : DataTemplateSelector 
    {
        public DataTemplate MapEditorTemplate 
        {
            get;
            set;
        }
        public DataTemplate MessagePaneTemplate 
        {
            get;
            set;
        }
        public DataTemplate PropertyPaneTemplate 
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) 
        {
            if (item is MapEditorViewModel) 
            {
                return MapEditorTemplate;
            }
            if (item is MessageViewModel) 
            {
                return MessagePaneTemplate;
            }
            return base.SelectTemplate(item, container);
        }
    }
}
