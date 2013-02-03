using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// Represents a trigger that performs actions when the bound data meets a specified condition.
    /// </summary>
    public class DataTrigger : PropertyChangedTrigger
    {
        public static readonly DependencyProperty ComparisonProperty = DependencyProperty.Register("Comparison", typeof(ComparisonConditionType), typeof(DataTrigger), new PropertyMetadata(default(ComparisonCondition), new PropertyChangedCallback(DataTrigger.OnComparisonChanged)));

        /// <summary>
        /// Gets or sets the type of comparison to be performed between the specified values. This is a dependency property.
        /// </summary>
        public ComparisonConditionType Comparison
        {
            get
            {
                return (ComparisonConditionType)base.GetValue(ComparisonProperty);
            }
            set
            {
                base.SetValue(ComparisonProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DataTrigger), new PropertyMetadata(null, new PropertyChangedCallback(DataTrigger.OnValueChanged)));

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

        private bool Compare()
        {
            return ((base.AssociatedObject != null) && ComparisonLogic.EvaluateImpl(base.Binding, this.Comparison, this.Value));
        }

        /// <summary>
        /// Called when the binding property has changed. 
        /// </summary>
        /// <param name="args"><see cref="System.Windows.DependencyPropertyChangedEventArgs"/> argument.</param>
        protected override void EvaluateBindingChange(object args)
        {
            if (this.Compare())
            {
                base.InvokeActions(args);
            }
        }

        private static void OnComparisonChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ((DataTrigger)sender).EvaluateBindingChange(args);
        }

        private static void OnValueChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            ((DataTrigger)sender).EvaluateBindingChange(args);
        }
    }
}