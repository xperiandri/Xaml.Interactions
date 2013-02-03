using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// An action that will transition a FrameworkElement to a specified VisualState when invoked.
    /// </summary>
    /// <remarks>
    /// If the TargetName property is set, this action will attempt to change the state of the targeted element. If not, it walks
    /// the element tree in an attempt to locate an alternative target that defines states. ControlTemplate and UserControl are
    /// two common possibilities.
    /// </remarks>
    public class GoToStateAction : TargetedTriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty UseTransitionsProperty = DependencyProperty.Register("UseTransitions", typeof(bool), typeof(GoToStateAction), new PropertyMetadata(true));

        /// <summary>
        /// Determines whether or not to use a VisualTransition to transition between states.
        /// </summary>
        public bool UseTransitions
        {
            get
            {
                return (bool)base.GetValue(UseTransitionsProperty);
            }
            set
            {
                base.SetValue(UseTransitionsProperty, value);
            }
        }

        public static readonly DependencyProperty StateNameProperty = DependencyProperty.Register("StateName", typeof(string), typeof(GoToStateAction), new PropertyMetadata(string.Empty));

        /// <summary>
        /// The name of the VisualState.
        /// </summary>
        public string StateName
        {
            get
            {
                return (string)base.GetValue(StateNameProperty);
            }
            set
            {
                base.SetValue(StateNameProperty, value);
            }
        }

        private bool IsTargetObjectSet
        {
            get
            {
                return (base.ReadLocalValue(TargetedTriggerAction.TargetObjectProperty) != DependencyProperty.UnsetValue);
            }
        }

        private FrameworkElement StateTarget { get; set; }

        /// <summary>
        /// This method is called when some criteria is met and the action is invoked.
        /// </summary>
        /// <param name="parameter"></param>
        /// <exception cref="System.InvalidOperationException">Could not change the target to the specified StateName.</exception>
        protected override void Invoke(object parameter)
        {
            if (base.AssociatedObject != null)
            {
                this.InvokeImpl(this.StateTarget);
            }
        }

        internal void InvokeImpl(FrameworkElement stateTarget)
        {
            if (stateTarget != null)
            {
                VisualStateUtilities.GoToState(stateTarget, this.StateName, this.UseTransitions);
            }
        }

        /// <summary>
        /// <summary>
        /// Called when the target changes. If the TargetName property isn't set, this action has custom behavior.
        /// </summary>
        /// <param name="oldTarget"></param>
        /// <param name="newTarget"></param>
        /// <exception cref="T:System.InvalidOperationException">Could not locate an appropriate FrameworkElement with states.</exception>
        protected override void OnTargetChanged(FrameworkElement oldTarget, FrameworkElement newTarget)
        {
            base.OnTargetChanged(oldTarget, newTarget);
            FrameworkElement resolvedControl = null;
            if (string.IsNullOrEmpty(base.TargetName) && !this.IsTargetObjectSet)
            {
                if (!VisualStateUtilities.TryFindNearestStatefulControl(base.AssociatedObject as FrameworkElement, out resolvedControl) && (resolvedControl != null))
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.GoToStateActionTargetHasNoStateGroups, new object[] { resolvedControl.Name }));
                }
            }
            else
            {
                resolvedControl = base.Target;
            }
            this.StateTarget = resolvedControl;
        }
    }
}