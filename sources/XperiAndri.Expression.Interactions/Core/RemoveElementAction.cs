using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// An action that will remove the targeted element from the tree when invoked.
    /// </summary>
    /// <remarks>
    /// This action may fail. The action understands how to remove elements from common parents but not from custom collections or direct manipulation
    /// of the visual tree.
    /// </remarks>
    public class RemoveElementAction : TargetedTriggerAction<FrameworkElement>
    {
        protected override void Invoke(object parameter)
        {
            if ((base.AssociatedObject != null) && (base.Target != null))
            {
                DependencyObject parent = base.Target.Parent;
                Panel panel = parent as Panel;
                if (panel != null)
                {
                    panel.Children.Remove(base.Target);
                }
                else
                {
                    ContentControl control = parent as ContentControl;
                    if (control != null)
                    {
                        if (control.Content == base.Target)
                        {
                            control.Content = null;
                        }
                    }
                    else
                    {
                        ItemsControl control2 = parent as ItemsControl;
                        if (control2 != null)
                        {
                            control2.Items.Remove(base.Target);
                        }
                        else if (parent != null)
                        {
                            throw new InvalidOperationException(ExceptionStringTableHelper.UnsupportedRemoveTargetExceptionMessage);
                        }
                    }
                }
            }
        }
    }
}