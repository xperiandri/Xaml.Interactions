using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Microsoft.Expression.Interactivity.Input
{
    // Для определения клавиш использовать CoreWindow
    public class KeyTrigger : EventTriggerBase<UIElement>
    {
        public static readonly DependencyProperty FiredOnProperty = DependencyProperty.Register("FiredOn", typeof(KeyTriggerFiredOn), typeof(KeyTrigger), null);

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(VirtualKey), typeof(KeyTrigger), null);
        public static readonly DependencyProperty ModifiersProperty = DependencyProperty.Register("Modifiers", typeof(VirtualKeyModifiers), typeof(KeyTrigger), null);
        private UIElement targetElement;

        // Methods
        private static VirtualKeyModifiers GetActualModifiers(VirtualKey key, VirtualKeyModifiers modifiers)
        {
            if (key == VirtualKey.Control)
            {
                modifiers |= VirtualKeyModifiers.Control;
                return modifiers;
            }
            if (key == VirtualKey.Menu)
            {
                modifiers |= VirtualKeyModifiers.Menu;
                return modifiers;
            }
            if (key == VirtualKey.Shift)
            {
                modifiers |= VirtualKeyModifiers.Shift;
            }
            return modifiers;
        }

        protected override string GetEventName()
        {
            return "Loaded";
        }

        private static UIElement GetRoot(DependencyObject current)
        {
            UIElement element = null;
            while (current != null)
            {
                element = current as UIElement;
                current = VisualTreeHelper.GetParent(current);
            }
            return element;
        }

        protected override void OnDetaching()
        {
            if (this.targetElement != null)
            {
                if (this.FiredOn == KeyTriggerFiredOn.KeyDown)
                {
                    this.targetElement.KeyDown -= this.OnKeyPress;
                }
                else
                {
                    this.targetElement.KeyUp -= this.OnKeyPress;
                }
            }
            base.OnDetaching();
        }

        protected override void OnEvent(EventArgs eventArgs)
        {
            this.targetElement = Application.Current.RootVisual;
            if (this.FiredOn == KeyTriggerFiredOn.KeyDown)
            {
                this.targetElement.KeyDown += this.OnKeyPress;
            }
            else
            {
                this.targetElement.KeyUp += this.OnKeyPress;
            }
        }

        private void OnKeyPress(object sender, KeyRoutedEventArgs e)
        {
            if ((e.Key == this.Key) && (CoreWindow.GetForCurrentThread().GetKeyState() Keyboard.Modifiers == GetActualModifiers(e.Key, this.Modifiers)))
            {
                base.InvokeActions(e);
            }
        }

        // Properties
        public KeyTriggerFiredOn FiredOn
        {
            get
            {
                return (KeyTriggerFiredOn)base.GetValue(FiredOnProperty);
            }
            set
            {
                base.SetValue(FiredOnProperty, value);
            }
        }

        public VirtualKey Key
        {
            get
            {
                return (VirtualKey)base.GetValue(KeyProperty);
            }
            set
            {
                base.SetValue(KeyProperty, value);
            }
        }

        public VirtualKeyModifiers Modifiers
        {
            get
            {
                return (VirtualKeyModifiers)base.GetValue(ModifiersProperty);
            }
            set
            {
                base.SetValue(ModifiersProperty, value);
            }
        }
    }
}