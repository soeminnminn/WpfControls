using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ObjectTree.Models;

namespace ObjectTree
{
    /// <summary>
    /// Represents a container for items in the <see cref="ObjectTreeView" /> .
    /// </summary>
    internal sealed class ObjectTreeViewItem : ListBoxItem
    {
        #region Dependency Properties
        /// <summary>
        /// Identifies the <see cref="HasItems"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasItemsProperty = DependencyProperty.Register(
            nameof(HasItems), typeof(bool), typeof(ObjectTreeViewItem),
            new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsExpanded"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
            nameof(IsExpanded), typeof(bool), typeof(ObjectTreeViewItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (s, e) => ((ObjectTreeViewItem)s).OnIsExpandedChanged()));

        /// <summary>
        /// Identifies the <see cref="LevelPadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LevelPaddingProperty = DependencyProperty.Register(
            nameof(LevelPadding), typeof(Thickness), typeof(ObjectTreeViewItem));

        /// <summary>
        /// Identifies the <see cref="ValuePadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValuePaddingProperty = DependencyProperty.Register(
            nameof(ValuePadding), typeof(Thickness), typeof(ObjectTreeViewItem));

        /// <summary>
        /// Identifies the <see cref="Level"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register(
            nameof(Level), typeof(int), typeof(ObjectTreeViewItem),
            new UIPropertyMetadata(0, (s, e) => ((ObjectTreeViewItem)s).OnLevelOrIndentationChanged()));
        #endregion

        #region Properties
        /// <summary>
        /// Gets the expand toggle command.
        /// </summary>
        /// <value>The command.</value>
        public ICommand ToggleExpandCommand
        {
            get => new DelegateCommand(() => IsExpanded = !IsExpanded);
        }

        /// <summary>
        /// Gets the refrence object link clicked command.
        /// </summary>
        /// <value>The command.</value>
        public ICommand ClickRefCommand
        {
            get => new DelegateCommand(OnClickRef);
        }

        /// <summary>
        /// Gets or sets a value indicating whether this item has child items.
        /// </summary>
        public bool HasItems
        {
            get => (bool)GetValue(HasItemsProperty);
            set { SetValue(HasItemsProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get => (bool)GetValue(IsExpandedProperty);
            set { SetValue(IsExpandedProperty, value); }
        }

        /// <summary>
        /// Gets or sets the hierarchy level of the item.
        /// </summary>
        /// <value>The level.</value>
        public int Level
        {
            get => (int)GetValue(LevelProperty);
            set { SetValue(LevelProperty, value); }
        }

        /// <summary>
        /// Gets the padding due to hierarchy level and the parent control Indentation.
        /// </summary>
        /// <value>The level padding.</value>
        public Thickness LevelPadding
        {
            get => (Thickness)GetValue(LevelPaddingProperty);
            private set { SetValue(LevelPaddingProperty, value); }
        }

        public Thickness ValuePadding
        {
            get => (Thickness)GetValue(ValuePaddingProperty);
            private set { SetValue(ValuePaddingProperty, value); }
        }

        /// <summary>
        /// Gets the parent <see cref="ObjectTreeView" />.
        /// </summary>
        internal ObjectTreeView ParentObjectTreeView
        {
            get => (ObjectTreeView)ItemsControl.ItemsControlFromItemContainer(this);
        }
        #endregion

        #region Methods
        internal void SetProperty(MemberNode property)
        {
            HasItems = property.HasItems;
            Level = property.Level;
            IsExpanded = property.IsExpanded;
        }

        /// <summary>
        /// Handles changes in <see cref="Level" /> or <see cref="ObjectTreeView.Indentation" /> (in the parent control).
        /// </summary>
        internal void OnLevelOrIndentationChanged()
        {
            var level = Level - 1;
            var indentation = ParentObjectTreeView.Indentation;
            
            LevelPadding = new Thickness(level * indentation, 0, 0, 0);
            ValuePadding = new Thickness(-((Level * indentation) + 4), 0, 0, 0);
        }

        /// <summary>
        /// Handles changes in the <see cref="IsExpanded" /> property.
        /// </summary>
        private void OnIsExpandedChanged()
        {
            if (IsExpanded)
                ParentObjectTreeView.Expand(Content);
            else
                ParentObjectTreeView.Collapse(Content);
        }

        private void OnClickRef()
        {
            ParentObjectTreeView.SelectRefrence(Content);
        }
        #endregion
    }
}
