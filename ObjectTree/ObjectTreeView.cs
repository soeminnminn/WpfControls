using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ObjectTree.Models;

namespace ObjectTree
{
    /// <summary>
    /// Represents a Object's hierarchical.
    /// </summary>
    [TemplatePart(Name = PART_Scroller, Type = typeof(ScrollViewer))]
    [TemplatePart(Name = PART_Splitter, Type = typeof(Thumb))]
    public class ObjectTreeView : ListBox
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="Indentation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IndentationProperty = DependencyProperty.Register(
            nameof(Indentation), typeof(double), typeof(ObjectTreeView),
            new UIPropertyMetadata(19.0, (s, e) => ((ObjectTreeView)s).OnIndentationChanged(e)));

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(object), typeof(ObjectTreeView),
            new UIPropertyMetadata(null, (s, e) => ((ObjectTreeView)s).OnSourceChanged(e)));

        /// <summary>
        /// Identifies the <see cref="SplitPosition"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SplitPositionProperty = DependencyProperty.Register(
            nameof(SplitPosition), typeof(double), typeof(ObjectTreeView),
            new UIPropertyMetadata(200.0, (s, e) => ((ObjectTreeView)s).OnSplitPositionChanged(e)));

        /// <summary>
        /// Identifies the <see cref="BlobLengthMin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BlobLengthMinProperty = DependencyProperty.Register(
            nameof(BlobLengthMin), typeof(int), typeof(ObjectTreeView),
            new UIPropertyMetadata(32, (s, e) => ((ObjectTreeView)s).OnBlobLengthMinChanged(e)));

        /// <summary>
        /// Identifies the <see cref="SelectedNode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedNodeProperty = DependencyProperty.Register(
            nameof(SelectedNode), typeof(ObjectNode), typeof(ObjectTreeView),
            new UIPropertyMetadata(null, (s, e) => ((ObjectTreeView)s).OnSelectedNodeChanged(e)));
        #endregion

        #region Variables
        private const string PART_Scroller = "PART_Scroller";
        private const string PART_Splitter = "PART_Splitter";

        private readonly Dictionary<object, bool> isExpandedMap = new Dictionary<object, bool>();

        private ObjectNode root = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes static members of the <see cref="ObjectTreeView" /> class.
        /// </summary>
        static ObjectTreeView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ObjectTreeView), new FrameworkPropertyMetadata(typeof(ObjectTreeView)));
        }

        /// <summary>
        /// Initializes members of the ObjectTreeView class.
        /// </summary>
        public ObjectTreeView()
        {
            base.SelectionMode = SelectionMode.Single;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <value>The source.</value>
        public object Source
        {
            get => (IEnumerable)GetValue(SourceProperty);
            set { SetValue(SourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the split position.
        /// </summary>
        /// <value>The split position.</value>
        public double SplitPosition
        {
            get => (double)GetValue(SplitPositionProperty);
            set { SetValue(SplitPositionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the indentation.
        /// </summary>
        /// <value>The indentation.</value>
        public double Indentation
        {
            get => (double)GetValue(IndentationProperty);
            set { SetValue(IndentationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the BlobLengthMin.
        /// </summary>
        /// <value>The Blob minimun length.</value>
        public int BlobLengthMin
        {
            get => (int)GetValue(BlobLengthMinProperty);
            set { SetValue(BlobLengthMinProperty, value); }
        }

        /// <summary>
        /// Gets or sets the SelectedNode.
        /// </summary>
        /// <value>The selected node.</value>
        public ObjectNode SelectedNode
        {
            get => (ObjectNode)GetValue(SelectedNodeProperty);
            set { SetValue(SelectedNodeProperty, value); }
        }

        [Browsable(false)]
        public new SelectionMode SelectionMode
        {
            get { return base.SelectionMode; }
            set { base.SelectionMode = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or internal processes call <see
        /// cref="M:System.Windows.FrameworkElement.ApplyTemplate" /> .
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template.FindName(PART_Splitter, this) is GridSplitter splitter)
            {
                splitter.DragDelta += PART_Splitter_DragDelta;
            }

            ApplyIndentationChanged();
        }

        private void PART_Splitter_DragDelta(object sender, DragDeltaEventArgs e)
        {
            SplitPosition = Math.Max(0.0, SplitPosition + e.HorizontalChange);
        }

        private void ApplyIndentationChanged()
        {
            foreach (var item in Items)
            {
                var container = GetContainerFromItem(item);
                if (container == null)
                    continue;

                container.OnLevelOrIndentationChanged();
            }
        }

        private void ApplySourceChanged(object oldValue, object newValue)
        {
            if (base.SelectionMode != SelectionMode.Single)
            {
                foreach (var item in Items)
                {
                    var container = GetContainerFromItem(item);
                    if (container == null) continue;

                    if (container.IsSelected)
                    {
                        SelectedItems.Remove(item);
                    }
                }
            }

            Items.Clear();
            isExpandedMap.Clear();

            if (newValue != null)
            {
                root = new RootNode(newValue, BlobLengthMin);
                root.LoadChildren();

                foreach (var item in root.Children)
                {
                    InsertItem(Items.Count, item);
                }
            }
        }

        internal ObjectTreeViewItem ContainerFromIndex(int index)
        {
            return this.ItemContainerGenerator.ContainerFromIndex(index) as ObjectTreeViewItem;
        }

        internal ObjectTreeViewItem GetContainerFromItem(object item)
        {
            return (ObjectTreeViewItem)ItemContainerGenerator.ContainerFromItem(item);
        }

        /// <summary>
        /// Creates or identifies the element used to display a specified item.
        /// </summary>
        /// <returns>
        /// A <see cref="TreeListBoxItem" /> .
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ObjectTreeViewItem();
        }

        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own ItemContainer.
        /// </summary>
        /// <param name="item">Specified item.</param>
        /// <returns>
        /// <c>true</c> if the item is its own ItemContainer; otherwise, <c>false</c>.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ObjectTreeViewItem;
        }

        /// <summary>
        /// Prepares the specified element to display the specified item.
        /// </summary>
        /// <param name="element">Container used to display the specified item.</param>
        /// <param name="item">The item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            var container = element as ObjectTreeViewItem;
            if (container == null)
            {
                throw new InvalidOperationException("Container not created.");
            }

            var property = item as MemberNode;
            if (property == null)
            {
                throw new InvalidOperationException("Item not specified.");
            }

            property.IsExpanded = isExpandedMap[property];
            container.SetProperty(property);
        }

        /// <summary>
        /// Provides an appropriate <see cref="T:System.Windows.Automation.Peers.ListBoxAutomationPeer" /> implementation for this control, as part of the WPF automation infrastructure.
        /// </summary>
        /// <returns>The type-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer" /> implementation.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ObjectTreeViewAutomationPeer(this);
        }

        /// <summary>
        /// <see cref="System.Windows.Controls.ListBox.OnKeyDown(KeyEventArgs)" /> .
        /// </summary>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            var control = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            var item = SelectedIndex >= 0 ? Items[SelectedIndex] : null;

            switch (e.Key)
            {
                case Key.Left:
                    if (control)
                    {
                        // Collapse all items
                        foreach (var topLevelItem in Items)
                        {
                            Collapse(topLevelItem);
                        }
                    }
                    else if (!isExpandedMap[item] && item is MemberNode node && node.Parent != root)
                    {
                        var idx = Math.Max(0, SelectedIndex - 1);
                        var container = GetContainerFromItem(Items[idx]);
                        if (container != null)
                        {
                            container.IsSelected = true;
                            container.Focus();
                        }
                    }
                    else
                        Collapse(item);

                    e.Handled = true;
                    break;
                case Key.Right:
                    if (control)
                    {
                        // Expand all items
                        foreach (var i in Items)
                        {
                            Expand(i);
                        }
                    }
                    else if (isExpandedMap[item] && item is MemberNode node && node.HasItems)
                    {
                        var idx = Math.Min(Items.Count - 1, SelectedIndex + 1);
                        var container = GetContainerFromItem(Items[idx]);
                        if (container != null)
                        {
                            container.IsSelected = true;
                            container.Focus();
                        }
                    }
                    else
                        Expand(item);

                    e.Handled = true;
                    break;
                case Key.Enter:
                    e.Handled = ToggleExpandCollapse(item);
                    break;
            }

            base.OnKeyDown(e);
        }

        /// <summary>
        /// Handles changes in the HierarchySource.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            ApplySourceChanged(e.OldValue, e.NewValue);
        }

        /// <summary>
        /// Handles changes in the SplitPosition.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnSplitPositionChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        /// <summary>
        /// Handles changes in indentation.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnIndentationChanged(DependencyPropertyChangedEventArgs e)
        {
            ApplyIndentationChanged();
        }

        /// <summary>
        /// Handles changes in BlobLengthMin.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnBlobLengthMinChanged(DependencyPropertyChangedEventArgs e)
        {
            if (root != null)
            {
                ApplySourceChanged(null, root.Instance);
            }
        }

        /// <summary>
        /// Handles changes in SelectedNode.
        /// </summary>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        protected virtual void OnSelectedNodeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue != SelectedItem)
            {
                SelectedItem = e.NewValue;
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            SelectedNode = SelectedItem as ObjectNode;
            base.OnSelectionChanged(e);
        }

        /// <summary>
        /// Expands / Collapses the specified item.
        /// </summary>
        /// <param name="item">The item to expand or collapse.</param>
        internal bool ToggleExpandCollapse(object item)
        {
            if (item is MemberNode node && isExpandedMap.ContainsKey(node))
            {
                if (isExpandedMap[node])
                    Collapse(node);
                else
                    Expand(node);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Collapses the specified item.
        /// </summary>
        /// <param name="item">The item to collapse.</param>
        internal void Collapse(object item)
        {
            if (item is MemberNode node)
            {
                if (!isExpandedMap[node])
                    return;

                var children = node.Children;
                if (children.Count == 0)
                    return;

                RemoveItems(children);

                isExpandedMap[node] = false;

                var container = GetContainerFromItem(node);
                if (container != null)
                {
                    // Update the IsExpanded flag
                    container.IsExpanded = false;
                }
            }
        }

        /// <summary>
        /// Expands the specified item.
        /// </summary>
        /// <param name="item">The item to expand.</param>
        internal void Expand(object item)
        {
            if (item is MemberNode node)
            {
                if (isExpandedMap[node])
                    return;

                node.LoadChildren();
                var children = node.Children;
                if (children.Count == 0)
                    return;

                InsertItems(node, children, 0);
                isExpandedMap[node] = true;

                var container = GetContainerFromItem(node);
                if (container != null)
                {
                    // Update the IsExpanded flag
                    container.IsExpanded = true;
                }
            }
        }

        private void RemoveItems(IList<ObjectNode> itemsToRemove)
        {
            var queue = new Queue<ObjectNode>(itemsToRemove);
            while (queue.Count > 0)
            {
                var item = queue.Dequeue();
                if (Items.Contains(item))
                {
                    if (isExpandedMap[item])
                    {
                        var children = item.Children;
                        foreach (var child in children)
                        {
                            queue.Enqueue(child);
                        }
                    }

                    RemoveItem(item);
                }
            }
        }

        private void RemoveItem(ObjectNode item)
        {
            Items.Remove(item);
            isExpandedMap.Remove(item);
        }

        private void InsertItems(ObjectNode parent, IList<ObjectNode> newItems, int newStartingIndex)
        {
            var parentChildren = parent.Children;

            // Find the index where the new items should be added
            int index;
            if (newStartingIndex + newItems.Count < parentChildren.Count)
            {
                // inserted items should be added just before the next item in the collection
                // note that the items have already been added to the collection, so we need to add the newItems.Count
                var followingChild = parentChildren[newStartingIndex + newItems.Count];
                index = Items.IndexOf(followingChild);
            }
            else
            {
                // added items should be added before the next sibling of the parent
                var parentSibling = GetNextParentSibling(parent);
                if (parentSibling == null)
                {
                    // No sibling found, so add at the end of the list.
                    index = Items.Count;
                }
                else
                {
                    // Found the sibling, so add the items before this item.
                    index = Items.IndexOf(parentSibling);
                }
            }

            if (index > -1)
            {
                foreach (var item in newItems)
                {
                    InsertItem(index++, item);
                }
            }
        }

        private void InsertItem(int index, ObjectNode item)
        {
            isExpandedMap[item] = false;
            try
            {
                Items.Insert(index, item);
            }
            catch
            {
            }
        }

        internal void SelectRefrence(object item)
        {
            if (item is MemberNode node)
            {
                var path = node.PathRef.Replace("ref:/", "object:/");
                var splitedPath = path.Split('/').Where(x => x != "").ToArray();
                if (splitedPath.Length > 1)
                {
                    var currPath = $"{splitedPath[0]}//{splitedPath[1]}/";
                    var refNode = Items.Cast<ObjectNode>().First(x => x.Path == currPath);

                    int i = 2;
                    while(refNode != null && currPath != path && i < splitedPath.Length)
                    {
                        Expand(refNode);
                        currPath = $"{currPath}{splitedPath[i]}/";
                        refNode = refNode.Children.FirstOrDefault(x => x.Path == currPath);
                        i++;
                    }

                    var container = GetContainerFromItem(refNode);
                    if (container != null)
                    {
                        container.IsSelected = true;
                        container.Focus();
                    }
                    else
                    {
                        var idx = Items.IndexOf(refNode);
                        SelectedIndex = idx;
                    }

                    ScrollIntoView(SelectedItem);
                }
            }
        }

        private ObjectNode GetNextParentSibling(ObjectNode item)
        {
            if (item == root)
                return null;

            if (item is MemberNode node)
            {
                var parentItem = node.Parent;
                var parentChildren = parentItem.Children;

                int index = parentChildren.IndexOf(item);
                if (index + 1 < parentChildren.Count)
                {
                    return parentChildren[index + 1];
                }

                return GetNextParentSibling(parentItem);
            }

            return null;
        }
        #endregion
    }
}
