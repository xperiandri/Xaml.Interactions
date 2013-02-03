using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// Toggles between two states based on a conditional statement.
    /// </summary>
    public class DataStateBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty BindingProperty = DependencyProperty.Register("Binding", typeof(object), typeof(DataStateBehavior), new PropertyMetadata(null, new PropertyChangedCallback(DataStateBehavior.OnBindingChanged)));

        private static void OnBindingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((DataStateBehavior)obj).Evaluate();
        }

        /// <summary>
        /// Gets or sets the binding that produces the property value of the data object. This is a dependency property.
        /// </summary>
        public object Binding
        {
            get
            {
                return base.GetValue(BindingProperty);
            }
            set
            {
                base.SetValue(BindingProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DataStateBehavior), new PropertyMetadata(null, new PropertyChangedCallback(DataStateBehavior.OnValueChanged)));

        private static void OnValueChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ((DataStateBehavior)obj).Evaluate();
        }

        /// <summary>
        /// Gets or sets the value to be compared with the property value of the data object. This is a dependency property.
        /// </summary>
        public object Value
        {
            get
            {
                return base.GetValue(ValueProperty);
            }
            set
            {
                base.SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty TrueStateProperty = DependencyProperty.Register("TrueState", typeof(string), typeof(DataStateBehavior), new PropertyMetadata(default(string), new PropertyChangedCallback(DataStateBehavior.OnTrueStateChanged)));

        private static void OnTrueStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DataStateBehavior behavior = (DataStateBehavior)obj;
            behavior.ValidateStateName(behavior.TrueState);
            behavior.Evaluate();
        }

        /// <summary>
        /// Gets or sets the name of the visual state to transition to when the condition is met. This is a dependency property.
        /// </summary>
        public string TrueState
        {
            get
            {
                return (string)base.GetValue(TrueStateProperty);
            }
            set
            {
                base.SetValue(TrueStateProperty, value);
            }
        }

        public static readonly DependencyProperty FalseStateProperty = DependencyProperty.Register("FalseState", typeof(string), typeof(DataStateBehavior), new PropertyMetadata(default(string), new PropertyChangedCallback(DataStateBehavior.OnFalseStateChanged)));

        private static void OnFalseStateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            DataStateBehavior behavior = (DataStateBehavior)obj;
            behavior.ValidateStateName(behavior.FalseState);
            behavior.Evaluate();
        }

        /// <summary>
        /// Gets or sets the name of the visual state to transition to when the condition is not met. This is a dependency property.
        /// </summary>
        public string FalseState
        {
            get
            {
                return (string)base.GetValue(FalseStateProperty);
            }
            set
            {
                base.SetValue(FalseStateProperty, value);
            }
        }

        private FrameworkElement TargetObject
        {
            get
            {
                return VisualStateUtilities.FindNearestStatefulControl(base.AssociatedObject);
            }
        }

        private void Evaluate()
        {
            FrameworkElement targetObject = this.TargetObject;
            if (targetObject != null)
            {
                string stateName = null;

                if (ComparisonLogic.EvaluateImpl(this.Binding, ComparisonConditionType.Equal, this.Value))
                    stateName = this.TrueState;
                else
                    stateName = this.FalseState;

                VisualStateUtilities.GoToState(targetObject, stateName, true);
            }
        }

        ///// <summary>
        ///// A helper function to take the place of FrameworkElement.IsLoaded, as this property isn't available in WinRT.
        ///// </summary>
        ///// <param name="element">The element of interest.</param>
        ///// <returns>Returns true if the element has been loaded; otherwise, returns false.</returns>
        //internal static bool IsElementLoaded(FrameworkElement element)
        //{
        //    UIElement rootVisual = Window.Current.Content;
        //    DependencyObject parent = element.Parent;
        //    if (parent == null)
        //    {
        //        parent = VisualTreeHelper.GetParent(element);
        //    }
        //    return ((parent != null) || ((rootVisual != null) && (element == rootVisual)));
        //}

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            //this.ValidateStateNamesDeferred();
            // As OnAttached occurs after loading we can not worry about something is not ready
            this.ValidateStateNames();
        }

        private void ValidateStateName(string stateName)
        {
            if ((base.AssociatedObject != null) && !string.IsNullOrEmpty(stateName))
            {
                if (!VisualStateUtilities.GetVisualStateGroups(this.TargetObject)
                        .Any(visualStateGroup => visualStateGroup.States
                            .Any(visualState => visualState.Name == stateName)))
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.DataStateBehaviorStateNameNotFoundExceptionMessage, new object[] { stateName, (this.TargetObject != null) ? this.TargetObject.GetType().Name : "null" }));
                }
            }
        }

        private void ValidateStateNames()
        {
            this.ValidateStateName(this.TrueState);
            this.ValidateStateName(this.FalseState);
        }

        //private void ValidateStateNamesDeferred()
        //{
        //    RoutedEventHandler handler = null;
        //    FrameworkElement parent = base.AssociatedObject.Parent as FrameworkElement;
        //    if ((parent != null) && IsElementLoaded(parent))
        //    {
        //        this.ValidateStateNames();
        //    }
        //    else
        //    {
        //        if (handler == null)
        //        {
        //            handler = (o, e) => this.ValidateStateNames();
        //        }
        //        base.AssociatedObject.Loaded += handler;
        //    }
        //}
    }
}