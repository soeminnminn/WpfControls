﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace wspGridControl.Primitives
{
    /// <summary>
    ///     A Border used to provide the default look of DataGrid headers.
    ///     When Background or BorderBrush are set, the rendering will
    ///     revert back to the default Border implementation.
    /// </summary>
    public sealed class GridHeaderChrome : Border
    {
        #region Variables
        private static List<Freezable> _freezableCache;
        private static object _cacheAccess = new object();
        #endregion

        #region Constructors
        static GridHeaderChrome()
        {
            // We always set this to true on these borders, so just default it to true here.
            SnapsToDevicePixelsProperty.OverrideMetadata(typeof(GridHeaderChrome), new FrameworkPropertyMetadata(true));
        }
        #endregion

        #region Dependency Properties

        #region IsHoveredProperty
        /// <summary>
        ///     Whether the hover look should be applied.
        /// </summary>
        public bool IsHovered
        {
            get { return (bool)GetValue(IsHoveredProperty); }
            set { SetValue(IsHoveredProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for IsHovered.
        /// </summary>
        public static readonly DependencyProperty IsHoveredProperty =
            DependencyProperty.Register("IsHovered", typeof(bool), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region IsPressedProperty
        /// <summary>
        ///     Whether the pressed look should be applied.
        /// </summary>
        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            set { SetValue(IsPressedProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for IsPressed.
        /// </summary>
        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register("IsPressed", typeof(bool), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange));
        #endregion

        #region IsClickableProperty
        /// <summary>
        ///     When false, will not apply the hover look even when IsHovered is true.
        /// </summary>
        public bool IsClickable
        {
            get { return (bool)GetValue(IsClickableProperty); }
            set { SetValue(IsClickableProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for IsClickable.
        /// </summary>
        public static readonly DependencyProperty IsClickableProperty =
            DependencyProperty.Register("IsClickable", typeof(bool), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsArrange));
        #endregion

        #region SortDirectionProperty
        /// <summary>
        ///     Whether to appear sorted.
        /// </summary>
        public ListSortDirection? SortDirection
        {
            get { return (ListSortDirection?)GetValue(SortDirectionProperty); }
            set { SetValue(SortDirectionProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for SortDirection.
        /// </summary>
        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register("SortDirection", typeof(ListSortDirection?), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region IsSelectedProperty
        /// <summary>
        ///     Whether to appear selected.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for IsSelected.
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region OrientationProperty
        /// <summary>
        ///     Vertical = column header
        ///     Horizontal = row header
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for Orientation.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        /// <summary>
        ///     When there is a Background or BorderBrush, revert to the Border implementation.
        /// </summary>
        private bool UsingBorderImplementation
        {
            get
            {
                return (Background != null) || (BorderBrush != null);
            }
        }

        #region IsUseAeroProperty
        /// <summary>
        ///     Use Aero styles.
        /// </summary>
        public bool IsUseAero
        {
            get { return (bool)GetValue(IsUseAeroProperty); }
            set { SetValue(IsUseAeroProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for IsUseAero.
        /// </summary>
        public static readonly DependencyProperty IsUseAeroProperty =
            DependencyProperty.Register("IsUseAero", typeof(bool), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
        #endregion

        #region SeparatorBrushProperty
        /// <summary>
        ///     Property that indicates the brush to use when drawing seperators between headers.
        /// </summary>
        public Brush SeparatorBrush
        {
            get { return (Brush)GetValue(SeparatorBrushProperty); }
            set { SetValue(SeparatorBrushProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for SeparatorBrush.
        /// </summary>
        public static readonly DependencyProperty SeparatorBrushProperty =
            DependencyProperty.Register("SeparatorBrush", typeof(Brush), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(null));
        #endregion

        #region SeparatorVisibilityProperty
        /// <summary>
        ///     Property that indicates the Visibility for the header seperators.
        /// </summary>
        public Visibility SeparatorVisibility
        {
            get { return (Visibility)GetValue(SeparatorVisibilityProperty); }
            set { SetValue(SeparatorVisibilityProperty, value); }
        }

        /// <summary>
        ///     DependencyProperty for SeperatorBrush.
        /// </summary>
        public static readonly DependencyProperty SeparatorVisibilityProperty =
            DependencyProperty.Register("SeparatorVisibility", typeof(Visibility), typeof(GridHeaderChrome),
                new FrameworkPropertyMetadata(Visibility.Visible));
        #endregion

        #endregion

        #region Properties
        /// <summary>
        ///     Returns a default padding for the various themes for use
        ///     by measure and arrange.
        /// </summary>
        private Thickness DefaultPadding
        {
            get
            {
                Thickness padding = new Thickness(3.0); // The default padding
                Thickness? themePadding = ThemeDefaultPadding;
                if (themePadding == null)
                {
                    if (Orientation == Orientation.Vertical)
                    {
                        // Reserve space to the right for the arrow
                        padding.Right = 15.0;
                    }
                }
                else
                {
                    padding = (Thickness)themePadding;
                }

                // When pressed, offset the child
                if (IsPressed && IsClickable)
                {
                    padding.Left += 1.0;
                    padding.Top += 1.0;
                    padding.Right -= 1.0;
                    padding.Bottom -= 1.0;
                }

                return padding;
            }
        }

        private Thickness? ThemeDefaultPadding
        {
            get
            {
                if (Orientation == Orientation.Vertical)
                {
                    return new Thickness(5.0, 4.0, 5.0, 4.0);
                }
                return null;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        ///     Calculates the desired size of the element given the constraint.
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            if (UsingBorderImplementation)
            {
                // Revert to the Border implementation
                return base.MeasureOverride(constraint);
            }

            UIElement child = Child;
            if (child != null)
            {
                // Use the public Padding property if it's set
                Thickness padding = Padding;
                if (padding.Equals(new Thickness()))
                {
                    padding = DefaultPadding;
                }

                double childWidth = constraint.Width;
                double childHeight = constraint.Height;

                // If there is an actual constraint, then reserve space for the chrome
                if (!double.IsInfinity(childWidth))
                {
                    childWidth = Math.Max(0.0, childWidth - padding.Left - padding.Right);
                }

                if (!double.IsInfinity(childHeight))
                {
                    childHeight = Math.Max(0.0, childHeight - padding.Top - padding.Bottom);
                }

                child.Measure(new Size(childWidth, childHeight));
                Size desiredSize = child.DesiredSize;

                // Add on the reserved space for the chrome
                return new Size(desiredSize.Width + padding.Left + padding.Right, desiredSize.Height + padding.Top + padding.Bottom);
            }

            return new Size();
        }

        /// <summary>
        ///     Positions children and returns the final size of the element.
        /// </summary>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (UsingBorderImplementation)
            {
                // Revert to the Border implementation
                return base.ArrangeOverride(arrangeSize);
            }

            UIElement child = Child;
            if (child != null)
            {
                // Use the public Padding property if it's set
                Thickness padding = Padding;
                if (padding.Equals(new Thickness()))
                {
                    padding = DefaultPadding;
                }

                // Reserve space for the chrome
                double childWidth = Math.Max(0.0, arrangeSize.Width - padding.Left - padding.Right);
                double childHeight = Math.Max(0.0, arrangeSize.Height - padding.Top - padding.Bottom);

                child.Arrange(new Rect(padding.Left, padding.Top, childWidth, childHeight));
            }

            return arrangeSize;
        }

        /// <summary>
        ///     Called when this element should re-render.
        /// </summary>
        protected override void OnRender(DrawingContext dc)
        {
            if (UsingBorderImplementation)
            {
                // Revert to the Border implementation
                base.OnRender(dc);
            }
            else if (IsUseAero)
            {
                RenderThemeAero(dc);
            }
            else
            {
                RenderTheme(dc);
            }
        }

        private static double Max0(double d)
        {
            return Math.Max(0.0, d);
        }

        private void RenderThemeAero(DrawingContext dc)
        {
            Size size = RenderSize;
            bool horizontal = Orientation == Orientation.Horizontal;
            bool isClickable = IsClickable && IsEnabled;
            bool isHovered = isClickable && IsHovered;
            bool isPressed = isClickable && IsPressed;
            ListSortDirection? sortDirection = SortDirection;
            bool isSorted = sortDirection != null;
            bool isSelected = IsSelected;
            bool hasBevel = (!isHovered && !isPressed && !isSorted && !isSelected);

            EnsureCache((int)FreezableParts.NumFreezables);

            if (horizontal)
            {
                // When horizontal, rotate the rendering by -90 degrees
                Matrix m1 = new Matrix();
                m1.RotateAt(-90.0, 0.0, 0.0);
                Matrix m2 = new Matrix();
                m2.Translate(0.0, size.Height);

                MatrixTransform horizontalRotate = new MatrixTransform(m1 * m2);
                horizontalRotate.Freeze();
                dc.PushTransform(horizontalRotate);

                double temp = size.Width;
                size.Width = size.Height;
                size.Height = temp;
            }

            if (hasBevel)
            {
                // This is a highlight that can be drawn by just filling the background with the color.
                // It will be seen through the gab between the border and the background.
                LinearGradientBrush bevel = (LinearGradientBrush)GetCachedFreezable((int)FreezableParts.NormalBevel);
                if (bevel == null)
                {
                    bevel = new LinearGradientBrush();
                    bevel.StartPoint = new Point();
                    bevel.EndPoint = new Point(0.0, 1.0);
                    bevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), 0.0));
                    bevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), 0.4));
                    bevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xFC, 0xFC, 0xFD), 0.4));
                    bevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xFB, 0xFC, 0xFC), 1.0));
                    bevel.Freeze();

                    CacheFreezable(bevel, (int)FreezableParts.NormalBevel);
                }

                dc.DrawRectangle(bevel, null, new Rect(0.0, 0.0, size.Width, size.Height));
            }

            // Fill the background
            FreezableParts backgroundType = FreezableParts.NormalBackground;
            if (isPressed)
            {
                backgroundType = FreezableParts.PressedBackground;
            }
            else if (isHovered)
            {
                backgroundType = FreezableParts.HoveredBackground;
            }
            else if (isSorted || isSelected)
            {
                backgroundType = FreezableParts.SortedBackground;
            }

            LinearGradientBrush background = (LinearGradientBrush)GetCachedFreezable((int)backgroundType);
            if (background == null)
            {
                background = new LinearGradientBrush();
                background.StartPoint = new Point();
                background.EndPoint = new Point(0.0, 1.0);

                switch (backgroundType)
                {
                    case FreezableParts.NormalBackground:
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), 0.0));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF7, 0xF8, 0xFA), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF1, 0xF2, 0xF4), 1.0));
                        break;

                    case FreezableParts.PressedBackground:
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xBC, 0xE4, 0xF9), 0.0));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xBC, 0xE4, 0xF9), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x8D, 0xD6, 0xF7), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x8A, 0xD1, 0xF5), 1.0));
                        break;

                    case FreezableParts.HoveredBackground:
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xE3, 0xF7, 0xFF), 0.0));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xE3, 0xF7, 0xFF), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xBD, 0xED, 0xFF), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xB7, 0xE7, 0xFB), 1.0));
                        break;

                    case FreezableParts.SortedBackground:
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF2, 0xF9, 0xFC), 0.0));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF2, 0xF9, 0xFC), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xE1, 0xF1, 0xF9), 0.4));
                        background.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xD8, 0xEC, 0xF6), 1.0));
                        break;
                }

                background.Freeze();

                CacheFreezable(background, (int)backgroundType);
            }

            dc.DrawRectangle(background, null, new Rect(0.0, 0.0, size.Width, size.Height));

            if (size.Width >= 2.0)
            {
                // Draw the borders on the sides
                FreezableParts sideType = FreezableParts.NormalSides;
                if (isPressed)
                {
                    sideType = FreezableParts.PressedSides;
                }
                else if (isHovered)
                {
                    sideType = FreezableParts.HoveredSides;
                }
                else if (isSorted || isSelected)
                {
                    sideType = FreezableParts.SortedSides;
                }

                if (SeparatorVisibility == Visibility.Visible)
                {
                    Brush sideBrush;
                    if (SeparatorBrush != null)
                    {
                        sideBrush = SeparatorBrush;
                    }
                    else
                    {
                        sideBrush = (Brush)GetCachedFreezable((int)sideType);
                        if (sideBrush == null)
                        {
                            LinearGradientBrush lgBrush = null;
                            if (sideType != FreezableParts.SortedSides)
                            {
                                lgBrush = new LinearGradientBrush();
                                lgBrush.StartPoint = new Point();
                                lgBrush.EndPoint = new Point(0.0, 1.0);
                                sideBrush = lgBrush;
                            }

                            switch (sideType)
                            {
                                case FreezableParts.NormalSides:
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xF2, 0xF2, 0xF2), 0.0));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xEF, 0xEF, 0xEF), 0.4));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xE7, 0xE8, 0xEA), 0.4));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xDE, 0xDF, 0xE1), 1.0));
                                    break;

                                case FreezableParts.PressedSides:
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x7A, 0x9E, 0xB1), 0.0));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x7A, 0x9E, 0xB1), 0.4));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x50, 0x91, 0xAF), 0.4));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x4D, 0x8D, 0xAD), 1.0));
                                    break;

                                case FreezableParts.HoveredSides:
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x88, 0xCB, 0xEB), 0.0));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x88, 0xCB, 0xEB), 0.4));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x69, 0xBB, 0xE3), 0.4));
                                    lgBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x69, 0xBB, 0xE3), 1.0));
                                    break;

                                case FreezableParts.SortedSides:
                                    sideBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x96, 0xD9, 0xF9));
                                    break;
                            }

                            sideBrush.Freeze();

                            CacheFreezable(sideBrush, (int)sideType);
                        }
                    }

                    dc.DrawRectangle(sideBrush, null, new Rect(0.0, 0.0, 1.0, Max0(size.Height - 0.95)));
                    dc.DrawRectangle(sideBrush, null, new Rect(size.Width - 1.0, 0.0, 1.0, Max0(size.Height - 0.95)));
                }
            }

            if (isPressed && (size.Width >= 4.0) && (size.Height >= 4.0))
            {
                // When pressed, there are added borders on the left and top
                LinearGradientBrush topBrush = (LinearGradientBrush)GetCachedFreezable((int)FreezableParts.PressedTop);
                if (topBrush == null)
                {
                    topBrush = new LinearGradientBrush();
                    topBrush.StartPoint = new Point();
                    topBrush.EndPoint = new Point(0.0, 1.0);
                    topBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x86, 0xA3, 0xB2), 0.0));
                    topBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x86, 0xA3, 0xB2), 0.1));
                    topBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xAA, 0xCE, 0xE1), 0.9));
                    topBrush.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xAA, 0xCE, 0xE1), 1.0));
                    topBrush.Freeze();

                    CacheFreezable(topBrush, (int)FreezableParts.PressedTop);
                }

                dc.DrawRectangle(topBrush, null, new Rect(0.0, 0.0, size.Width, 2.0));

                LinearGradientBrush pressedBevel = (LinearGradientBrush)GetCachedFreezable((int)FreezableParts.PressedBevel);
                if (pressedBevel == null)
                {
                    pressedBevel = new LinearGradientBrush();
                    pressedBevel.StartPoint = new Point();
                    pressedBevel.EndPoint = new Point(0.0, 1.0);
                    pressedBevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xA2, 0xCB, 0xE0), 0.0));
                    pressedBevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xA2, 0xCB, 0xE0), 0.4));
                    pressedBevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x72, 0xBC, 0xDF), 0.4));
                    pressedBevel.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x6E, 0xB8, 0xDC), 1.0));
                    pressedBevel.Freeze();

                    CacheFreezable(pressedBevel, (int)FreezableParts.PressedBevel);
                }

                dc.DrawRectangle(pressedBevel, null, new Rect(1.0, 0.0, 1.0, size.Height - 0.95));
                dc.DrawRectangle(pressedBevel, null, new Rect(size.Width - 2.0, 0.0, 1.0, size.Height - 0.95));
            }

            if (size.Height >= 2.0)
            {
                // Draw the bottom border
                FreezableParts bottomType = FreezableParts.NormalBottom;
                if (isPressed)
                {
                    bottomType = FreezableParts.PressedOrHoveredBottom;
                }
                else if (isHovered)
                {
                    bottomType = FreezableParts.PressedOrHoveredBottom;
                }
                else if (isSorted || isSelected)
                {
                    bottomType = FreezableParts.SortedBottom;
                }

                SolidColorBrush bottomBrush = (SolidColorBrush)GetCachedFreezable((int)bottomType);
                if (bottomBrush == null)
                {
                    switch (bottomType)
                    {
                        case FreezableParts.NormalBottom:
                            bottomBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD5, 0xD5, 0xD5));
                            break;

                        case FreezableParts.PressedOrHoveredBottom:
                            bottomBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x93, 0xC9, 0xE3));
                            break;

                        case FreezableParts.SortedBottom:
                            bottomBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x96, 0xD9, 0xF9));
                            break;
                    }

                    bottomBrush.Freeze();

                    CacheFreezable(bottomBrush, (int)bottomType);
                }

                dc.DrawRectangle(bottomBrush, null, new Rect(0.0, size.Height - 1.0, size.Width, 1.0));
            }

            if (isSorted && (size.Width > 14.0) && (size.Height > 10.0))
            {
                // Draw the sort arrow
                TranslateTransform positionTransform = new TranslateTransform((size.Width - 8.0) * 0.5, 1.0);
                positionTransform.Freeze();
                dc.PushTransform(positionTransform);

                bool ascending = (sortDirection == ListSortDirection.Ascending);
                PathGeometry arrowGeometry = (PathGeometry)GetCachedFreezable(ascending ? (int)FreezableParts.ArrowUpGeometry : (int)FreezableParts.ArrowDownGeometry);
                if (arrowGeometry == null)
                {
                    arrowGeometry = new PathGeometry();
                    PathFigure arrowFigure = new PathFigure();

                    if (ascending)
                    {
                        arrowFigure.StartPoint = new Point(0.0, 4.0);

                        LineSegment line = new LineSegment(new Point(4.0, 0.0), false);
                        line.Freeze();
                        arrowFigure.Segments.Add(line);

                        line = new LineSegment(new Point(8.0, 4.0), false);
                        line.Freeze();
                        arrowFigure.Segments.Add(line);
                    }
                    else
                    {
                        arrowFigure.StartPoint = new Point(0.0, 0.0);

                        LineSegment line = new LineSegment(new Point(8.0, 0.0), false);
                        line.Freeze();
                        arrowFigure.Segments.Add(line);

                        line = new LineSegment(new Point(4.0, 4.0), false);
                        line.Freeze();
                        arrowFigure.Segments.Add(line);
                    }

                    arrowFigure.IsClosed = true;
                    arrowFigure.Freeze();

                    arrowGeometry.Figures.Add(arrowFigure);
                    arrowGeometry.Freeze();

                    CacheFreezable(arrowGeometry, ascending ? (int)FreezableParts.ArrowUpGeometry : (int)FreezableParts.ArrowDownGeometry);
                }

                // Draw two arrows, one inset in the other. This is to achieve a double gradient over both the border and the fill.
                LinearGradientBrush arrowBorder = (LinearGradientBrush)GetCachedFreezable((int)FreezableParts.ArrowBorder);
                if (arrowBorder == null)
                {
                    arrowBorder = new LinearGradientBrush();
                    arrowBorder.StartPoint = new Point();
                    arrowBorder.EndPoint = new Point(1.0, 1.0);
                    arrowBorder.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x3C, 0x5E, 0x72), 0.0));
                    arrowBorder.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x3C, 0x5E, 0x72), 0.1));
                    arrowBorder.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xC3, 0xE4, 0xF5), 1.0));
                    arrowBorder.Freeze();
                    CacheFreezable(arrowBorder, (int)FreezableParts.ArrowBorder);
                }

                dc.DrawGeometry(arrowBorder, null, arrowGeometry);

                LinearGradientBrush arrowFill = (LinearGradientBrush)GetCachedFreezable((int)FreezableParts.ArrowFill);
                if (arrowFill == null)
                {
                    arrowFill = new LinearGradientBrush();
                    arrowFill.StartPoint = new Point();
                    arrowFill.EndPoint = new Point(1.0, 1.0);
                    arrowFill.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x61, 0x96, 0xB6), 0.0));
                    arrowFill.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0x61, 0x96, 0xB6), 0.1));
                    arrowFill.GradientStops.Add(new GradientStop(Color.FromArgb(0xFF, 0xCA, 0xE6, 0xF5), 1.0));
                    arrowFill.Freeze();
                    CacheFreezable(arrowFill, (int)FreezableParts.ArrowFill);
                }

                // Inset the fill arrow inside the border arrow
                ScaleTransform arrowScale = (ScaleTransform)GetCachedFreezable((int)FreezableParts.ArrowFillScale);
                if (arrowScale == null)
                {
                    arrowScale = new ScaleTransform(0.75, 0.75, 3.5, 4.0);
                    arrowScale.Freeze();
                    CacheFreezable(arrowScale, (int)FreezableParts.ArrowFillScale);
                }

                dc.PushTransform(arrowScale);

                dc.DrawGeometry(arrowFill, null, arrowGeometry);

                dc.Pop(); // Scale Transform
                dc.Pop(); // Position Transform
            }

            if (horizontal)
            {
                dc.Pop(); // Horizontal Rotate
            }
        }

        private void RenderTheme(DrawingContext dc)
        {
            Size size = RenderSize;
            bool horizontal = Orientation == Orientation.Horizontal;
            bool isClickable = IsClickable && IsEnabled;
            bool isHovered = isClickable && IsHovered;
            bool isPressed = isClickable && IsPressed;
            ListSortDirection? sortDirection = SortDirection;
            bool isSorted = sortDirection != null;
            bool isSelected = IsSelected;

            EnsureCache((int)FreezableParts.NumFreezables);

            if (horizontal)
            {
                // When horizontal, rotate the rendering by -90 degrees
                Matrix m1 = new Matrix();
                m1.RotateAt(-90, 0, 0);
                Matrix m2 = new Matrix();
                m2.Translate(0, size.Height);

                var horizontalRotate = new MatrixTransform(m1 * m2);
                horizontalRotate.Freeze();
                dc.PushTransform(horizontalRotate);

                double temp = size.Width;
                size.Width = size.Height;
                size.Height = temp;
            }

            // Fill the background
            FreezableParts backgroundType = FreezableParts.NormalBackground;
            if (isPressed)
                backgroundType = FreezableParts.PressedBackground;
            else if (isHovered)
                backgroundType = FreezableParts.HoveredBackground;
            else if (isSelected)
                backgroundType = FreezableParts.PressedBackground;

            Brush background = (Brush)GetCachedFreezable((int)backgroundType);
            if (background == null)
            {
                switch (backgroundType)
                {
                    default:
                        background = new SolidColorBrush(
                            Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                        break;

                    case FreezableParts.HoveredBackground:
                        background = new SolidColorBrush(
                            Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                        break;

                    case FreezableParts.PressedBackground:
                        background = new SolidColorBrush(
                            Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                        break;
                }

                background.Freeze();

                CacheFreezable(background, (int)backgroundType);
            }

            dc.DrawRectangle(background, null, new Rect(size));

            if (size.Width >= 2.0)
            {
                // Draw the borders on the sides
                FreezableParts separatorType = FreezableParts.NormalSides;
                if (isPressed)
                    separatorType = FreezableParts.PressedSides;
                else if (isHovered)
                    separatorType = FreezableParts.HoveredSides;

                if (SeparatorVisibility == Visibility.Visible)
                {
                    Brush sepBrush;
                    if (SeparatorBrush != null)
                    {
                        sepBrush = SeparatorBrush;
                    }
                    else
                    {
                        sepBrush = (Brush)GetCachedFreezable((int)separatorType);
                        if (sepBrush == null)
                        {
                            switch (separatorType)
                            {
                                default:
                                    sepBrush = new SolidColorBrush(
                                        Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5));
                                    break;
                                case FreezableParts.HoveredSides:
                                    sepBrush = new SolidColorBrush(
                                        Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                                    break;
                                case FreezableParts.PressedSides:
                                    sepBrush = new SolidColorBrush(
                                        Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                                    break;
                            }

                            sepBrush.Freeze();
                            CacheFreezable(sepBrush, (int)separatorType);
                        }
                    }

                    if (Orientation == Orientation.Horizontal)
                        dc.DrawRectangle(sepBrush, null, new Rect(0.0, 0.0, 1.0, Math.Max(0, size.Height - 0.95)));
                    else
                        dc.DrawRectangle(sepBrush, null, new Rect(size.Width - 1, 0, 1, size.Height));
                }
            }

            if (size.Height >= 2.0)
            {
                // Separator and Border use the same brush.
                FreezableParts separatorType = FreezableParts.NormalSides;
                if (isPressed)
                    separatorType = FreezableParts.PressedSides;
                else if (isHovered)
                    separatorType = FreezableParts.HoveredSides;

                Brush sepBrush = (Brush)GetCachedFreezable((int)separatorType);
                if (sepBrush == null)
                {
                    switch (separatorType)
                    {
                        default:
                            sepBrush = new SolidColorBrush(
                                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5));
                            break;
                        case FreezableParts.HoveredSides:
                            sepBrush = new SolidColorBrush(
                                Color.FromArgb(0xFF, 0xD9, 0xEB, 0xF9));
                            break;
                        case FreezableParts.PressedSides:
                            sepBrush = new SolidColorBrush(
                                Color.FromArgb(0xFF, 0xBC, 0xDC, 0xF4));
                            break;
                    }

                    sepBrush.Freeze();
                    CacheFreezable(sepBrush, (int)separatorType);
                }

                dc.DrawRectangle(sepBrush, null, new Rect(0, size.Height - 1, size.Width, 1));
            }

            if (isSorted && size.Height > 10.0)
            {
                bool ascending = sortDirection == ListSortDirection.Ascending;
                Geometry arrowGeometry = (Geometry)GetCachedFreezable(ascending ? (int)FreezableParts.ArrowUpGeometry : (int)FreezableParts.ArrowDownGeometry);
                if (arrowGeometry == null)
                {
                    if (ascending)
                    {
                        // M 3,4 6.5,0.5 10,4 z (widen=1)
                        arrowGeometry = Geometry.Parse("F1M3.35,4.35L2.646,3.646 6.146,0.146 6.5,-0.207 10.354,3.646 9.646,4.354 6.146,0.854 6.5,0.5 6.854,0.854 3.354,4.354z");
                    }
                    else
                    {
                        // M 3,0 6.5,3.5 10,0 z (widen=1)
                        arrowGeometry = Geometry.Parse("F1M2.65,0.35L3.354,-0.354 6.854,3.146 6.5,3.5 6.146,3.146 9.646,-0.354 10.354,0.354 6.5,4.207 6.146,3.854 2.646,0.354z");
                    }

                    arrowGeometry.Freeze();

                    CacheFreezable(arrowGeometry, ascending ? (int)FreezableParts.ArrowUpGeometry : (int)FreezableParts.ArrowDownGeometry);
                }

                Brush arrowFill = (Brush)GetCachedFreezable((int)FreezableParts.ArrowFill);
                if (arrowFill == null)
                {
                    arrowFill = new SolidColorBrush(Color.FromArgb(0xDD, 0x33, 0x33, 0x33));
                    arrowFill.Freeze();
                    CacheFreezable(arrowFill, (int)FreezableParts.ArrowFill);
                }

                var offsetX = (size.Width - arrowGeometry.Bounds.Width - 2 * arrowGeometry.Bounds.X) * 0.5;
                var translation = new TranslateTransform(Math.Round(offsetX), 0);
                translation.Freeze();

                dc.PushTransform(translation);
                dc.DrawGeometry(arrowFill, null, arrowGeometry);
                dc.Pop();
            }

            if (horizontal)
            {
                dc.Pop(); // Horizontal Rotate
            }
        }

        #region FreezableCache
        /// <summary>
        ///     Creates a cache of frozen Freezable resources for use 
        ///     across all instances of the border.
        /// </summary>
        private static void EnsureCache(int size)
        {
            // Quick check to avoid locking
            if (_freezableCache == null)
            {
                lock (_cacheAccess)
                {
                    // Re-check in case another thread created the cache
                    if (_freezableCache == null)
                    {
                        _freezableCache = new List<Freezable>(size);
                        for (int i = 0; i < size; i++)
                        {
                            _freezableCache.Add(null);
                        }
                    }
                }
            }

            Debug.Assert(_freezableCache.Count == size, "The cache size does not match the requested amount.");
        }

        /// <summary>
        ///     Releases all resources in the cache.
        /// </summary>
        private static void ReleaseCache()
        {
            // Avoid locking if necessary
            if (_freezableCache != null)
            {
                lock (_cacheAccess)
                {
                    // No need to re-check if non-null since it's OK to set it to null multiple times
                    _freezableCache = null;
                }
            }
        }

        /// <summary>
        ///     Retrieves a cached resource.
        /// </summary>
        private static Freezable GetCachedFreezable(int index)
        {
            lock (_cacheAccess)
            {
                Freezable freezable = _freezableCache[index];
                Debug.Assert((freezable == null) || freezable.IsFrozen, "Cached Freezables should have been frozen.");
                return freezable;
            }
        }

        /// <summary>
        ///     Caches a resources.
        /// </summary>
        private static void CacheFreezable(Freezable freezable, int index)
        {
            Debug.Assert(freezable.IsFrozen, "Cached Freezables should be frozen.");

            lock (_cacheAccess)
            {
                if (_freezableCache[index] != null)
                {
                    _freezableCache[index] = freezable;
                }
            }
        }
        #endregion

        #endregion

        #region Nested Types
        private enum FreezableParts : int
        {
            NormalBevel,
            NormalBackground,
            PressedBackground,
            HoveredBackground,
            SortedBackground,
            PressedTop,
            NormalSides,
            PressedSides,
            HoveredSides,
            SortedSides,
            PressedBevel,
            NormalBottom,
            PressedOrHoveredBottom,
            SortedBottom,
            ArrowBorder,
            ArrowFill,
            ArrowFillScale,
            ArrowUpGeometry,
            ArrowDownGeometry,
            NumFreezables
        }
        #endregion
    }
}
