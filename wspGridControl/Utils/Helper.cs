using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Data;
using wspGridControl.Primitives;
using System.Reflection;

namespace wspGridControl
{
    internal static class Helper
    {
        #region UITree Methods
        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, 
        /// a null parent is being returned.</returns>
        public static T FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && frameworkElement.Name == childName)
                    {
                        // if the child's name is of the request name
                        foundChild = (T)child;
                        break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        public static IEnumerable<T> FindLogicalChildren<T>(DependencyObject element) where T : DependencyObject
        {
            if (element != null)
            {
                foreach (object rawChild in LogicalTreeHelper.GetChildren(element))
                {
                    if (rawChild is DependencyObject)
                    {
                        DependencyObject child = (DependencyObject)rawChild;
                        if (child is T)
                        {
                            yield return (T)child;
                        }

                        foreach (T childOfChild in FindLogicalChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject element) where T : DependencyObject
        {
            if (element == null) yield return (T)Enumerable.Empty<T>();

            int childrenCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject ithChild = VisualTreeHelper.GetChild(element, i);
                if (ithChild == null) continue;
                if (ithChild is T t) yield return t;
                foreach (T childOfChild in FindVisualChildren<T>(ithChild)) yield return childOfChild;
            }
        }

        /// <summary>
        ///     Walks up the templated parent tree looking for a parent type.
        /// </summary>
        public static T FindParent<T>(FrameworkElement element) where T : FrameworkElement
        {
            FrameworkElement parent = element.TemplatedParent as FrameworkElement;

            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = parent.TemplatedParent as FrameworkElement;
            }

            return null;
        }

        public static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }

            return null;
        }

        /// <summary>
        ///     Helper method which determines if any of the elements of
        ///     the tree is focusable and has tab stop
        /// </summary>
        public static bool TreeHasFocusAndTabStop(DependencyObject element)
        {
            if (element == null)
            {
                return false;
            }

            UIElement uielement = element as UIElement;
            if (uielement != null)
            {
                if (uielement.Focusable && KeyboardNavigation.GetIsTabStop(uielement))
                {
                    return true;
                }
            }
            else
            {
                ContentElement contentElement = element as ContentElement;
                if (contentElement != null && contentElement.Focusable && KeyboardNavigation.GetIsTabStop(contentElement))
                {
                    return true;
                }
            }

            int childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(element, i) as DependencyObject;
                if (TreeHasFocusAndTabStop(child))
                {
                    return true;
                }
            }

            return false;
        }

        // a version of IsAncestorOf that stops looking when it finds an ancestor
        // of the given type
        public static bool IsAncestorOf(DependencyObject ancestor, DependencyObject descendant, Type stopType)
        {
            if (ancestor == null)
            {
                throw new ArgumentNullException("ancestor");
            }
            if (descendant == null)
            {
                throw new ArgumentNullException("descendant");
            }

            EnsureVisual(ancestor);
            EnsureVisual(descendant);

            // Walk up the parent chain of the descendant until we run out of parents,
            // or we find the ancestor, or we reach a node of the given type.
            DependencyObject current = descendant;

            while ((current != null) && (current != ancestor) && !stopType.IsInstanceOfType(current))
            {
                Visual visual;
                Visual3D visual3D;

                if ((visual = current as Visual) != null)
                {
                    current = VisualTreeHelper.GetParent(visual);
                }
                else if ((visual3D = current as Visual3D) != null)
                {
                    current = VisualTreeHelper.GetParent(visual3D);
                }
                else
                {
                    current = null;
                }
            }

            return current == ancestor;
        }

        private static void EnsureVisual(DependencyObject element)
        {
            if (element == null)
            {
                return;
            }

            //
            if (!(element is Visual || element is Visual3D))
            {
                throw new ArgumentException();
            }

            element.VerifyAccess();
        }

        internal static void UpdateTarget(FrameworkElement element)
        {
            BindingGroup bindingGroup = element.BindingGroup;
            GridCell cell = (element != null) ? element.Parent as GridCell : null;

            if (bindingGroup != null && cell != null)
            {
                Collection<BindingExpressionBase> expressions = bindingGroup.BindingExpressions;
                BindingExpressionBase[] bindingExpressionsCopy = new BindingExpressionBase[expressions.Count];
                expressions.CopyTo(bindingExpressionsCopy, 0);

                for (int i = 0; i < bindingExpressionsCopy.Length; i++)
                {
                    BindingExpressionBase beb = bindingExpressionsCopy[i];
                    DependencyObject targetElement = beb.Target;
                    if (targetElement != null && IsAncestorOf(cell, targetElement, typeof(GridCell)))
                    {
                        beb.UpdateTarget();
                    }
                }
            }
        }
        #endregion

        #region Bounds Methods
        public static Rect GetBounds(FrameworkElement element)
        {
            var offset = VisualTreeHelper.GetOffset(element);
            return new Rect(offset.X, offset.Y, element.ActualWidth, element.ActualHeight);
        }

        public static Rect BoundsRelativeTo(FrameworkElement parent, FrameworkElement child)
        {
            if (child == null) return Rect.Empty;
            GeneralTransform gt = child.TransformToAncestor(parent);
            return gt.TransformBounds(new Rect(0, 0, child.ActualWidth, child.ActualHeight));
        }
        #endregion

        #region DependencyProperty Methods
        /// <summary>
        ///     Tracks which properties are currently being transfered.  This information is needed when GetPropertyTransferEnabledMapForObject
        ///     is called inside of Coercion.
        /// </summary>
        private static ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, bool>> _propertyTransferEnabledMap = new ConditionalWeakTable<DependencyObject, Dictionary<DependencyProperty, bool>>();

        public static bool IsDefaultValue(DependencyObject d, DependencyProperty dp)
        {
            return DependencyPropertyHelper.GetValueSource(d, dp).BaseValueSource == BaseValueSource.Default;
        }

        private static Dictionary<DependencyProperty, bool> GetPropertyTransferEnabledMapForObject(DependencyObject d)
        {
            Dictionary<DependencyProperty, bool> propertyTransferEnabledForObject;

            if (!_propertyTransferEnabledMap.TryGetValue(d, out propertyTransferEnabledForObject))
            {
                propertyTransferEnabledForObject = new Dictionary<DependencyProperty, bool>();
                _propertyTransferEnabledMap.Add(d, propertyTransferEnabledForObject);
            }

            return propertyTransferEnabledForObject;
        }

        internal static bool IsPropertyTransferEnabled(DependencyObject d, DependencyProperty p)
        {
            Dictionary<DependencyProperty, bool> propertyTransferEnabledForObject;

            if (_propertyTransferEnabledMap.TryGetValue(d, out propertyTransferEnabledForObject))
            {
                bool isPropertyTransferEnabled;
                if (propertyTransferEnabledForObject.TryGetValue(p, out isPropertyTransferEnabled))
                {
                    return isPropertyTransferEnabled;
                }
            }

            return false;
        }

        /// <summary>
        ///     Causes the given DependencyProperty to be coerced in transfer mode.
        /// </summary>
        /// <remarks>
        ///     This should be called from within the target object's NotifyPropertyChanged.  It MUST be called in
        ///     response to a change in the target property.
        /// </remarks>
        /// <param name="d">The DependencyObject which contains the property that needs to be transfered.</param>
        /// <param name="p">The DependencyProperty that is the target of the property transfer.</param>
        public static void TransferProperty(DependencyObject d, DependencyProperty p)
        {
            var transferEnabledMap = GetPropertyTransferEnabledMapForObject(d);
            transferEnabledMap[p] = true;
            d.CoerceValue(p);
            transferEnabledMap[p] = false;
        }

        public static object GetCoercedTransferPropertyValue(DependencyObject baseObject, object baseValue, DependencyProperty baseProperty, DependencyObject parentObject, DependencyProperty parentProperty)
        {
            return GetCoercedTransferPropertyValue(baseObject, baseValue, baseProperty, parentObject, parentProperty, null, null);
        }

        /// <summary>
        ///     Computes the value of a given property based on the DataList property transfer rules.
        /// </summary>
        /// <remarks>
        ///     This is intended to be called from within the coercion of the baseProperty.
        /// </remarks>
        /// <param name="baseObject">The target object which recieves the transferred property</param>
        /// <param name="baseValue">The baseValue that was passed into the coercion delegate</param>
        /// <param name="baseProperty">The property that is being coerced</param>
        /// <param name="parentObject">The object that contains the parentProperty</param>
        /// <param name="parentProperty">A property who's value should be transfered (via coercion) to the baseObject if it has a higher precedence.</param>
        /// <param name="grandParentObject">Same as parentObject but evaluated at a lower presedece for a given BaseValueSource</param>
        /// <param name="grandParentProperty">Same as parentProperty but evaluated at a lower presedece for a given BaseValueSource</param>
        /// <returns></returns>
        public static object GetCoercedTransferPropertyValue(DependencyObject baseObject, object baseValue, DependencyProperty baseProperty,
            DependencyObject parentObject, DependencyProperty parentProperty, DependencyObject grandParentObject, DependencyProperty grandParentProperty)
        {
            // Transfer Property Coercion rules:
            //
            // Determine if this is a 'Transfer Property Coercion'.  If so:
            //   We can safely get the BaseValueSource because the property change originated from another
            //   property, and thus this BaseValueSource wont be stale.
            //   Pick a value to use based on who has the greatest BaseValueSource
            // If not a 'Transfer Property Coercion', simply return baseValue.  This will cause a property change if the value changes, which
            // will trigger a 'Transfer Property Coercion', and we will no longer have a stale BaseValueSource
            var coercedValue = baseValue;

            if (IsPropertyTransferEnabled(baseObject, baseProperty))
            {
                var propertySource = DependencyPropertyHelper.GetValueSource(baseObject, baseProperty);
                var maxBaseValueSource = propertySource.BaseValueSource;

                if (parentObject != null)
                {
                    var parentPropertySource = DependencyPropertyHelper.GetValueSource(parentObject, parentProperty);

                    if (parentPropertySource.BaseValueSource > maxBaseValueSource)
                    {
                        coercedValue = parentObject.GetValue(parentProperty);
                        maxBaseValueSource = parentPropertySource.BaseValueSource;
                    }
                }

                if (grandParentObject != null)
                {
                    var grandParentPropertySource = DependencyPropertyHelper.GetValueSource(grandParentObject, grandParentProperty);

                    if (grandParentPropertySource.BaseValueSource > maxBaseValueSource)
                    {
                        coercedValue = grandParentObject.GetValue(grandParentProperty);
                        maxBaseValueSource = grandParentPropertySource.BaseValueSource;
                    }
                }
            }

            return coercedValue;
        }

        public static void SyncProperty(DependencyObject source, DependencyProperty sourceProperty, DependencyObject dest, DependencyProperty destProperty)
        {
            if (IsDefaultValue(source, sourceProperty))
            {
                dest.ClearValue(destProperty);
            }
            else
            {
                dest.SetValue(destProperty, source.GetValue(sourceProperty));
            }
        }
        #endregion

        #region Binding Methods
        internal static bool IsOneWay(BindingBase bindingBase)
        {
            if (bindingBase == null)
            {
                return false;
            }

            // If it is a standard Binding, then check if it's Mode is OneWay
            Binding binding = bindingBase as Binding;
            if (binding != null)
            {
                return binding.Mode == BindingMode.OneWay;
            }

            // A multi-binding can be OneWay as well
            MultiBinding multiBinding = bindingBase as MultiBinding;
            if (multiBinding != null)
            {
                return multiBinding.Mode == BindingMode.OneWay;
            }

            // A priority binding is a list of bindings, if any are OneWay, we'll call it OneWay
            PriorityBinding priBinding = bindingBase as PriorityBinding;
            if (priBinding != null)
            {
                Collection<BindingBase> subBindings = priBinding.Bindings;
                int count = subBindings.Count;
                for (int i = 0; i < count; i++)
                {
                    if (IsOneWay(subBindings[i]))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static string GetPlainText(FrameworkElement element)
        {
            if (element == null) return null;

            Type type = element.GetType();
            MethodInfo methodInfo = type.GetMethod("GetPlainText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (methodInfo != null)
            {
                try
                {
                    object value = methodInfo.Invoke(element, null);
                    if (value != null)
                        return value.ToString();
                }
                catch(Exception)
                { }
            }

            PropertyInfo propertyInfo = type.GetProperty("Text", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (propertyInfo != null && propertyInfo.CanRead)
            {   
                try
                {
                    object textValue = propertyInfo.GetValue(element);
                    if (textValue != null)
                        return textValue.ToString();
                }
                catch (Exception)
                { }
            }

            return null;
        }
        #endregion

        #region Reflection Methods
        /// <summary>
        /// Get the type to use for reflection:  the custom type, if any, otherwise just the type.
        /// </summary>
        internal static Type GetReflectionType(object item)
        {
            if (item == null)
                return null;

            ICustomTypeProvider ictp = item as ICustomTypeProvider;
            if (ictp == null)
                return item.GetType();
            else
                return ictp.GetCustomType();
        }
        #endregion
    }
}
