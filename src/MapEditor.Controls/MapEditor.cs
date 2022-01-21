using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace MapEditor.Controls
{
    /// <summary>
    /// Load/Unload Unity Control
    /// </summary>
    [StyleTypedProperty(Property = PART_NAME_WINFORM_HOST, StyleTargetType = typeof(WindowsFormsHost))]
    public class MapEditor : Control
    {
        internal const string PART_NAME_WINFORM_HOST = "WindowsFormHost";

        #region fields

        protected MapControl m_MapControl;

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty MapEditorUriProperty;

        #endregion

        #region RoutedEvents

        public static readonly RoutedEvent ErrorEvent = EventManager.RegisterRoutedEvent("Error", RoutingStrategy.Direct, typeof(EventHandler<ExceptionRoutedEventArgs>), typeof(MapEditor));
        public static readonly RoutedEvent ClosingEvent = EventManager.RegisterRoutedEvent("Closing", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(MapEditor));
        public static readonly RoutedEvent ClosedEvent = EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Direct, typeof(EventHandler), typeof(MapEditor));

        #endregion

        #region Properties

        public string MapEditorUri 
        {
            get 
            { 
                return (string)GetValue(MapEditorUriProperty);
            }
            set 
            { 
                SetValue(MapEditorUriProperty, value);
            }
        }

        #endregion

        #region Events

        public event EventHandler<ExceptionRoutedEventArgs> Error
        {
            add 
            { 
                AddHandler(ErrorEvent, value);
            }
            remove 
            { 
                RemoveHandler(ErrorEvent, value);
            }
        }
        public event RoutedEventHandler Closing 
        {
            add 
            { 
                AddHandler(ClosingEvent, value);
            }
            remove 
            { 
                RemoveHandler(ClosingEvent, value);
            }
        }
        public event RoutedEventHandler Closed 
        { 
            add 
            { 
                AddHandler(ClosedEvent, value);
            }
            remove 
            { 
                RemoveHandler(ClosedEvent, value);
            }
        }

        #endregion

        static MapEditor() 
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapEditor), new FrameworkPropertyMetadata(typeof(MapEditor)));
            MapEditorUriProperty = DependencyProperty.Register("MapEditorUri", typeof(string), typeof(MapEditor), new PropertyMetadata(null, new PropertyChangedCallback(OnMapEditorUriChanged)));
        }
        public MapEditor() 
        {
        }

        public override void OnApplyTemplate() 
        {
            base.OnApplyTemplate();
            if (m_MapControl != null) 
            {
                m_MapControl.Error -= OnMapError;
                m_MapControl.Dispose();
                m_MapControl = null;
            }
            WindowsFormsHost winformHost = GetTemplateChild(PART_NAME_WINFORM_HOST) as WindowsFormsHost;
            if (winformHost != null) 
            {
                m_MapControl = new MapControl();
                m_MapControl.Error += OnMapError;
                winformHost.Child = m_MapControl;
                if (!string.IsNullOrEmpty(MapEditorUri)) 
                {
                    m_MapControl.LoadUnityControl(MapEditorUri);
                }
            }
        }
        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) 
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (m_MapControl != null) 
            {
                m_MapControl.Resize((int)sizeInfo.NewSize.Width, (int)sizeInfo.NewSize.Height);
            }
        }

        protected void OnMapEditorUriChanged(string uriOld, string uriNew) 
        {
            if (m_MapControl == null) 
            {
                // TODO: Error
                return;
            }
            if (!string.IsNullOrEmpty(uriOld)) 
            {
                m_MapControl.UnloadUnityControl();
            }
            if (!string.IsNullOrEmpty(uriNew)) 
            {
                m_MapControl.LoadUnityControl(uriNew);
            }
            m_MapControl.Resize((int)ActualWidth, (int)ActualHeight);
        }
        protected void OnMapError(object sender, ErrorEventArgs args) 
        {
            Type t = typeof(ExceptionRoutedEventArgs);
            System.Reflection.ConstructorInfo[] constructors = t.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (constructors.Length != 1) 
            {
                return;
            }
            ExceptionRoutedEventArgs arg = (ExceptionRoutedEventArgs)constructors[0].Invoke(new object[3] { ErrorEvent, this, args.ErrorException });
            FireMapError(arg);
        }
        private void FireMapError(ExceptionRoutedEventArgs e) 
        {
            e.RoutedEvent = ErrorEvent;
            RaiseEvent(e);
        }

        public static void OnMapEditorUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) 
        {
            MapEditor mapEditor = d as MapEditor;
            if (mapEditor != null) 
            {
                mapEditor.OnMapEditorUriChanged((string)e.OldValue, (string)e.NewValue);
            }
        }
    }
}
