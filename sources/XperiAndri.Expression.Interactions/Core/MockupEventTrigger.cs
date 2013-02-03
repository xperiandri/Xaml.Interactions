using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace XperiAndri.Expression.Interactivity.Core
{
    public class MockupEventTrigger : TriggerBase<UserControl>
    {
        // Fields
        private Delegate eventHandler;

        private EventInfo eventInfo;
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName", typeof(string), typeof(MockupEventTrigger), new PropertyMetadata(default(string), new PropertyChangedCallback(MockupEventTrigger.OnEventNameChangedCallback)));
        private FrameworkElement eventSource;
        public static readonly DependencyProperty SourceNameProperty = DependencyProperty.Register("SourceName", typeof(string), typeof(MockupEventTrigger), new PropertyMetadata(default(string), new PropertyChangedCallback(MockupEventTrigger.OnSourceNameChangedCallback)));

        // Methods
        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            base.AssociatedObject.Loaded -= this.AssociatedObject_Loaded;
            UpdateSource(this);
        }

        internal void AttachToSourceEvent(FrameworkElement eventSource, string eventName)
        {
            if (((this.eventSource != null) && (this.eventInfo != null)) && (this.eventHandler != null))
            {
                this.DetachFromSourceEvent();
            }
            EventInfo info = eventSource.GetType().GetRuntimeEvent(eventName);
            if ((info != null) && (info.EventHandlerType != null))
            {
                MethodInfo addMethod = info.AddMethod;
                if (addMethod != null)
                {
                    MethodInfo method = base.GetType().GetRuntimeMethods().First(m => m.Name == GetMethodName<object, EventArgs>((s, e) => this.OnEvent(s, e)) && !m.IsStatic && !m.IsPublic);
                    Delegate delegate2 = method.CreateDelegate(info.EventHandlerType, this);
                    addMethod.Invoke(eventSource, new object[] { delegate2 });
                    this.eventSource = eventSource;
                    this.eventInfo = info;
                    this.eventHandler = delegate2;
                }
            }
        }

        internal void DetachFromSourceEvent()
        {
            if (((this.eventSource != null) && (this.eventInfo != null)) && (this.eventHandler != null))
            {
                MethodInfo removeMethod = this.eventInfo.RemoveMethod;
                if (removeMethod != null)
                {
                    removeMethod.Invoke(this.eventSource, new object[] { this.eventHandler });
                }
            }
            this.eventSource = null;
            this.eventInfo = null;
            this.eventHandler = null;
        }

        private static string GetMethodName<TParam0, TParam1>(Expression<Action<TParam0, TParam1>> methodLambda)
        {
            MethodCallExpression body = (MethodCallExpression)methodLambda.Body;
            return body.Method.Name;
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            base.AssociatedObject.Loaded += new RoutedEventHandler(this.AssociatedObject_Loaded);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.DetachFromSourceEvent();
        }

        private void OnEvent(object sender, EventArgs e)
        {
            base.InvokeActions(e);
        }

        private static void OnEventNameChangedCallback(DependencyObject trigger, DependencyPropertyChangedEventArgs e)
        {
            UpdateSource((MockupEventTrigger)trigger);
        }

        private static void OnSourceNameChangedCallback(DependencyObject trigger, DependencyPropertyChangedEventArgs e)
        {
            UpdateSource((MockupEventTrigger)trigger);
        }

        private static void UpdateSource(MockupEventTrigger trigger)
        {
            UserControl associatedObject = trigger.AssociatedObject;
            string sourceName = trigger.SourceName;
            string eventName = trigger.EventName;
            if ((!string.IsNullOrEmpty(sourceName) && (associatedObject != null)) && (eventName != null))
            {
                FrameworkElement eventSource = associatedObject.FindName(sourceName) as FrameworkElement;
                if (eventSource != null)
                {
                    trigger.AttachToSourceEvent(eventSource, eventName);
                }
            }
        }

        // Properties
        public string EventName
        {
            get
            {
                return (string)base.GetValue(EventNameProperty);
            }
            set
            {
                base.SetValue(EventNameProperty, value);
            }
        }

        public string SourceName
        {
            get
            {
                return (string)base.GetValue(SourceNameProperty);
            }
            set
            {
                base.SetValue(SourceNameProperty, value);
            }
        }
    }
}