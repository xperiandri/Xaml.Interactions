using XperiAndri.Expression.Interactivity.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace XperiAndri.Expression.Interactivity
{
    /// <summary>
    /// This class provides various platform agnostic standard operations for working with VisualStateManager.
    /// </summary>
    public static class VisualStateUtilities
    {
        internal static FrameworkElement FindNearestStatefulControl(FrameworkElement contextElement)
        {
            FrameworkElement resolvedControl = null;
            TryFindNearestStatefulControl(contextElement, out resolvedControl);
            return resolvedControl;
        }

        private static FrameworkElement FindTemplatedParent(FrameworkElement parent)
        {
            return (VisualTreeHelper.GetParent(parent) as FrameworkElement);
        }

        /// <summary>
        /// Gets the value of the VisualStateManager.VisualStateGroups attached property.
        /// </summary>
        /// <param name="targetObject">The element from which to get the VisualStateManager.VisualStateGroups.</param>
        /// <returns></returns>
        public static IList<VisualStateGroup> GetVisualStateGroups(FrameworkElement targetObject)
        {
            IList<VisualStateGroup> visualStateGroups = new List<VisualStateGroup>();
            if (targetObject != null)
            {
                visualStateGroups = VisualStateManager.GetVisualStateGroups(targetObject);
                if ((visualStateGroups.Count == 0) && (VisualTreeHelper.GetChildrenCount(targetObject) > 0))
                {
                    FrameworkElement child = VisualTreeHelper.GetChild(targetObject, 0) as FrameworkElement;
                    visualStateGroups = VisualStateManager.GetVisualStateGroups(child);
                }
            }
            return visualStateGroups;
        }

        /// <summary>
        /// Transitions the control between two states.
        /// </summary>
        /// <param name="element">The element to transition between states.</param>
        /// <param name="stateName">The state to transition to.</param>
        /// <param name="useTransitions">True to use a System.Windows.VisualTransition to transition between states; otherwise, false.</param>
        /// <returns>True if the control successfully transitioned to the new state; otherwise, false.</returns>
        /// <exception cref="T:System.ArgumentNullException">Control is null.</exception>
        /// <exception cref="T:System.ArgumentNullException">StateName is null.</exception>
        public static bool GoToState(FrameworkElement element, string stateName, bool useTransitions)
        {
            if (string.IsNullOrEmpty(stateName))
            {
                return false;
            }
            Control control = element as Control;
            if (control != null)
            {
                control.ApplyTemplate();
                return VisualStateManager.GoToState(control, stateName, useTransitions);
            }
            return ExtendedVisualStateManager.GoToElementState(element, stateName, useTransitions);
        }

        private static bool HasVisualStateGroupsDefined(FrameworkElement frameworkElement)
        {
            return ((frameworkElement != null) && (VisualStateManager.GetVisualStateGroups(frameworkElement).Count != 0));
        }

        private static bool ShouldContinueTreeWalk(FrameworkElement element)
        {
            if (element == null)
            {
                return false;
            }
            if (element is UserControl)
            {
                return false;
            }
            if (element.Parent == null)
            {
                FrameworkElement element2 = FindTemplatedParent(element);
                if ((element2 == null) || (!(element2 is Control) && !(element2 is ContentPresenter)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Find the nearest parent which contains visual states.
        /// </summary>
        /// <param name="contextElement">The element from which to find the nearest stateful control.</param>
        /// <param name="resolvedControl">The nearest stateful control if True; else null.</param>
        /// <returns>True if a parent contains visual states; else False.</returns>
        public static bool TryFindNearestStatefulControl(FrameworkElement contextElement, out FrameworkElement resolvedControl)
        {
            FrameworkElement frameworkElement = contextElement;
            if (frameworkElement == null)
            {
                resolvedControl = null;
                return false;
            }
            FrameworkElement parent = frameworkElement.Parent as FrameworkElement;
            bool flag = true;
            while (!HasVisualStateGroupsDefined(frameworkElement) && ShouldContinueTreeWalk(parent))
            {
                frameworkElement = parent;
                parent = parent.Parent as FrameworkElement;
            }
            if (HasVisualStateGroupsDefined(frameworkElement))
            {
                FrameworkElement element3 = VisualTreeHelper.GetParent(frameworkElement) as FrameworkElement;
                if ((element3 != null) && (element3 is Control))
                {
                    frameworkElement = element3;
                }
            }
            else
            {
                flag = false;
            }
            resolvedControl = frameworkElement;
            return flag;
        }
    }
}