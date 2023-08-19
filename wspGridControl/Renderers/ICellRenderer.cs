using System;
using System.Windows;
using System.Windows.Media;

namespace wspGridControl
{
    public interface ICellRenderer
    {
        HorizontalAlignment HorizontalAlignment { get; set; }

        VerticalAlignment VerticalAlignment { get; set; }

        void Draw(DrawingContext dc, object content, Rect bounds);
    }
}
