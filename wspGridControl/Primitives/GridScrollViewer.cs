using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace wspGridControl.Primitives
{
    public class GridScrollViewer : ScrollViewer
    {
        #region Variables
        internal const string PART_HorizontalScrollBar = "PART_HorizontalScrollBar";
        internal const string PART_VerticalScrollBar = "PART_VerticalScrollBar";
        internal const string PART_ScrollContentPresenter = "PART_ScrollContentPresenter";

        public const double ScrollLineDelta = 16.0;
        private static readonly TimeSpan AutoScrollTimeout = new TimeSpan(1000);

        private GridRowsPresenter _rowsPresenter = null;
        private ScrollBar _horizontalScrollBar = null;
        private ScrollBar _verticalScrollBar = null;

        private ICommand _autoScrollCommand = null;
        private DispatcherTimer _autoScrollTimer;
        #endregion

        #region Events
        public static RoutedEvent TemplateAppliedEvent = EventManager.RegisterRoutedEvent(
            "TemplateApplied", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(GridScrollViewer));

        public event RoutedEventHandler TemplateApplied
        {
            add { AddHandler(TemplateAppliedEvent, value); }
            remove { RemoveHandler(TemplateAppliedEvent, value); }
        }
        public static void AddTemplateAppliedHandler(UIElement el, RoutedEventHandler handler)
        {
            el.AddHandler(TemplateAppliedEvent, handler);
        }
        public static void RemoveTemplateAppliedHandler(UIElement el, RoutedEventHandler handler)
        {
            el.RemoveHandler(TemplateAppliedEvent, handler);
        }
        #endregion

        #region Constructors
        static GridScrollViewer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridScrollViewer), new FrameworkPropertyMetadata(typeof(GridScrollViewer)));

            FocusableProperty.OverrideMetadata(typeof(GridScrollViewer), new FrameworkPropertyMetadata(false));
        }

        public GridScrollViewer()
            : base()
        {
            Focusable = false;
        }
        #endregion

        #region Dependency Properties

        #region IsAutoScrollingProperty
        private static readonly DependencyPropertyKey IsAutoScrollingPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsAutoScrolling", typeof(bool), typeof(GridScrollViewer), new FrameworkPropertyMetadata(false));

        private static readonly DependencyProperty IsAutoScrollingProperty = IsAutoScrollingPropertyKey.DependencyProperty;

        internal bool IsAutoScrolling
        {
            get => (bool)GetValue(IsAutoScrollingProperty);
            private set { SetValue(IsAutoScrollingPropertyKey, value); }
        }
        #endregion

        #endregion

        #region Properties
        internal double HorizontalScrollBarHeight
        {
            get
            {
                if (_horizontalScrollBar != null && _horizontalScrollBar.Visibility != Visibility.Collapsed)
                {
                    return _horizontalScrollBar.ActualHeight;
                }
                return 0.0;
            }
        }

        internal double VerticalScrollBarWidth
        {
            get
            {
                if (_verticalScrollBar != null && _verticalScrollBar.Visibility != Visibility.Collapsed)
                {
                    return _verticalScrollBar.ActualWidth;
                }
                return 0.0;
            }
        }

        internal Rect RowsBounds
        {
            get
            {
                var width = _horizontalScrollBar != null ? _horizontalScrollBar.ActualWidth : 0;
                var height = _verticalScrollBar != null ? _verticalScrollBar.ActualHeight : 0;
                if (width == 0 || height == 0) return Rect.Empty;

                double top = 0.0;

                if (_rowsPresenter != null)
                {
                    Vector offset = VisualTreeHelper.GetOffset(_rowsPresenter);
                    top = offset.Y;
                    height -= top;
                }

                return new Rect(0.0, top, width, height);
            }
        }

        public ICommand AutoScrollCommand
        {
            set { _autoScrollCommand = value; }
        }

        internal RelativeMousePositions RelativeMousePosition
        {
            get
            {
                RelativeMousePositions position = RelativeMousePositions.Over;
                Rect bounds = RowsBounds;
                if (bounds.IsEmpty) return position;

                Point pt = Mouse.GetPosition(this);

                if (DoubleUtil.LessThan(pt.X, bounds.Left))
                {
                    position |= RelativeMousePositions.Left;
                }
                else if (DoubleUtil.GreaterThan(pt.X, bounds.Right))
                {
                    position |= RelativeMousePositions.Right;
                }

                if (DoubleUtil.LessThan(pt.Y, bounds.Top))
                {
                    position |= RelativeMousePositions.Above;
                }
                else if (DoubleUtil.GreaterThan(pt.Y, bounds.Bottom))
                {
                    position |= RelativeMousePositions.Below;
                }

                return position;
            }
        }
        #endregion

        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _rowsPresenter = GetTemplateChild(GridControl.PART_RowsPresenter) as GridRowsPresenter;
            _horizontalScrollBar = GetTemplateChild(PART_HorizontalScrollBar) as ScrollBar;
            _verticalScrollBar = GetTemplateChild(PART_VerticalScrollBar) as ScrollBar;

            RaiseEvent(new RoutedEventArgs(TemplateAppliedEvent, this));
        }

        internal T GetTemplateChild<T>(string childName) where T : DependencyObject
        {
            return GetTemplateChild(childName) as T;
        }

        public void StartAutoScroll()
        {
            if (_autoScrollCommand == null) return;

            if (_autoScrollTimer == null)
            {
                IsAutoScrolling = false;

                _autoScrollTimer = new DispatcherTimer(DispatcherPriority.SystemIdle);
                _autoScrollTimer.Interval = AutoScrollTimeout;
                _autoScrollTimer.Tick += new EventHandler(OnAutoScrollTimeout);
                _autoScrollTimer.Start();
            }
        }

        public void StopAutoScroll()
        {
            if (_autoScrollTimer != null)
            {
                _autoScrollTimer.Stop();
                _autoScrollTimer = null;

                IsAutoScrolling = false;
            }
        }

        private bool DoAutoScroll()
        {
            if (_autoScrollCommand == null) return false;

            RelativeMousePositions position = RelativeMousePosition;
            if (position != RelativeMousePositions.Over)
            {
                if (position.HasFlag(RelativeMousePositions.Left))
                {
                    LineLeft();
                }
                else if (position.HasFlag(RelativeMousePositions.Right))
                {
                    LineRight();
                }
                else if (position.HasFlag(RelativeMousePositions.Above))
                {
                    LineUp();
                }
                else if (position.HasFlag(RelativeMousePositions.Below))
                {
                    LineDown();
                }

                return ExecuteAutoScroll();
            }

            return false;
        }

        private bool ExecuteAutoScroll()
        {
            if (_autoScrollCommand.CanExecute(this) == true)
            {
                IsAutoScrolling = true;
                _autoScrollCommand.Execute(this);
                return true;
            }

            return false;
        }

        private void OnAutoScrollTimeout(object sender, EventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DoAutoScroll();
            }
            else
            {
                StopAutoScroll();
            }
        }
        #endregion
    }
}
