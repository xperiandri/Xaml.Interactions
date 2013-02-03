using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// Calls a method on a specified object when invoked. 
    /// </summary>
    public class CallMethodAction : TriggerAction<FrameworkElement>
    {
        private List<MethodDescriptor> methodDescriptors = new List<MethodDescriptor>();

        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(CallMethodAction), new PropertyMetadata(null, new PropertyChangedCallback(CallMethodAction.OnMethodNameChanged)));

        /// <summary>
        /// The name of the method to invoke. This is a dependency property.
        /// </summary>
        public string MethodName
        {
            get
            {
                return (string)base.GetValue(MethodNameProperty);
            }
            set
            {
                base.SetValue(MethodNameProperty, value);
            }
        }

        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register("TargetObject", typeof(object), typeof(CallMethodAction), new PropertyMetadata(null, new PropertyChangedCallback(CallMethodAction.OnTargetObjectChanged)));

        /// <summary>
        /// The object that exposes the method of interest. This is a dependency property.
        /// </summary>
        public object TargetObject
        {
            get
            {
                return base.GetValue(TargetObjectProperty);
            }
            set
            {
                base.SetValue(TargetObjectProperty, value);
            }
        }

        private object Target
        {
            get
            {
                return (this.TargetObject ?? base.AssociatedObject);
            }
        }

        private static bool AreMethodParamsValid(ParameterInfo[] methodParams)
        {
            if (methodParams.Length == 2)
            {
                if (methodParams[0].ParameterType != typeof(object))
                {
                    return false;
                }
                if (!typeof(EventArgs).GetTypeInfo().IsAssignableFrom(methodParams[1].ParameterType.GetTypeInfo()))
                {
                    return false;
                }
            }
            else if (methodParams.Length != 0)
            {
                return false;
            }
            return true;
        }

        private MethodDescriptor FindBestMethod(object parameter)
        {
            if (parameter != null)
            {
                parameter.GetType();
            }
            return this.methodDescriptors.FirstOrDefault<MethodDescriptor>(methodDescriptor => (!methodDescriptor.HasParameters || ((parameter != null) && methodDescriptor.SecondParameterType.GetTypeInfo().IsAssignableFrom(parameter.GetType().GetTypeInfo()))));
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="parameter">The parameter of the action. If the action does not require a parameter, the parameter may be set to a null reference.</param>
        protected override void Invoke(object parameter)
        {
            if (base.AssociatedObject != null)
            {
                MethodDescriptor descriptor = this.FindBestMethod(parameter);
                if (descriptor != null)
                {
                    ParameterInfo[] parameters = descriptor.Parameters;
                    if (parameters.Length == 0)
                    {
                        descriptor.MethodInfo.Invoke(this.Target, null);
                    }
                    else if ((((parameters.Length == 2) && (base.AssociatedObject != null)) && ((parameter != null) && parameters[0].ParameterType.GetTypeInfo().IsAssignableFrom(base.AssociatedObject.GetType().GetTypeInfo()))) && parameters[1].ParameterType.GetTypeInfo().IsAssignableFrom(parameter.GetType().GetTypeInfo()))
                    {
                        descriptor.MethodInfo.Invoke(this.Target, new object[] { base.AssociatedObject, parameter });
                    }
                }
                else if (this.TargetObject != null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.CallMethodActionValidMethodNotFoundExceptionMessage, new object[] { this.MethodName, this.TargetObject.GetType().Name }));
                }
            }
        }

        private bool IsMethodValid(MethodInfo method)
        {
            if (!string.Equals(method.Name, this.MethodName, StringComparison.Ordinal))
            {
                return false;
            }
            if (method.ReturnType != typeof(void))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called after the action is attached to an AssociatedObject.
        /// </summary>
        /// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
        protected override void OnAttached()
        {
            base.OnAttached();
            this.UpdateMethodInfo();
        }

        /// <summary>
        /// Called when the action is getting detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        /// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
        protected override void OnDetaching()
        {
            this.methodDescriptors.Clear();
            base.OnDetaching();
        }

        private static void OnMethodNameChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((CallMethodAction)sender).UpdateMethodInfo();
        }

        private static void OnTargetObjectChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((CallMethodAction)sender).UpdateMethodInfo();
        }

        private void UpdateMethodInfo()
        {
            this.methodDescriptors.Clear();
            if ((this.Target != null) && !string.IsNullOrEmpty(this.MethodName))
            {
                //foreach (MethodInfo info in this.Target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
                foreach (MethodInfo info in this.Target.GetType().GetRuntimeMethods().Where(m => m.IsPublic && !m.IsStatic))
                {
                    if (this.IsMethodValid(info))
                    {
                        ParameterInfo[] parameters = info.GetParameters();
                        if (AreMethodParamsValid(parameters))
                        {
                            this.methodDescriptors.Add(new MethodDescriptor(info, parameters));
                        }
                    }
                }
                this.methodDescriptors = this.methodDescriptors.OrderByDescending<MethodDescriptor, int>(delegate(MethodDescriptor methodDescriptor)
                {
                    int num = 0;
                    if (methodDescriptor.HasParameters)
                    {
                        for (Type type = methodDescriptor.SecondParameterType; type != typeof(EventArgs); type = type.GetTypeInfo().BaseType)
                        {
                            num++;
                        }
                    }
                    return (methodDescriptor.ParameterCount + num);
                }).ToList<MethodDescriptor>();
            }
        }
        // Nested Types
        private class MethodDescriptor
        {
            // Methods
            public MethodDescriptor(MethodInfo methodInfo, ParameterInfo[] methodParams)
            {
                this.MethodInfo = methodInfo;
                this.Parameters = methodParams;
            }

            // Properties
            public bool HasParameters
            {
                get
                {
                    return (this.Parameters.Length > 0);
                }
            }

            public MethodInfo MethodInfo { get; private set; }

            public int ParameterCount
            {
                get
                {
                    return this.Parameters.Length;
                }
            }

            public ParameterInfo[] Parameters { get; private set; }

            public Type SecondParameterType
            {
                get
                {
                    if (this.Parameters.Length >= 2)
                    {
                        return this.Parameters[1].ParameterType;
                    }
                    return null;
                }
            }
        }
    }
}