using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Microsoft.Expression.Interactivity.Core
{
    //[DefaultTrigger(typeof(UIElement), typeof(EventTrigger), "MouseLeftButtonDown"), DefaultTrigger(typeof(ButtonBase), typeof(EventTrigger), "Click")]
    /// <summary>
    /// Makes the attached element switch to a different XAML page when clicked on.
    /// </summary>
    public class NavigateToPageAction : TargetedTriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty TargetPageProperty = DependencyProperty.Register("TargetPage", typeof(Type), typeof(NavigateToPageAction), new PropertyMetadata(default(Type)));

        /// <summary>
        /// Gets or sets the XAML file to navigate to. This is a dependency property.
        /// </summary>
        public Type TargetPage
        {
            get
            {
                return (Type)base.GetValue(TargetPageProperty);
            }
            set
            {
                base.SetValue(TargetPageProperty, value);
            }
        }

        private bool IsTargetObjectSet
        {
            get
            {
                return (base.ReadLocalValue(TargetedTriggerAction.TargetObjectProperty) != DependencyProperty.UnsetValue);
            }
        }

        /// <summary>
        /// Invokes the navigation action.
        /// </summary>
        /// <param name="parameter">The parameter to the action. If the Action does not require a parameter, the parameter may be set to a null reference.</param>
        protected override void Invoke(object parameter)
        {
            (Window.Current.Content as INavigate).Navigate(this.TargetPage);
        }

        protected override void OnTargetChanged(FrameworkElement oldTarget, FrameworkElement newTarget)
        {
            base.OnTargetChanged(oldTarget, newTarget);
            FrameworkElement resolvedFrame = base.Target as Frame;
            if (string.IsNullOrEmpty(base.TargetName) && !this.IsTargetObjectSet)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Target {0} is not a Frame.", new object[] { resolvedFrame.Name }));
            }
        }
    }
}
