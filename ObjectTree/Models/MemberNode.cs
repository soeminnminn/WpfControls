using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ObjectTree.Models
{
    internal class MemberNode : ObjectNode
    {
        #region Variables
        private PropertyDescriptor _property = null;
        private FieldInfo _field = null;

        private string _pathRef = string.Empty;
        private object _value = null;
        private bool _isSelected = false;
        #endregion

        #region Properties
        public override string Name 
        { 
            get
            {
                if (_instance == null)
                    return "null";
                else if (_property != null)
                    return _property.Name;
                else if (_field != null)
                    return _field.Name;
                else
                    return base.Name;
            }
            protected set { base.Name = value; }
        }

        public override string Path
        {
            get => $"{Parent.Path}{Name}/";
        }

        public string PathRef
        {
            get => _pathRef.Replace("object:/", "ref:/");
        }

        public Type MemberType
        { 
            get 
            {
                if (_property != null)
                    return _property.PropertyType;

                else if (_field != null)
                    return _field.FieldType;

                else
                    return base.InstanceType;
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { SetProperty(ref _isSelected, value); }
        }

        public override bool IsReadOnly 
        {
            get 
            {
                if (_property != null)
                    return _property.IsReadOnly;
                else if (_field != null)
                    return true;
                else
                    return base.IsReadOnly;
            }
            protected set { base.IsReadOnly = value; }
        }

        public bool IsProperty
        {
            get => IsValue && _property != null;
        }

        public bool IsField
        {
            get => IsValue && _field != null;
        }

        public bool IsArray { get => (ValueType & ValueTypes.Array) == ValueTypes.Array; }

        public bool IsNotNullArray { get => IsArray && !IsNull; }

        public bool IsObject { get => (ValueType & ValueTypes.Object) == ValueTypes.Object; }

        public bool IsObjectRef { get => (ValueType & ValueTypes.Ref) == ValueTypes.Ref; }

        public bool IsNotRef { get => !IsObjectRef; }

        public bool IsValue { get => !IsArray && !IsObject && !IsObjectRef; }

        public bool IsNull { get => IsNotRef && (Value == null || (ValueType & ValueTypes.Null) == ValueTypes.Null); }

        public bool IsBlob { get => IsNotRef && !IsNull && ((ValueType & ValueTypes.Blob) == ValueTypes.Blob); }

        public bool IsString { get => IsNotRef && !IsNull && ((ValueType & ValueTypes.String) == ValueTypes.String); }

        public bool IsEnum { get => IsNotRef && !IsNull && ((ValueType & ValueTypes.Enum) == ValueTypes.Enum); }

        public bool IsPrimitive { get => IsNotRef && !IsNull && ((ValueType & ValueTypes.Boolean) == ValueTypes.Boolean || (ValueType & ValueTypes.Number) == ValueTypes.Number); }

        public override int Level { get => Parent.Level + 1; }

        public ObjectNode Parent { get; private set; }

        public object Value
        {
            get => _value;
            private set { SetProperty(ref _value, value); }
        }

        public string Text
        {
            get 
            {
                if (Value == null || (ValueType & ValueTypes.Null) == ValueTypes.Null)
                    return "null";

                else if ((ValueType & ValueTypes.Array) == ValueTypes.Array)
                    return GetCount(Value).ToString();

                else if ((ValueType & ValueTypes.Blob) == ValueTypes.Blob)
                    return GetBlobString(Value);

                else if ((ValueType & ValueTypes.Object) == ValueTypes.Object)
                    return GetObjectString(Value);

                else
                {
                    string val = Value.ToString();
                    return Regex.Replace(val, @"\r?\n", "\\n");
                }
            }
        }
        #endregion

        #region Constructors
        internal MemberNode(RootNode root, ObjectNode parent, object instance, PropertyDescriptor property)
            : base(root)
        {
            Parent = parent;
            _instance = instance;
            _property = property;

            if (instance != null)
            {
                if (instance is ICustomTypeDescriptor)
                    _instance = ((ICustomTypeDescriptor)instance).GetPropertyOwner(property);

                object value = null;
                try
                {
                    value = property.GetValue(instance);
                }
                catch
                { }

                Value = value;
            }

            ValueType = GetValueType(Value, MemberType, root.BlobLenMin);

            if (ValueType == ValueTypes.Object && !Root.RegisterInstance(Value, Path))
            {
                ValueType = ValueTypes.Ref;
                _pathRef = Root.RegisteredPath(Value);
            }
            else
                HasItems = HasMembers(Value, ValueType);
        }

        internal MemberNode(RootNode root, ObjectNode parent, object instance, FieldInfo field)
            : base(root)
        {
            Parent = parent;
            _instance = instance;
            _field = field;

            if (instance != null)
            {
                object value = null;
                try
                {
                    value = field.GetValue(instance);
                }
                catch
                { }

                Value = value;
            }

            ValueType = GetValueType(Value, MemberType, root.BlobLenMin);

            if (ValueType == ValueTypes.Object && !Root.RegisterInstance(Value, Path))
            {
                ValueType = ValueTypes.Ref;
                _pathRef = Root.RegisteredPath(Value);
            }
            else
                HasItems = HasMembers(Value, ValueType);
        }

        internal MemberNode(RootNode root, ObjectNode parent, object instance, string name, Type propertyType, object value)
            : base(root)
        {
            Parent = parent;
            _instance = instance;

            base.Name = name;
            base.InstanceType = propertyType;

            Value = value;
            ValueType = GetValueType(value, propertyType, root.BlobLenMin);

            if (ValueType == ValueTypes.Object && !Root.RegisterInstance(value, Path))
            {
                ValueType = ValueTypes.Ref;
                _pathRef = Root.RegisteredPath(value);
            }
            else
                HasItems = HasMembers(value, ValueType);
        }
        #endregion

        #region Methods
        internal override void LoadChildren()
        {
            if (!HasItems) return;
            var val = Value;

            if (val is IDictionary dictionary)
                LoadDictionary(val, dictionary);
            else if (val is IEnumerable enumerable)
                LoadEnumerable(val, enumerable.GetEnumerator());
            else
                LoadMembers(val);
        }
        #endregion
    }
}
