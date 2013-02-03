using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// Represents a trigger that performs actions when the bound data have changed.
    /// </summary>
    public class PropertyChangedTrigger : TriggerBase<FrameworkElement>
    {
        public static readonly DependencyProperty BindingProperty = DependencyProperty.Register("Binding", typeof(object), typeof(PropertyChangedTrigger), new PropertyMetadata(null, new PropertyChangedCallback(PropertyChangedTrigger.OnBindingChanged)));

        /// <summary>
        /// A binding object that the trigger will listen to, and that causes the trigger to fire when it changes.
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

        /// <summary>
        /// Called when the binding property has changed.
        /// </summary>
        /// <param name="args"><see cref="System.Windows.DependencyPropertyChangedEventArgs"/> argument.</param>
        protected virtual void EvaluateBindingChange(object args)
        {
            base.InvokeActions(args);
        }

        ///// <summary>
        ///// Called after the trigger is attached to an AssociatedObject.
        ///// </summary>
        //protected override void OnAttached()
        //{
        //    base.OnAttached();
        //    base.PreviewInvoke += new EventHandler<PreviewInvokeEventArgs>(this.OnPreviewInvoke);
        //}

        private static void OnBindingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PropertyChangedTrigger)sender).EvaluateBindingChange(args);
        }

        ///// <summary>
        ///// Called when the trigger is being detached from its AssociatedObject, but before it has actually occurred.
        ///// </summary>
        //protected override void OnDetaching()
        //{
        //    base.PreviewInvoke -= new EventHandler<PreviewInvokeEventArgs>(this.OnPreviewInvoke);
        //    this.OnDetaching();
        //}

        //private void OnPreviewInvoke(object sender, PreviewInvokeEventArgs e)
        //{
        //    DataBindingHelper.EnsureDataBindingOnActionsUpToDate(this);
        //}
    }
}