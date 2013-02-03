using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// Represents one ternary condition.
    /// </summary>
    public class ComparisonCondition : FrameworkElement
    {
        public static readonly DependencyProperty LeftOperandProperty = DependencyProperty.Register("LeftOperand", typeof(object), typeof(ComparisonCondition), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the left operand.
        /// </summary>
        public object LeftOperand
        {
            get
            {
                return base.GetValue(LeftOperandProperty);
            }
            set
            {
                base.SetValue(LeftOperandProperty, value);
            }
        }

        public static readonly DependencyProperty OperatorProperty = DependencyProperty.Register("Operator", typeof(ComparisonConditionType), typeof(ComparisonCondition), new PropertyMetadata(ComparisonConditionType.Equal));

        /// <summary>
        /// Gets or sets the comparison operator.
        /// </summary>
        public ComparisonConditionType Operator
        {
            get
            {
                return (ComparisonConditionType)base.GetValue(OperatorProperty);
            }
            set
            {
                base.SetValue(OperatorProperty, value);
            }
        }

        public static readonly DependencyProperty RightOperandProperty = DependencyProperty.Register("RightOperand", typeof(object), typeof(ComparisonCondition), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the right operand.
        /// </summary>
        public object RightOperand
        {
            get
            {
                return base.GetValue(RightOperandProperty);
            }
            set
            {
                base.SetValue(RightOperandProperty, value);
            }
        }

        ///// <summary>
        ///// Ensure that any binding on DP operands are up-to-date.
        ///// </summary>
        //private void EnsureBindingUpToDate()
        //{
        //    DataBindingHelper.EnsureBindingUpToDate(this, LeftOperandProperty);
        //    DataBindingHelper.EnsureBindingUpToDate(this, OperatorProperty);
        //    DataBindingHelper.EnsureBindingUpToDate(this, RightOperandProperty);
        //}

        /// <summary>
        /// Method that evaluates the condition. Note that this method can throw ArgumentException if the operator is
        /// incompatible with the type. For instance, operators LessThan, LessThanOrEqual, GreaterThan, and GreaterThanOrEqual
        /// require both operators to implement IComparable. 
        /// </summary>
        /// <returns>Returns true if the condition has been met; otherwise, returns false.</returns>
        public bool Evaluate()
        {
            //this.EnsureBindingUpToDate();
            return ComparisonLogic.EvaluateImpl(this.LeftOperand, this.Operator, this.RightOperand);
        }
    }
}
