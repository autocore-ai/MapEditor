using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MapEditor.Controls 
{
    /// <summary>
    /// BusyIndicator
    /// </summary>
    [StyleTypedProperty(Property = "ProgressBarStyle", StyleTargetType = typeof(ProgressBar))]
    [StyleTypedProperty(Property = "OverlayStyle", StyleTargetType = typeof(Rectangle))]
    [TemplateVisualState(Name = "Hidden", GroupName = "VisibilityStates")]
    [TemplateVisualState(Name = "Visible", GroupName = "VisibilityStates")]
    [TemplateVisualState(Name = "Idle", GroupName = "BusyStatusStates")]
    [TemplateVisualState(Name = "Busy", GroupName = "BusyStatusStates")]
    public class BusyIndicator : ContentControl 
    {
        private DispatcherTimer m_DisplayAfterTimer;

        #region DependencyProperties

        public static readonly DependencyProperty IsBusyProperty =
            DependencyProperty.Register(
                "IsBusy",
                typeof(bool),
                typeof(BusyIndicator),
                new PropertyMetadata(false, new PropertyChangedCallback(OnIsBusyChanged)));

        public static readonly DependencyProperty BusyContentProperty =
            DependencyProperty.Register(
                "BusyContent",
                typeof(object),
                typeof(BusyIndicator),
                new PropertyMetadata(null));

        public static readonly DependencyProperty BusyContentTemplateProperty =
            DependencyProperty.Register(
                "BusyContentTemplate",
                typeof(DataTemplate),
                typeof(BusyIndicator),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DisplayAfterProperty =
            DependencyProperty.Register(
                "DisplayAfter",
                typeof(TimeSpan),
                typeof(BusyIndicator),
                new PropertyMetadata(TimeSpan.FromSeconds(0.1)));

        public static readonly DependencyProperty OverlayStyleProperty =
            DependencyProperty.Register(
                "OverlayStyle",
                typeof(Style),
                typeof(BusyIndicator),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ProgressBarStyleProperty =
            DependencyProperty.Register(
                "ProgressBarStyle",
                typeof(Style),
                typeof(BusyIndicator),
                new PropertyMetadata(null));

        #endregion

        #region Properties

        protected bool IsContentVisible
        {
            get;
            set;
        }

        public bool IsBusy
        {
            get
            {
                return (bool)GetValue(IsBusyProperty);
            }
            set
            {
                SetValue(IsBusyProperty, value);
            }
        }

        public object BusyContent
        {
            get
            {
                return GetValue(BusyContentProperty);
            }
            set
            {
                SetValue(BusyContentProperty, value);
            }
        }

        public DataTemplate BusyContentTemplate
        {
            get
            {
                return (DataTemplate)GetValue(BusyContentTemplateProperty);
            }
            set
            {
                SetValue(BusyContentTemplateProperty, value);
            }
        }

        public TimeSpan DisplayAfter
        {
            get
            {
                return (TimeSpan)GetValue(DisplayAfterProperty);
            }
            set
            {
                SetValue(DisplayAfterProperty, value);
            }
        }

        public Style OverlayStyle
        {
            get
            {
                return (Style)GetValue(OverlayStyleProperty);
            }
            set
            {
                SetValue(OverlayStyleProperty, value);
            }
        }

        public Style ProgressBarStyle
        {
            get
            {
                return (Style)GetValue(ProgressBarStyleProperty);
            }
            set
            {
                SetValue(ProgressBarStyleProperty, value);
            }
        }

        #endregion

        #region Constructors

        static BusyIndicator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BusyIndicator), new FrameworkPropertyMetadata(typeof(BusyIndicator)));
        }

        public BusyIndicator()
        {
            m_DisplayAfterTimer = new DispatcherTimer();
            Loaded += delegate
            {
                m_DisplayAfterTimer.Tick += new EventHandler(DisplayAfterTimerElapsed);
            };
            Unloaded -= delegate
            {
                m_DisplayAfterTimer.Tick -= new EventHandler(DisplayAfterTimerElapsed);
                m_DisplayAfterTimer.Stop();
            };
        }

        #endregion

        #region Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ChangeVisualState(false);
        }

        #endregion

        #region methods

        private void DisplayAfterTimerElapsed(object sender, EventArgs e)
        {
            m_DisplayAfterTimer.Stop();
            IsContentVisible = true;
            ChangeVisualState(true);
        }

        private static void OnIsBusyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BusyIndicator)d).OnIsBusyChanged(e);
        }

        protected virtual void OnIsBusyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsBusy)
            {
                if (DisplayAfter.Equals(TimeSpan.Zero))
                {
                    IsContentVisible = true;
                }
                else
                {
                    m_DisplayAfterTimer.Interval = DisplayAfter;
                    m_DisplayAfterTimer.Start();
                }
            }
            else
            {
                m_DisplayAfterTimer.Stop();
                IsContentVisible = false;
            }
            ChangeVisualState(true);
        }

        protected virtual void ChangeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsBusy ? "Busy" : "Idle", useTransitions);
            VisualStateManager.GoToState(this, IsContentVisible ? "Visible" : "Hidden", useTransitions);
        }

        #endregion
    }
}
