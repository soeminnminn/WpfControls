using System;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace wspGridControl.Primitives
{
    public class GridRowHeader : ButtonBase
    {
        #region Variables
        private Flags _flags;
        #endregion

        #region Constructors
        static GridRowHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridRowHeader), new FrameworkPropertyMetadata(typeof(GridRowHeader)));
            FocusableProperty.OverrideMetadata(typeof(GridRowHeader), new FrameworkPropertyMetadata(false));
        }

        public GridRowHeader()
            : base()
        { }
        #endregion

        #region Dependency Properties

        #region RowIndexProperty
        /// <summary>
        /// The key for RowIndex (read-only property)
        /// </summary>
        internal static readonly DependencyPropertyKey RowIndexPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(RowIndex), typeof(long), typeof(GridRowHeader), null);

        /// <summary>
        /// The DependencyProperty for the RowIndex property.
        /// </summary>
        private static readonly DependencyProperty RowIndexProperty = RowIndexPropertyKey.DependencyProperty;

        /// <summary>
        /// RowIndex associated with this header
        /// </summary>
        public long RowIndex
        {
            get { return (long)GetValue(RowIndexProperty); }
        }
        #endregion

        #endregion

        #region Properties
        // indicating whether to fire click event
        internal bool SuppressClickEvent
        {
            get { return GetFlag(Flags.SuppressClickEvent); }
            set { SetFlag(Flags.SuppressClickEvent, value); }
        }

        // is clicked by access key or automation
        private bool IsAccessKeyOrAutomation
        {
            get { return GetFlag(Flags.IsAccessKeyOrAutomation); }
            set { SetFlag(Flags.IsAccessKeyOrAutomation, value); }
        }
        #endregion

        #region Methods
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

        /// <summary>
        /// Override base method: raises the Click event
        /// </summary>
        protected override void OnClick()
        {
            // if not suppress click event
            if (!SuppressClickEvent)
            {
                // if is clicked by access key or automation,
                // otherwise should be clicked by mouse
                if (IsAccessKeyOrAutomation || !IsMouseOutside())
                {
                    IsAccessKeyOrAutomation = false;
                    ClickImplement();
                }
            }
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
                if (IsMouseOver)
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

        protected override bool ShouldSerializeProperty(DependencyProperty dp)
        {
            return false;
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
        #endregion

        #region Nested Types
        [Flags]
        private enum Flags
        {
            None = 0,
            SuppressClickEvent = 0x00000001,
            IsAccessKeyOrAutomation = 0x00000002,
        }
        #endregion
    }
}
