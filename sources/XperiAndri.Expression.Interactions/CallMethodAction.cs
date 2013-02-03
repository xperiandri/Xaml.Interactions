using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity
{
    public class CallMethodAction : TriggerAction<FrameworkElement>
    {
        // Fields
        private List<MethodDescriptor> methodDescriptors = new List<MethodDescriptor>();

        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(CallMethodAction), new PropertyMetadata(null, new PropertyChangedCallback(CallMethodAction.OnMethodNameChanged)));
        public static readonly DependencyProperty TargetObjectProperty = DependencyProperty.Register("TargetObject", typeof(object), typeof(CallMethodAction), new PropertyMetadata(null, new PropertyChangedCallback(CallMethodAction.OnTargetObjectChanged)));

        // Methods
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

        protected override void OnAttached()
        {
            base.OnAttached();
            this.UpdateMethodInfo();
        }

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

        // Properties
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

        private object Target
        {
            get
            {
                return (this.TargetObject ?? base.AssociatedObject);
            }
        }

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