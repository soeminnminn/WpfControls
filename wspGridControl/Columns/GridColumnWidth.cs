using System;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace wspGridControl
{
    [TypeConverter(typeof(GridColumnWidthConverter))]
    public class GridColumnWidth
    {
        #region Variables
        private readonly double _unitValue;      //  unit value storage
        private readonly GridColumnWidthType _unitType; //  unit type storage
        #endregion

        #region Constructors
        public GridColumnWidth(double pixels)
            : this(pixels, GridColumnWidthType.InPixels)
        {
        }

        public GridColumnWidth(double value, GridColumnWidthType type)
        {
            if (DoubleUtil.IsNaN(value))
            {
                throw new ArgumentException();
            }
            if (double.IsInfinity(value))
            {
                throw new ArgumentException();
            }

            _unitValue = value;
            _unitType = type;
        }
        #endregion

        #region Properties
        public bool IsInPixels
        {
            get => _unitType == GridColumnWidthType.InPixels;
        }

        public double Value
        {
            get => _unitValue;
        }

        public GridColumnWidthType UnitType
        {
            get => _unitType;
        }
        #endregion

        #region Methods
        public override bool Equals(object other)
        {
            if (other is double val)
                return Value == val && UnitType == GridColumnWidthType.InPixels;
            if (other is GridColumnWidth l)
                return UnitType == l.UnitType && Value == l.Value;
            else
                return false;
        }

        public override int GetHashCode()
        {
            return (int)_unitValue + (int)_unitType;
        }

        public override string ToString()
        {
            return GridColumnWidthConverter.ToString(this, CultureInfo.InvariantCulture);
        }
        #endregion
    }

    public class GridColumnWidthConverter: TypeConverter
    {
        #region Methods
        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            // We can only handle strings, integral and floating types
            TypeCode tc = Type.GetTypeCode(sourceType);
            switch (tc)
            {
                case TypeCode.String:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
        {
            return destinationType == typeof(InstanceDescriptor) || destinationType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
        {
            if (source != null)
            {
                if (source is string)
                {
                    return FromString((string)source, cultureInfo);
                }
                else
                {
                    //  conversion from numeric type
                    double value = Convert.ToDouble(source, cultureInfo);
                    return new GridColumnWidth(value, GridColumnWidthType.InPixels);
                }
            }
            throw GetConvertFromException(source);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (value != null && value is GridColumnWidth gl)
            {
                if (destinationType == typeof(string))
                {
                    return ToString(gl, cultureInfo);
                }

                if (destinationType == typeof(InstanceDescriptor))
                {
                    ConstructorInfo ci = typeof(GridColumnWidth).GetConstructor(new Type[] { typeof(double), typeof(GridColumnWidthType) });
                    return new InstanceDescriptor(ci, new object[] { gl.Value, gl.UnitType });
                }
            }
            throw GetConvertToException(value, destinationType);
        }

        internal static GridColumnWidth FromString(string s, CultureInfo cultureInfo)
        {
            string goodString = s.Trim().ToLowerInvariant();
            int strLen = goodString.Length;

            double value;
            GridColumnWidthType unit = GridColumnWidthType.InAverageFontChar;

            if (goodString.EndsWith("px", StringComparison.Ordinal))
            {
                unit = GridColumnWidthType.InPixels;
                string valueString = goodString.Substring(0, strLen - 2);
                value = Convert.ToDouble(valueString, cultureInfo);
            }
            else
            {
                value = Convert.ToDouble(goodString, cultureInfo);
            }

            return new GridColumnWidth(value, unit);
        }

        internal static string ToString(GridColumnWidth gl, CultureInfo cultureInfo)
        {
            string valueStr = Convert.ToString(gl.Value, cultureInfo);
            if (gl.UnitType == GridColumnWidthType.InPixels)
            {
                valueStr += "px";
            }
            return valueStr;
        }
        #endregion
    }
}
