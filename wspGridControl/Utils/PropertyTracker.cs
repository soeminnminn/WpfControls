using System;
using System.Collections.Generic;
using System.Windows;
using System.Reflection;

namespace wspGridControl
{
    internal class PropertyTracker : IDisposable
    {
        #region Variables
        private readonly Dictionary<DependencyProperty, List<DependencyObject>> _listeners;
        private readonly DependencyObject _owner;
        #endregion

        #region Events
        private event DependencyPropertyChangedEventHandler _dependencyPropertyChanged = null;

        public event DependencyPropertyChangedEventHandler DependencyPropertyChanged
        {
            add { _dependencyPropertyChanged += value; }
            remove { _dependencyPropertyChanged -= value; }
        }
        #endregion

        #region Constructor
        public PropertyTracker(DependencyObject owner)
            : this(owner, GetDependencyProperties(owner.GetType()))
        {  
        }

        public PropertyTracker(DependencyObject owner, params DependencyProperty[] properties)
        {
            _owner = owner;
            if (properties.Length == 0)
            {
                _listeners = new Dictionary<DependencyProperty, List<DependencyObject>>();
            }
            else
            {
                _listeners = new Dictionary<DependencyProperty, List<DependencyObject>>(properties.Length);
                foreach (var dp in properties)
                {
                    if (dp == null) continue;
                    _listeners.Add(dp, new List<DependencyObject>());
                }
            }
        }
        #endregion

        #region Methods
        private static DependencyProperty[] GetDependencyProperties(Type owner)
        {
            DependencyProperty[] arr = new DependencyProperty[0];
            var fields = owner.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (fields != null && fields.Length > 0)
            {
                arr = new DependencyProperty[fields.Length];
                for(var i = 0; i < fields.Length; i++)
                {
                    arr[i] = fields[i].GetValue(null) as DependencyProperty;
                }
            }

            return arr;
        }

        public void AddListener(DependencyObject listener)
        {
            var listenerType = listener.GetType();
            var properties = GetDependencyProperties(listenerType);
            AddListener(listener, properties);
        }

        public void AddListener<T>(DependencyObject listener)
        {
            var listenerType = listener.GetType();
            var properties = GetDependencyProperties(listenerType);
            if (listenerType != typeof(T))
            {
                var baseProperties = GetDependencyProperties(typeof(T));
                var arrProperties = new DependencyProperty[properties.Length + baseProperties.Length];
                Array.Copy(baseProperties, arrProperties, baseProperties.Length);
                Array.Copy(properties, 0, arrProperties, baseProperties.Length, properties.Length);
                properties = arrProperties;
            }

            AddListener(listener, properties);
        }

        public void AddListener(DependencyObject listener, params Type[] inheritTypes)
        {
            var listenerType = listener.GetType();
            var properties = GetDependencyProperties(listenerType);
            var list = new List<DependencyProperty>(properties);

            foreach (var type in inheritTypes)
            {
                var inheritProperties = GetDependencyProperties(type);
                list.AddRange(inheritProperties);
            }

            AddListener(listener, list.ToArray());
        }

        public void AddListener(DependencyObject listener, params DependencyProperty[] properties)
        {
            foreach (var dp in properties)
            {
                if (dp == null) continue;
                if (dp.ReadOnly) continue;

                if (_listeners.TryGetValue(dp, out List<DependencyObject> list))
                {
                    list.Add(listener);
                    SyncValue(dp, listener);
                }
            }
        }

        public void Dispose()
        {
        }

        public void NotifyPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            DependencyProperty dp = args.Property;
            bool isDefaultValue = DependencyPropertyHelper.GetValueSource(_owner, dp).BaseValueSource == BaseValueSource.Default;

            if (_listeners.TryGetValue(dp, out List<DependencyObject> list))
            {
                foreach (var listener in list)
                {
                    try
                    {
                        if (isDefaultValue)
                        {
                            listener.ClearValue(dp);
                        }
                        else
                        {
                            listener.SetValue(dp, args.NewValue);
                        }
                    }
                    catch(Exception)
                    { }
                }
            }

            OnDependencyPropertyChanged(args);
        }

        private void SyncValue(DependencyProperty dp, DependencyObject listener)
        {
            if (dp.ReadOnly) return;

            bool isDefaultValue = DependencyPropertyHelper.GetValueSource(_owner, dp).BaseValueSource == BaseValueSource.Default;
            if (isDefaultValue)
            {
                listener.ClearValue(dp);
            }
            else
            {
                listener.SetValue(dp, _owner.GetValue(dp));
            }
        }

        private void OnDependencyPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if (_dependencyPropertyChanged != null)
            {
                _dependencyPropertyChanged(_owner, args);
            }
        }
        #endregion
    }
}
