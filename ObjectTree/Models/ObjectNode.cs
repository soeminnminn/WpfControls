using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ObjectTree.Models
{
    public abstract class ObjectNode : ObservableObject
    {
        #region Variables
        internal RootNode _root = null;
        internal object _instance = null;
        internal ValueTypes _valueType = ValueTypes.Unknown;
        private string _name = string.Empty;
        private string _category = string.Empty;
        private string _description = string.Empty;
        internal Type _type = typeof(object);
        internal bool _hasItems = false;
        private bool _isReadOnly = false;
        private bool _isExpanded = false;

        private ObservableCollection<ObjectNode> _children = new ObservableCollection<ObjectNode>();
        #endregion

        #region Properties
        internal virtual RootNode Root
        {
            get => _root;
            set { _root = value; }
        }

        internal virtual object Instance
        {
            get => _instance;
            set { SetProperty(ref _instance, value); }
        }

        internal virtual bool HasItems
        {
            get => _hasItems;
            set { SetProperty(ref _hasItems, value); }
        }

        internal virtual bool IsExpanded
        {
            get => _isExpanded;
            set { SetProperty(ref _isExpanded, value); }
        }

        public virtual int Level { get => 0; }

        public virtual string Name
        {
            get => _name;
            protected set { SetProperty(ref _name, value); }
        }

        public abstract string Path
        {
            get;
        }

        public virtual string Category
        {
            get => _category;
            protected set { SetProperty(ref _category, value); }
        }

        public virtual string Description
        {
            get => _description;
            protected set { SetProperty(ref _description, value); }
        }

        public virtual Type InstanceType
        {
            get => _type;
            protected set { SetProperty(ref _type, value); }
        }

        internal virtual ValueTypes ValueType
        {
            get => _valueType;
            set { SetProperty(ref _valueType, value); }
        }

        public virtual bool IsWriteable
        {
            get => !_isReadOnly;
            protected set { }
        }

        public virtual bool IsReadOnly
        {
            get => _isReadOnly;
            protected set { SetProperty(ref _isReadOnly, value); }
        }

        internal virtual ObservableCollection<ObjectNode> Children
        {
            get => _children;
        }
        #endregion

        #region Constructor
        internal ObjectNode()
        {
            Name = "null";
        }

        internal ObjectNode(RootNode root)
            : this()
        {
            _root = root;
        }
        #endregion

        #region Methods
        internal abstract void LoadChildren();

        internal virtual void LoadAttributes(object instance)
        {
            if (instance == null) return;

            var attributes = TypeDescriptor.GetAttributes(instance);

            if (attributes[typeof(DisplayNameAttribute)] is DisplayNameAttribute dnAttr)
                Name = dnAttr.DisplayName;

            if (attributes[typeof(CategoryAttribute)] is CategoryAttribute cAttr)
                Category = cAttr.Category;

            if (attributes[typeof(DescriptionAttribute)] is DescriptionAttribute dAttr)
                Description = dAttr.Description;
        }

        internal virtual void LoadMembers(object instance)
        {
            if (instance == null) return;
            var list = new List<MemberNode>();

            try
            {
                var fields = instance.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var browsable = field.GetCustomAttribute<BrowsableAttribute>();
                    if (browsable != null && !browsable.Browsable) continue;

                    var member = new MemberNode(_root, this, instance, field);
                    list.Add(member);
                }
            }
            catch (Exception)
            { }

            try
            {
                var properties = TypeDescriptor.GetProperties(instance.GetType());
                foreach (PropertyDescriptor descriptor in properties)
                {
                    if (descriptor.IsBrowsable)
                    {
                        var member = new MemberNode(_root, this, instance, descriptor);
                        list.Add(member);
                    }
                }
            }
            catch(Exception)
            { }

            list.Sort(new ByNameComparer());
            _children = new ObservableCollection<ObjectNode>(list);
        }

        internal virtual void LoadDictionary(object instance, IDictionary dictionary)
        {
            if (dictionary == null) return;

            try
            {
                var list = new List<MemberNode>();
                foreach (var key in dictionary.Keys)
                {
                    var val = dictionary[key];
                    var property = new MemberNode(_root, this, instance, key.ToString(), val == null ? null : val.GetType(), val);
                    list.Add(property);
                }
                _children = new ObservableCollection<ObjectNode>(list);
            }
            catch(Exception)
            { }
        }

        internal virtual void LoadEnumerable(object instance, IEnumerator enumerator)
        {
            if (enumerator == null) return;

            try
            {
                var list = new List<MemberNode>();
                int i = 0;
                while (enumerator.MoveNext())
                {
                    var v = enumerator.Current;
                    var property = new MemberNode(_root, this, instance, i.ToString(), v.GetType(), v);
                    list.Add(property);
                    i++;
                }
                _children = new ObservableCollection<ObjectNode>(list);
            }
            catch(Exception)
            { }
        }

        public override string ToString()
        {
            return Name;
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                foreach (ObservableObject child in Children)
                {
                    child.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        internal static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    return false;
                default:
                    return false;
            }
        }

        internal static ValueTypes GetValueType(object val, Type type = null, int blobMin = 0)
        {
            ValueTypes valueType = ValueTypes.Unknown;
            Type typeOfVal = null;

            if (val == null)
                valueType |= ValueTypes.Null;
            
            if (type != null)
                typeOfVal = type;
            else if (val != null)
                typeOfVal = val.GetType();

            if (typeOfVal == null) return valueType;

            TypeCode typeCode = Type.GetTypeCode(typeOfVal);

            if (typeCode == TypeCode.DBNull)
                valueType |= ValueTypes.Null;

            else if (typeCode == TypeCode.DateTime || typeCode == TypeCode.String || typeCode == TypeCode.Char)
                valueType |= ValueTypes.String;

            else if (typeCode == TypeCode.Boolean)
                valueType |= ValueTypes.Boolean;

            else if (typeOfVal.IsEnum)
                valueType |= ValueTypes.Enum;

            else if (IsNumericType(typeOfVal))
                valueType |= ValueTypes.Number;

            else if (typeOfVal.IsPrimitive && val != null)
            {
                var str = val.ToString();
                if (typeCode != TypeCode.String && Regex.IsMatch(str, @"^(([0-9-]*)|(([0-9-]*)\.([0-9]*)))$"))
                    valueType |= ValueTypes.Number;

                else if (typeCode == TypeCode.Boolean)
                    valueType |= ValueTypes.Boolean;

                else
                    valueType |= ValueTypes.String;
            }

            else if (val is IEnumerable || typeOfVal.IsArray)
            {
                if (val is byte[] || val is sbyte[] || val is char[])
                {
                    var count = GetCount(val);

                    if (count > blobMin)
                        valueType |= ValueTypes.Blob;
                    else
                        valueType |= ValueTypes.Array;
                }
                else
                    valueType |= ValueTypes.Array;
            }
            else if (typeCode == TypeCode.Object || typeOfVal.IsClass || typeOfVal.IsInterface || typeOfVal.IsValueType)
                valueType |= ValueTypes.Object;

            return valueType;
        }

        internal static bool HasMembers(object val, ValueTypes valueType)
        {
            if (val == null) return false;

            if ((valueType & ValueTypes.Array) == ValueTypes.Array)
            {
                var count = GetCount(val);
                return count > 0;
            }
            else if ((valueType & ValueTypes.Object) == ValueTypes.Object)
            {
                int count = 0;
                
                try
                {
                    var properties = TypeDescriptor.GetProperties(val);
                    count = properties.Count;
                }
                catch(Exception) { }

                try
                {
                    var fields = val.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    count += fields.Length;
                }
                catch(Exception)
                { }

                return count > 0;
            }

            return false;
        }

        internal static int GetCount(object value)
        {
            if (value == null) return 0;

            try
            {
                var properties = TypeDescriptor.GetProperties(value);

                var count = properties["Count"];
                if (count != null && count.GetValue(value) is int c)
                    return c;

                var length = properties["Length"];
                if (length != null && length.GetValue(value) is int l)
                    return l;
            }
            catch
            { }

            return 0;
        }

        internal static string GetBlobString(object value)
        {
            if (value == null)
                return "null";

            var bytes = value as byte[];
            if (bytes != null)
                return Convert.ToBase64String(bytes);

            return "…";
        }

        internal static string GetObjectString(object value)
        {
            if (value is Type || value is Assembly || value is Module) return string.Empty;

            Type valueType = value.GetType();
            if (valueType.IsGenericType) return string.Empty;

            var attributes = TypeDescriptor.GetAttributes(value);
            if (attributes[typeof(DebuggerDisplayAttribute)] is DebuggerDisplayAttribute ddAttr)
            {
                string memberName = ddAttr.Value;
                if (!string.IsNullOrEmpty(memberName))
                {
                    try
                    {
                        var pInfo = valueType.GetProperty(memberName);
                        if (pInfo != null && pInfo.CanRead)
                        {
                            var pValue = pInfo.GetValue(value);
                            if (pValue != null) return pValue.ToString();
                        }
                    }
                    catch (Exception) { }

                    try
                    {
                        var fInfo = valueType.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
                        if (fInfo != null)
                        {
                            var fValue = fInfo.GetValue(value);
                            if (fValue != null) return fValue.ToString();
                        }
                    }
                    catch (Exception)
                    { }
                }
            }

            string typeName = valueType.Name;
            string fullName = valueType.FullName;

            string val = value.ToString();
            if (val == fullName || val == typeName) return string.Empty;

            return val;
        }
        #endregion

        #region Nested Types
        private class ByNameComparer : IComparer<ObjectNode>
        {
            public int Compare(ObjectNode x, ObjectNode y)
            {
                if (x == null || y == null)
                    return 0;

                if (x == y)
                    return 0;

                return x.Name.CompareTo(y.Name);
            }
        }
        #endregion
    }
}
