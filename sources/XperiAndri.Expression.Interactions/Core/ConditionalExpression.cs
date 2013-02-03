using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// Represents a conditional expression that is set on a ConditionBehavior.Condition property. 
    /// Contains a list of conditions that gets evaluated in order to return true or false for ICondition.Evaluate().
    /// </summary>
    [ContentProperty(Name="Conditions")]
    public class ConditionalExpression : DependencyObject, ICondition
    {
        public static readonly DependencyProperty ConditionsProperty = DependencyProperty.Register("Conditions", typeof(ConditionCollection), typeof(ConditionalExpression), new PropertyMetadata(null));

        /// <summary>
        /// Return the Condition collections.
        /// </summary>
        public ConditionCollection Conditions
        {
            get
            {
                return (ConditionCollection)base.GetValue(ConditionsProperty);
            }
        }

        public static readonly DependencyProperty ForwardChainingProperty = DependencyProperty.Register("ForwardChaining", typeof(ForwardChaining), typeof(ConditionalExpression), new PropertyMetadata(ForwardChaining.And));

        /// <summary>
        /// Gets or sets forward chaining for the conditions.
        /// If forward chaining is set to ForwardChaining.And, all conditions must be met.
        /// If forward chaining is set to ForwardChaining.Or, only one condition must be met.
        /// </summary>
        public ForwardChaining ForwardChaining
        {
            get
            {
                return (ForwardChaining)base.GetValue(ForwardChainingProperty);
            }
            set
            {
                base.SetValue(ForwardChainingProperty, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Core.ConditionalExpression"/> class.
        /// </summary>
        public ConditionalExpression()
        {
            base.SetValue(ConditionsProperty, new ConditionCollection());
        }

        /// <summary>
        /// Goes through the Conditions collection and evalutes each condition based on 
        /// ForwardChaining property.
        /// </summary>
        /// <returns>Returns true if conditions are met; otherwise, returns false.</returns>
        public bool Evaluate()
        {
            bool flag = false;
            foreach (ComparisonCondition condition in this.Conditions)
            {
                flag = condition.Evaluate();
                if (!flag && (this.ForwardChaining == ForwardChaining.And))
                {
                    return flag;
                }
                if (flag && (this.ForwardChaining == ForwardChaining.Or))
                {
                    return flag;
                }
            }
            return flag;
        }
    }
}
