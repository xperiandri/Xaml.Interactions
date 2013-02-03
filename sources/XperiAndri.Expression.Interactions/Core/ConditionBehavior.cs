using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// A behavior that attaches to a trigger and controls the conditions
    /// to fire the actions.
    /// </summary>
    [ContentProperty(Name = "Condition")]
    public class ConditionBehavior : Behavior<Windows.UI.Interactivity.TriggerBase>
    {
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(ICondition), typeof(ConditionBehavior), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the IConditon object on behavior.
        /// </summary>
        /// <value>The name of the condition to change.</value>
        public ICondition Condition
        {
            get
            {
                return (ICondition)base.GetValue(ConditionProperty);
            }
            set
            {
                base.SetValue(ConditionProperty, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Core.ConditionBehavior"/> class.
        /// </summary>
        public ConditionBehavior()
        {
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            base.AssociatedObject.PreviewInvoke += this.OnPreviewInvoke;
        }

        protected override void OnDetaching()
        {
            base.AssociatedObject.PreviewInvoke -= this.OnPreviewInvoke;
            base.OnDetaching();
        }

        /// <summary>
        /// The event handler that is listening to the preview invoke event that is fired by
        /// the trigger. Setting PreviewInvokeEventArgs.Cancelling to True will
        /// cancel the invocation.
        /// </summary>
        /// <param name="sender">The trigger base object.</param>
        /// <param name="e">An object of type PreviewInvokeEventArgs where e.Cancelling can be set to True.</param>
        private void OnPreviewInvoke(object sender, PreviewInvokeEventArgs e)
        {
            if (this.Condition != null)
            {
                e.Cancelling = !this.Condition.Evaluate();
            }
        }
    }
}