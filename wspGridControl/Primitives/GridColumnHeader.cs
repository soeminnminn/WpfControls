using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace wspGridControl.Primitives
{
    [TemplatePart(Name = PART_HeaderGripper, Type = typeof(Thumb))]
    [TemplatePart(Name = PART_FloatingHeaderCanvas, Type = typeof(Canvas))]
    public class GridColumnHeader : ButtonBase
    {
        #region Variables
        private const string PART_HeaderGripper = "PART_HeaderGripper";
        private const string PART_FloatingHeaderCanvas = "PART_FloatingHeaderCanvas";

        private Thumb _headerGripper;
        private Canvas _floatingHeaderCanvas;

        private GridColumnHeader _previousHeader;
        private GridColumnHeader _srcHeader;

        private Flags _flags;
        private double _originalWidth;
        #endregion

        #region Constructors
        static GridColumnHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridColumnHeader), new FrameworkPropertyMetadata(typeof(GridColumnHeader)));
            FocusableProperty.OverrideMetadata(typeof(GridColumnHeader), new FrameworkPropertyMetadata(false));

            StyleProperty.OverrideMetadata(typeof(GridColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
            ContentTemplateProperty.OverrideMetadata(typeof(GridColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
            ContentTemplateSelectorProperty.OverrideMetadata(typeof(GridColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
            ContextMenuProperty.OverrideMetadata(typeof(GridColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
            ToolTipProperty.OverrideMetadata(typeof(GridColumnHeader), new FrameworkPropertyMetadata(PropertyChanged));
        }

        public GridColumnHeader()
            : base()
        {
            MinHeight = SystemParameters.CaptionHeight;
        }
        #endregion

        #region Dependency Properties

        #region RolePropertyKey
        /// <summary>
        /// The key for Role (read-only property)
        /// </summary>
        internal static readonly DependencyPropertyKey RolePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Role), typeof(GridColumnHeaderRole), typeof(GridColumnHeader),
            new FrameworkPropertyMetadata(GridColumnHeaderRole.Normal));

        /// <summary>
        /// The DependencyProperty for the Role property.
        /// </summary>
        private static readonly DependencyProperty RoleProperty = RolePropertyKey.DependencyProperty;

        /// <summary>
        /// What the role of the header is: Normal, Floating, Padding.
        /// </summary>
        [Category("Behavior")]
        public GridColumnHeaderRole Role
        {
            get => (GridColumnHeaderRole)GetValue(RoleProperty);
        }
        #endregion

        #region ColumnProperty
        /// <summary>
        /// The key for Column (read-only property)
        /// </summary>
        internal static readonly DependencyPropertyKey ColumnPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Column), typeof(GridColumn), typeof(GridColumnHeader), null);

        /// <summary>
        /// The DependencyProperty for the Column property.
        /// </summary>
        private static readonly DependencyProperty ColumnProperty = ColumnPropertyKey.DependencyProperty;

        /// <summary>
        /// Column associated with this header
        /// </summary>
        internal GridColumn Column
        {
            get => (GridColumn)GetValue(ColumnProperty);
        }
        #endregion

        #region IsClickableProperty
        internal static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register(
            nameof(IsClickable), typeof(bool), typeof(GridColumnHeader),
            new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsClickableChanged)));

        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set { SetValue(IsClickableProperty, value); }
        }

        private static void OnIsClickableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region IsResizableProperty
        internal static readonly DependencyProperty IsResizableProperty = DependencyProperty.Register(
            nameof(IsResizable), typeof(bool), typeof(GridColumnHeader),
            new FrameworkPropertyMetadata(true, new PropertyChangedCallback(OnIsResizableChanged)));

        public bool IsResizable
        {
            get => (bool)GetValue(IsResizableProperty);
            set { SetValue(IsResizableProperty, value); }
        }

        private static void OnIsResizableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridColumnHeader c)
            {
                if ((bool)e.NewValue)
                {
                    c.HookupGripperEvents();
                }
                else
                {
                    c.UnhookGripperEvents();
                }
            }
        }
        #endregion

        #endregion

        #region Properties
        private double ColumnActualWidth
        {
            get { return Column != null ? Column.ActualWidth : ActualWidth; }
        }

        private Cursor SplitCursor
        {
            get => Cursors.SizeWE;
        }

        private Cursor SplitOpenCursor
        {
            get => Cursors.SizeWE;
        }

        internal GridColumnHeader PreviousVisualHeader
        {
            get { return _previousHeader; }
            set { _previousHeader = value; }
        }

        internal GridColumnHeader FloatSourceHeader
        {
            get { return _srcHeader; }
            set { _srcHeader = value; }
        }

        // indicating whether to fire click event
        internal bool SuppressClickEvent
        {
            get { return GetFlag(Flags.SuppressClickEvent); }
            set { SetFlag(Flags.SuppressClickEvent, value); }
        }

        // whether this header is generated by GVHeaderRowPresenter or user
        internal bool IsInternalGenerated
        {
            get { return GetFlag(Flags.IsInternalGenerated); }
            set { SetFlag(Flags.IsInternalGenerated, value); }
        }

        // is clicked by access key or automation
        private bool IsAccessKeyOrAutomation
        {
            get { return GetFlag(Flags.IsAccessKeyOrAutomation); }
            set { SetFlag(Flags.IsAccessKeyOrAutomation, value); }
        }
        #endregion

        #region Methods
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            GridColumnHeaderRole role = Role;

            if (role == GridColumnHeaderRole.Normal)
            {
                if (IsResizable)
                {
                    HookupGripperEvents();
                }
            }
            else if (role == GridColumnHeaderRole.Floating)
            {
                // if this is a floating header, try to find the FloatingHeaderCanvas,
                // and copy source header's visual to it
                _floatingHeaderCanvas = GetTemplateChild(PART_FloatingHeaderCanvas) as Canvas;

                UpdateFloatingHeaderCanvas();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            // when render size is changed, check to hide the previous header's right half gripper
            CheckWidthForPreviousHeaderGripper();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            // give parent a chance to handle MouseButtonEvent (for GridHeaderRowPresenter by default)
            e.Handled = false;

            if (ClickMode == ClickMode.Hover && IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (!IsClickable)
            {
                e.Handled = true;
                return;
            }

            base.OnMouseLeftButtonDown(e);

            // give parent a chance to handle MouseButtonEvent (for GridHeaderRowPresenter by default)
            e.Handled = false;

            //If ClickMode is Hover, we must capture mouse in order to let column reorder work correctly (Bug#1496673)
            if (ClickMode == ClickMode.Hover && e.ButtonState == MouseButtonState.Pressed)
            {
                CaptureMouse();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Override base method: if left mouse is pressed, always set IsPressed as true
            if ((ClickMode != ClickMode.Hover) && IsMouseCaptured && (Mouse.PrimaryDevice.LeftButton == MouseButtonState.Pressed))
            {
                IsPressed = true;
            }

            e.Handled = false;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (HandleIsMouseOverChanged())
            {
                e.Handled = true;
            }
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnLostKeyboardFocus(e);

            if (ClickMode == ClickMode.Hover && IsMouseCaptured)
            {
                ReleaseMouseCapture();
            }
        }

        protected override void OnAccessKey(AccessKeyEventArgs e)
        {
            IsAccessKeyOrAutomation = true;

            base.OnAccessKey(e);
        }

        protected override void OnClick()
        {
            if (!SuppressClickEvent && IsClickable)
            {
                // if is clicked by access key or automation,
                // otherwise should be clicked by mouse
                if (IsAccessKeyOrAutomation || !IsMouseOutside())
                {
                    IsAccessKeyOrAutomation = false;
                    ClickImplement();
                    MakeParentGotFocus();
                }
            }
        }

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            if (IsInternalGenerated)
            {
                // nothing should be serialized from this object.
                return false;
            }

            PropertyToFlags(dp, out Flags flag, out _);

            return ((flag == Flags.None) || GetFlag(flag)) && base.ShouldSerializeProperty(dp);
        }

        internal void AutomationClick()
        {
            IsAccessKeyOrAutomation = true;
            OnClick();
        }

        private bool HandleIsMouseOverChanged()
        {
            if (ClickMode == ClickMode.Hover)
            {
                if (!IsClickable && IsMouseOver) return true;

                if (IsMouseOver && (_headerGripper == null || !_headerGripper.IsMouseOver))
                {
                    // Hovering over the button will click in the OnHover click mode
                    IsPressed = true;
                    OnClick();
                }
                else
                {
                    IsPressed = false;
                }
                return true;
            }
            return false;
        }

        internal void OnColumnHeaderKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && _headerGripper != null && _headerGripper.IsDragging)
            {
                // NOTE: this will cause Thumb to complete the dragging and fire drag
                // complete event with the Canceled property as 'True'. Handler
                // OnGridColumnHeaderGripperDragCompleted will restore the width.
                _headerGripper.CancelDrag();
                e.Handled = true;
            }
        }

        #region Gripper Methods
        private void HookupGripperEvents()
        {
            UnhookGripperEvents();

            _headerGripper = GetTemplateChild(PART_HeaderGripper) as Thumb;

            if (_headerGripper != null)
            {
                _headerGripper.DragStarted += new DragStartedEventHandler(OnColumnHeaderGripperDragStarted);
                _headerGripper.DragDelta += new DragDeltaEventHandler(OnColumnHeaderResize);
                _headerGripper.DragCompleted += new DragCompletedEventHandler(OnColumnHeaderGripperDragCompleted);
                _headerGripper.MouseDoubleClick += new MouseButtonEventHandler(OnGripperDoubleClicked);
                _headerGripper.MouseEnter += new MouseEventHandler(OnGripperMouseEnterLeave);
                _headerGripper.MouseLeave += new MouseEventHandler(OnGripperMouseEnterLeave);

                _headerGripper.Cursor = SplitCursor;
            }
        }

        private void UnhookGripperEvents()
        {
            if (_headerGripper != null)
            {
                _headerGripper.DragStarted -= new DragStartedEventHandler(OnColumnHeaderGripperDragStarted);
                _headerGripper.DragDelta -= new DragDeltaEventHandler(OnColumnHeaderResize);
                _headerGripper.DragCompleted -= new DragCompletedEventHandler(OnColumnHeaderGripperDragCompleted);
                _headerGripper.MouseDoubleClick -= new MouseButtonEventHandler(OnGripperDoubleClicked);
                _headerGripper.MouseEnter -= new MouseEventHandler(OnGripperMouseEnterLeave);
                _headerGripper.MouseLeave -= new MouseEventHandler(OnGripperMouseEnterLeave);
                _headerGripper = null;
            }
        }

        private void OnColumnHeaderGripperDragStarted(object sender, DragStartedEventArgs e)
        {
            MakeParentGotFocus();
            _originalWidth = ColumnActualWidth;
            e.Handled = true;
        }

        private void OnColumnHeaderResize(object sender, DragDeltaEventArgs e)
        {
            double width = ColumnActualWidth + e.HorizontalChange;
            if (DoubleUtil.LessThanOrClose(width, 0.0))
            {
                width = 0.0;
            }

            UpdateColumnHeaderWidth(width);
            e.Handled = true;
        }

        private void OnColumnHeaderGripperDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (e.Canceled)
            {
                // restore to original width
                UpdateColumnHeaderWidth(_originalWidth);
            }

            UpdateGripperCursor();
            e.Handled = true;
        }

        private void OnGripperMouseEnterLeave(object sender, MouseEventArgs e)
        {
            HandleIsMouseOverChanged();
        }

        private void OnGripperDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            if (Column != null)
            {
                if (double.IsNaN(Column.ColumnWidth))
                {
                    // force update will be triggered
                    Column.ColumnWidth = Column.ActualWidth;
                }

                Column.ColumnWidth = double.NaN;
                e.Handled = true;
            }
        }

        private void UpdateGripperCursor()
        {
            if (_headerGripper != null && !_headerGripper.IsDragging)
            {
                Cursor gripperCursor;

                if (DoubleUtil.IsZero(ActualWidth))
                {
                    gripperCursor = SplitOpenCursor;
                }
                else
                {
                    gripperCursor = SplitCursor;
                }

                if (gripperCursor != null)
                {
                    _headerGripper.Cursor = gripperCursor;
                }
            }
        }

        private void HideGripperRightHalf(bool hide)
        {
            if (_headerGripper != null)
            {
                // hide gripper's right half by setting Parent.ClipToBounds=true
                FrameworkElement gripperContainer = _headerGripper.Parent as FrameworkElement;
                if (gripperContainer != null)
                {
                    gripperContainer.ClipToBounds = hide;
                }
            }
        }

        internal void CheckWidthForPreviousHeaderGripper()
        {
            bool hideGripperRightHalf = false;

            if (_headerGripper != null)
            {
                // when header's width is less than gripper's width,
                // hide the right half of the left header's gripper
                hideGripperRightHalf = DoubleUtil.LessThan(ActualWidth, _headerGripper.Width);
            }

            if (_previousHeader != null)
            {
                _previousHeader.HideGripperRightHalf(hideGripperRightHalf);
            }

            UpdateGripperCursor();
        }
        #endregion

        #region FloatingHeader
        private void UpdateFloatingHeaderCanvas()
        {
            if (_floatingHeaderCanvas != null && FloatSourceHeader != null)
            {
                // because the gripper is partially positioned out of the header, we need to
                // map the appropriate area(viewbox) in the source header to visual brush
                // to avoid a distorded image on the floating header.
                Vector offsetVector = VisualTreeHelper.GetOffset(FloatSourceHeader);
                VisualBrush visualBrush = new VisualBrush(FloatSourceHeader);

                // set visual brush's mapping
                visualBrush.ViewboxUnits = BrushMappingMode.Absolute;
                visualBrush.Viewbox = new Rect(offsetVector.X, offsetVector.Y, FloatSourceHeader.ActualWidth, FloatSourceHeader.ActualHeight);

                _floatingHeaderCanvas.Background = visualBrush;
                FloatSourceHeader = null;
            }
        }

        internal void ResetFloatingHeaderCanvasBackground()
        {
            if (_floatingHeaderCanvas != null)
            {
                _floatingHeaderCanvas.Background = null;
            }
        }
        #endregion

        internal void UpdateProperty(DependencyProperty dp, object value)
        {
            if (dp == null) return;

            Flags ignoreFlag = Flags.None;

            if (!IsInternalGenerated)
            {
                Flags flag;
                PropertyToFlags(dp, out flag, out ignoreFlag);
                Debug.Assert(flag != Flags.None && ignoreFlag != Flags.None, "Invalid parameter dp.");

                if (GetFlag(flag)) /* user has provided value for the property */
                {
                    return;
                }
                else
                {
                    SetFlag(ignoreFlag, true);
                }
            }

            if (value != null)
            {
                SetValue(dp, value);
            }
            else
            {
                ClearValue(dp);
            }

            SetFlag(ignoreFlag, false);
        }

        private static void PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GridColumnHeader header = (GridColumnHeader)d;
            if (!header.IsInternalGenerated)
            {
                Flags flag, ignoreFlag;
                PropertyToFlags(e.Property, out flag, out ignoreFlag);

                if (!header.GetFlag(ignoreFlag)) // value is updated by user
                {
                    var valueSource = DependencyPropertyHelper.GetValueSource(d, e.Property);
                    if (valueSource.BaseValueSource.HasFlag(BaseValueSource.Local))
                    {
                        header.SetFlag(flag, true);
                    }
                    else
                    {
                        header.SetFlag(flag, false);
                    }
                }
            }
        }

        private static void PropertyToFlags(DependencyProperty dp, out Flags flag, out Flags ignoreFlag)
        {
            if (dp == GridColumnHeader.StyleProperty)
            {
                flag = Flags.StyleSetByUser;
                ignoreFlag = Flags.IgnoreStyle;
            }
            else if (dp == GridColumnHeader.ContentTemplateProperty)
            {
                flag = Flags.ContentTemplateSetByUser;
                ignoreFlag = Flags.IgnoreContentTemplate;
            }
            else if (dp == GridColumnHeader.ContentTemplateSelectorProperty)
            {
                flag = Flags.ContentTemplateSelectorSetByUser;
                ignoreFlag = Flags.IgnoreContentTemplateSelector;
            }
            else if (dp == GridColumnHeader.ContentStringFormatProperty)
            {
                flag = Flags.ContentStringFormatSetByUser;
                ignoreFlag = Flags.IgnoreContentStringFormat;
            }
            else if (dp == GridColumnHeader.ContextMenuProperty)
            {
                flag = Flags.ContextMenuSetByUser;
                ignoreFlag = Flags.IgnoreContextMenu;
            }
            else if (dp == GridColumnHeader.ToolTipProperty)
            {
                flag = Flags.ToolTipSetByUser;
                ignoreFlag = Flags.IgnoreToolTip;
            }
            else
            {
                flag = ignoreFlag = Flags.None;
            }
        }

        private bool GetFlag(Flags flag)
        {
            return (_flags & flag) == flag;
        }

        private void SetFlag(Flags flag, bool set)
        {
            if (set)
            {
                _flags |= flag;
            }
            else
            {
                _flags &= ~flag;
            }
        }

        private void UpdateColumnHeaderWidth(double width)
        {
            if (Column != null)
            {
                Column.ColumnWidth = width;
            }
            else
            {
                Width = width;
            }
        }

        private bool IsMouseOutside()
        {
            Point pos = Mouse.PrimaryDevice.GetPosition(this);
            return !((pos.X >= 0) && (pos.X <= ActualWidth) && (pos.Y >= 0) && (pos.Y <= ActualHeight));
        }

        private void ClickImplement()
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.InvokePatternOnInvoked))
            {
                AutomationPeer peer = UIElementAutomationPeer.CreatePeerForElement(this);
                if (peer != null)
                    peer.RaiseAutomationEvent(AutomationEvents.InvokePatternOnInvoked);
            }

            base.OnClick();
        }

        private void MakeParentGotFocus()
        {
            GridColumnHeadersPresenter headerRP = Parent as GridColumnHeadersPresenter;
            if (headerRP != null)
            {
                headerRP.MakeParentControlGotFocus();
            }
        }
        #endregion

        #region Nested Types
        /// <summary>
        /// StyleSetByUser: the value of Style property is set by user.
        /// IgnoreStyle: the OnStyleChanged is triggered by HeaderRowPresenter,
        /// not by user. Don't turn on the StyleSetByUser flag.
        /// And so on
        /// (Only for user provided header. Ignored for internal generated header)
        ///
        /// Go to UpdateProperty and OnPropetyChanged for how these flags work.
        /// </summary>
        [Flags]
        private enum Flags
        {
            // IgnoreXXX can't be combined into one flag.
            // Reason:
            // Define a Style with ContentTemplate and assign it to GridColumn.HeaderContainerStyle property. GridColumnHeader.OnPropertyChagned method will be called twice.
            // The first call is for ContentTemplate property. In this call, IgnoreContentTemplate is false.
            // The second call is for Style property. In this call, IgnoreStyle is true.
            // One flag can’t distinguish them.
            None = 0,
            StyleSetByUser = 0x00000001,
            IgnoreStyle = 0x00000002,
            ContentTemplateSetByUser = 0x00000004,
            IgnoreContentTemplate = 0x00000008,
            ContentTemplateSelectorSetByUser = 0x00000010,
            IgnoreContentTemplateSelector = 0x00000020,
            ContextMenuSetByUser = 0x00000040,
            IgnoreContextMenu = 0x00000080,
            ToolTipSetByUser = 0x00000100,
            IgnoreToolTip = 0x00000200,

            SuppressClickEvent = 0x00000400,
            IsInternalGenerated = 0x00000800,
            IsAccessKeyOrAutomation = 0x00001000,

            ContentStringFormatSetByUser = 0x00002000,
            IgnoreContentStringFormat = 0x00004000,
        }
        #endregion
    }
}
