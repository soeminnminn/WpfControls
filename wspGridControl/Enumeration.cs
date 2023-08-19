using System;

namespace wspGridControl
{
    [Flags]
    internal enum RelativeMousePositions
    {
        Over = 0x00,
        Above = 0x01,
        Below = 0x02,
        Left = 0x04,
        Right = 0x08,
    }

    internal enum ColumnMeasureState
    {
        /// <summary>
        /// Column width is just initialized and will size to content width
        /// </summary>
        Init = 0,

        /// <summary>
        /// Column width reach max desired width of header(s) in this column
        /// </summary>
        Headered = 1,

        /// <summary>
        /// Column width reach max desired width of data row(s) in this column
        /// </summary>
        Data = 2,

        /// <summary>
        /// Column has a specific value as width
        /// </summary>
        SpecificWidth = 3
    }

    public enum GridColumnHeaderRole
    {
        /// <summary>
        /// The normal header
        /// </summary>
        Normal,
        /// <summary>
        /// The floating header (when dragging a header)
        /// </summary>
        Floating,
        /// <summary>
        /// The padding header (the very last header in header bar)
        /// </summary>
        Padding
    }

    public enum GridColumnWidthType
    {
        InPixels,
        InAverageFontChar
    }
}
