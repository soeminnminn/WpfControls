using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace ObjectTree.Models
{
    internal class RootNode : ObjectNode
    {
        #region Variables
        private readonly Dictionary<object, string> storedObjects = new Dictionary<object, string>();
        private int _blobLenMin = 0;
        #endregion

        #region Properties
        public override string Path
        {
            get => "object://";
        }

        internal int BlobLenMin
        {
            get => _blobLenMin;
            set { SetProperty(ref _blobLenMin, value); }
        }
        #endregion

        #region Constructors
        internal RootNode(object instance, int blobLenMin)
            : base()
        {
            _root = this;
            _instance = instance;
            _blobLenMin = blobLenMin;
            _type = instance == null ? typeof(object) : instance.GetType();
            _valueType = GetValueType(instance, _type, blobLenMin);
            _hasItems = HasMembers(instance, _valueType);

            if (instance != null)
            {
                Name = TypeDescriptor.GetClassName(instance);
                LoadAttributes(instance);
            }
            else
            {
                Name = "null";
            }
        }
        #endregion

        #region Methods
        internal override void LoadChildren()
        {
            if (!HasItems) return;
            var val = Instance;

            if (val is IDictionary dictionary)
                LoadDictionary(val, dictionary);
            else if (val is IEnumerable enumerable)
                LoadEnumerable(val, enumerable.GetEnumerator());
            else
                LoadMembers(val);
        }

        internal bool RegisterInstance(object obj, string path)
        {
            if (obj != null)
            {
                if (storedObjects.ContainsKey(obj))
                {
                    return storedObjects.TryGetValue(obj, out string storedPath) && storedPath == path;
                }

                storedObjects.Add(obj, path);
            }
            return true;
        }

        internal string RegisteredPath(object obj)
        {
            if (obj != null && storedObjects.TryGetValue(obj, out string path))
                return path;

            return string.Empty;
        }

        protected override void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                storedObjects.Clear();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
