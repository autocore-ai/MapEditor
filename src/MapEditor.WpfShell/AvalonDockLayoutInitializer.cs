using AvalonDock.Layout;
using MapEditor.WpfShell.ViewModels;
using System.Linq;

namespace MapEditor.WpfShell
{
    internal class AvalonDockLayoutInitializer : ILayoutUpdateStrategy
    {
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument anchorableShown)
        {
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            bool isFiexed = false;
            // fix into default group begin
            if (anchorableToShow.ContentId == PropertyViewModel.CONTENT_ID)
            {
                LayoutAnchorGroup sideGroupRight = layout.RightSide.Children.OfType<LayoutAnchorGroup>().FirstOrDefault();
                if (sideGroupRight != null) 
                {
                    sideGroupRight.Children.Add(anchorableToShow);
                    isFiexed = true;
                }
            }
            else if (anchorableToShow.ContentId == MessageViewModel.CONTENT_ID) 
            {
                LayoutAnchorGroup sideGroupBottom = layout.BottomSide.Children.OfType<LayoutAnchorGroup>().FirstOrDefault();
                if (sideGroupBottom != null)
                {
                    sideGroupBottom.Children.Add(anchorableToShow);
                    isFiexed = true;
                }
            }
            // fix into default group end
            if (!isFiexed) 
            {
                LayoutAnchorablePane destPane = destinationContainer as LayoutAnchorablePane;
                if (destinationContainer != null &&
                    destinationContainer.FindParent<LayoutFloatingWindow>() != null)
                {
                    return false;
                }
                var groupPanes = layout.Descendents().OfType<LayoutAnchorGroup>();
                if (groupPanes != null && groupPanes.Any())
                {
                    //layout.BottomSide;
                }
                var toolsPane = layout.Descendents().OfType<LayoutAnchorablePane>().FirstOrDefault(d => d.Name == "ToolsPane");
                if (toolsPane != null)
                {
                    toolsPane.Children.Add(anchorableToShow);
                    return true;
                }
            }
            return isFiexed;
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument anchorableToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }
    }
}
