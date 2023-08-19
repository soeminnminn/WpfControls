using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;

namespace wspGridControl
{
    internal delegate void ColumnCollectionChangedEventHandler(object sender, ColumnCollectionChangedEventArgs e);

    /// <summary>
    /// Argument for ColumnCollectionChanged event
    /// </summary>
    public class ColumnCollectionChangedEventArgs : NotifyCollectionChangedEventArgs
    {
        #region Variables
        private string _propertyName;
        private GridColumn _column;
        private ReadOnlyCollection<GridColumn> _clearedColumns;
        private int _actualIndex = -1;
        #endregion

        #region Constructors
        /// <summary>
        /// constructor (for a property of one column changed)
        /// </summary>
        /// <param name="column">column whose property changed</param>
        /// <param name="propertyName">Name of the changed property</param>
        internal ColumnCollectionChangedEventArgs(GridColumn column, string propertyName)
            : base(NotifyCollectionChangedAction.Reset) // NotifyCollectionChangedEventArgs doesn't have 0 parameter constructor, so pass in an arbitrary parameter.
        {
            _column = column;
            _propertyName = propertyName;
        }

        /// <summary>
        /// constructor (for clear)
        /// </summary>
        /// <param name="action">must be NotifyCollectionChangedAction.Reset</param>
        /// <param name="clearedColumns">Columns removed in reset action</param>
        internal ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridColumn[] clearedColumns)
            : base(action)
        {
            _clearedColumns = Array.AsReadOnly(clearedColumns);
        }

        /// <summary>
        /// Construct for one-column Add/Remove event.
        /// </summary>
        internal ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridColumn changedItem, int index, int actualIndex)
            : base(action, changedItem, index)
        {
            Debug.Assert(action == NotifyCollectionChangedAction.Add || action == NotifyCollectionChangedAction.Remove,
                "This constructor only supports Add/Remove action.");
            Debug.Assert(changedItem != null, "changedItem can't be null");
            Debug.Assert(index >= 0, "index must >= 0");
            Debug.Assert(actualIndex >= 0, "actualIndex must >= 0");

            _actualIndex = actualIndex;
        }

        /// <summary>
        /// Construct for a one-column Replace event.
        /// </summary>
        internal ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridColumn newItem, GridColumn oldItem, int index, int actualIndex)
            : base(action, newItem, oldItem, index)
        {
            Debug.Assert(newItem != null, "newItem can't be null");
            Debug.Assert(oldItem != null, "oldItem can't be null");
            Debug.Assert(index >= 0, "index must >= 0");
            Debug.Assert(actualIndex >= 0, "actualIndex must >= 0");

            _actualIndex = actualIndex;
        }

        /// <summary>
        /// Construct for a one-column Move event.
        /// </summary>
        internal ColumnCollectionChangedEventArgs(NotifyCollectionChangedAction action, GridColumn changedItem, int index, int oldIndex, int actualIndex)
            : base(action, changedItem, index, oldIndex)
        {
            Debug.Assert(changedItem != null, "changedItem can't be null");
            Debug.Assert(index >= 0, "index must >= 0");
            Debug.Assert(oldIndex >= 0, "oldIndex must >= 0");
            Debug.Assert(actualIndex >= 0, "actualIndex must >= 0");

            _actualIndex = actualIndex;
        }
        #endregion

        #region Properties
        /// <summary>
        /// index of the changed column in the internal column list.
        /// </summary>
        internal int ActualIndex
        {
            get { return _actualIndex; }
        }

        /// <summary>
        /// Columns removed in reset action.
        /// </summary>
        internal ReadOnlyCollection<GridColumn> ClearedColumns
        {
            get { return _clearedColumns; }
        }

        /// <summary>
        /// Column whose property changed
        /// </summary>
        internal GridColumn Column
        {
            get { return _column; }
        }

        /// <summary>
        /// Name of the changed property
        /// </summary>
        internal string PropertyName
        {
            get { return _propertyName; }
        }
        #endregion
    }

    /// <summary>
    /// Manager for the GridColumnsCollection.CollectionChanged event.
    /// </summary>
    internal class ColumnsCollectionChangedEventManager : WeakEventManager
    {
        #region Constructors
        private ColumnsCollectionChangedEventManager()
        {
        }
        #endregion

        #region Properties
        // get the event manager for the current thread
        private static ColumnsCollectionChangedEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(ColumnsCollectionChangedEventManager);
                ColumnsCollectionChangedEventManager manager = (ColumnsCollectionChangedEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new ColumnsCollectionChangedEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add a listener to the given source's event.
        /// </summary>
        public static void AddListener(GridColumnsCollection source, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.ProtectedAddListener(source, listener);
        }

        /// <summary>
        /// Remove a listener to the given source's event.
        /// </summary>
        public static void RemoveListener(GridColumnsCollection source, IWeakEventListener listener)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (listener == null)
                throw new ArgumentNullException("listener");

            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        /// <summary>
        /// Add a handler for the given source's event.
        /// </summary>
        public static void AddHandler(GridColumnsCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        /// <summary>
        /// Remove a handler for the given source's event.
        /// </summary>
        public static void RemoveHandler(GridColumnsCollection source, EventHandler<NotifyCollectionChangedEventArgs> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        /// <summary>
        /// Return a new list to hold listeners to the event.
        /// </summary>
        protected override ListenerList NewListenerList()
        {
            return new ListenerList<NotifyCollectionChangedEventArgs>();
        }

        /// <summary>
        /// Listen to the given source for the event.
        /// </summary>
        protected override void StartListening(object source)
        {
            GridColumnsCollection typedSource = (GridColumnsCollection)source;
            typedSource.InternalCollectionChanged += new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }

        /// <summary>
        /// Stop listening to the given source for the event.
        /// </summary>
        protected override void StopListening(object source)
        {
            GridColumnsCollection typedSource = (GridColumnsCollection)source;
            typedSource.InternalCollectionChanged -= new NotifyCollectionChangedEventHandler(OnCollectionChanged);
        }

        // event handler for CollectionChanged event
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            DeliverEvent(sender, args);
        }
        #endregion
    }
}
