// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

#if !WINDOWS_UWP
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
#else
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#endif

namespace Microsoft.Toolkit.Uwp.UI.Controls.DataGridInternals
{
    internal static class Extensions
    {
        private static Dictionary<DependencyObject, Dictionary<DependencyProperty, bool>> _suspendedHandlers = new Dictionary<DependencyObject, Dictionary<DependencyProperty, bool>>();

        public static bool IsHandlerSuspended(this DependencyObject dependencyObject, DependencyProperty dependencyProperty)
        {
            return _suspendedHandlers.ContainsKey(dependencyObject) ? _suspendedHandlers[dependencyObject].ContainsKey(dependencyProperty) : false;
        }

        /// <summary>
        /// Walks the visual tree to determine if a particular child is contained within a parent DependencyObject.
        /// </summary>
        /// <param name="element">Parent DependencyObject</param>
        /// <param name="child">Child DependencyObject</param>
        /// <returns>True if the parent element contains the child</returns>
        internal static bool ContainsChild(this DependencyObject element, DependencyObject child)
        {
            if (element != null)
            {
                while (child != null)
                {
                    if (child == element)
                    {
                        return true;
                    }

                    // Walk up the visual tree.  If we hit the root, try using the framework element's
                    // parent.  We do this because Popups behave differently with respect to the visual tree,
                    // and it could have a parent even if the VisualTreeHelper doesn't find it.
                    DependencyObject parent = VisualTreeHelper.GetParent(child);
                    if (parent == null)
                    {
                        FrameworkElement childElement = child as FrameworkElement;
                        if (childElement != null)
                        {
                            parent = childElement.Parent;
                        }
                    }

                    child = parent;
                }
            }

            return false;
        }

        /// <summary>
        /// Walks the visual tree to determine if the currently focused element is contained within
        /// a parent DependencyObject. The FocusManager's GetFocusedElement method is used to determine
        /// the currently focused element, which is updated synchronously.
        /// </summary>
        /// <param name="element">Parent DependencyObject</param>
        /// <returns>True if the currently focused element is within the visual tree of the parent</returns>
        internal static bool ContainsFocusedElement(this DependencyObject element)
        {
#if WINDOWS_UWP
            return (element == null) ? false : element.ContainsChild(FocusManager.GetFocusedElement() as DependencyObject);
#else
            return (element == null) ? false : element.ContainsChild(FocusManager.GetFocusedElement(null /*TODO - correct parameter?*/) as DependencyObject);
#endif
        }

        /// <summary>
        /// Checks a MemberInfo object (e.g. a Type or PropertyInfo) for the ReadOnly attribute
        /// and returns the value of IsReadOnly if it exists.
        /// </summary>
        /// <param name="memberInfo">MemberInfo to check</param>
        /// <returns>true if MemberInfo is read-only, false otherwise</returns>
        internal static bool GetIsReadOnly(this MemberInfo memberInfo)
        {
#if !WINDOWS_UWP
            if (memberInfo != null)
            {
                // Check if ReadOnlyAttribute is defined on the member
                object[] attributes = memberInfo.GetCustomAttributes(typeof(ReadOnlyAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    ReadOnlyAttribute readOnlyAttribute = attributes[0] as ReadOnlyAttribute;
                    Debug.Assert(readOnlyAttribute != null, "Expected non-null readOnlyAttribute.");
                    return readOnlyAttribute.IsReadOnly;
                }
            }
#endif
            return false;
        }

        internal static Type GetItemType(this IEnumerable list)
        {
            Type listType = list.GetType();
            Type itemType = null;
            bool isICustomTypeProvider = false;

            // if it's a generic enumerable, we get the generic type

            // Unfortunately, if data source is fed from a bare IEnumerable, TypeHelper will report an element type of object,
            // which is not particularly interesting.  We deal with it further on.
            if (listType.IsEnumerableType())
            {
                itemType = listType.GetEnumerableItemType();
                if (itemType != null)
                {
#if !WINDOWS_UWP
                    isICustomTypeProvider = typeof(ICustomTypeProvider).IsAssignableFrom(itemType);
#endif
                }
            }

            // Bare IEnumerables mean that result type will be object.  In that case, we try to get something more interesting.
            // Or, if the itemType implements ICustomTypeProvider, we try to retrieve the custom type from one of the object
            // instances.
            if (itemType == null || itemType == typeof(object) || isICustomTypeProvider)
            {
                // We haven't located a type yet. Does the list have anything in it?
                IEnumerator en = list.GetEnumerator();
                if (en.MoveNext() && en.Current != null)
                {
                    Type firstItemType = en.Current.GetCustomOrCLRType();
                    if (firstItemType != typeof(object))
                    {
                        return firstItemType;
                    }
                }

                /* TODO - use this code instead?
                itemType = list
                    .Cast<object>() // cast to convert IEnumerable to IEnumerable<object>
                    .Select(x => x.GetType()) // get the type
                    .FirstOrDefault(); // get only the first thing to come out of the sequence, or null if empty

                // TODO if result != null, then we want to verify that each item in the sequence is castable to that type, or find a better type
                // though it would be better if the user just used a strongly-typed data source to begin with
                */
            }

            // We couldn't get the CustomType because there were no items.  Fail here so we try again
            // once items are added to the DataGrid
            if (isICustomTypeProvider)
            {
                return null;
            }

            // if we're null at this point, give up
            return itemType;
        }

        public static void SetStyleWithType(this FrameworkElement element, Style style)
        {
            if (element.Style != style && (style == null || style.TargetType != null))
            {
                element.Style = style;
            }
        }

        public static void SetValueNoCallback(this DependencyObject obj, DependencyProperty property, object value)
        {
            obj.SuspendHandler(property, true);
            try
            {
                obj.SetValue(property, value);
            }
            finally
            {
                obj.SuspendHandler(property, false);
            }
        }

        internal static Point Translate(this UIElement fromElement, UIElement toElement, Point fromPoint)
        {
            if (fromElement == toElement)
            {
                return fromPoint;
            }
            else
            {
#if WINDOWS_UWP
                return fromElement.TransformToVisual(toElement).TransformPoint(fromPoint);
#else
                return fromElement.TransformToVisual(toElement).Transform(fromPoint);
#endif
            }
        }

        internal static bool Within(this Point referencePoint, UIElement referenceElement, FrameworkElement targetElement, bool ignoreVertical)
        {
            Point position = referenceElement.Translate(targetElement, referencePoint);

            return position.X > 0 && position.X < targetElement.ActualWidth &&
                   (ignoreVertical || (position.Y > 0 && position.Y < targetElement.ActualHeight));
        }

        // If the DataGrid goes into a background tab, the elements need to be remeasured
        // or they will report 0 height.
        internal static UIElement EnsureMeasured(this UIElement element)
        {
            if (element.DesiredSize.Height == 0)
            {
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return element;
        }

        private static void SuspendHandler(this DependencyObject obj, DependencyProperty dependencyProperty, bool suspend)
        {
            if (_suspendedHandlers.ContainsKey(obj))
            {
                Dictionary<DependencyProperty, bool> suspensions = _suspendedHandlers[obj];

                if (suspend)
                {
                    Debug.Assert(!suspensions.ContainsKey(dependencyProperty), "Expected no key for dependencyProperty.");
                    suspensions[dependencyProperty] = true; // true = dummy value
                }
                else
                {
                    Debug.Assert(suspensions.ContainsKey(dependencyProperty), "Expected existing key for dependencyProperty.");
                    suspensions.Remove(dependencyProperty);
                    if (suspensions.Count == 0)
                    {
                        _suspendedHandlers.Remove(obj);
                    }
                }
            }
            else
            {
                Debug.Assert(suspend, "Expected suspend==true.");
                _suspendedHandlers[obj] = new Dictionary<DependencyProperty, bool>();
                _suspendedHandlers[obj][dependencyProperty] = true;
            }
        }
    }
}
