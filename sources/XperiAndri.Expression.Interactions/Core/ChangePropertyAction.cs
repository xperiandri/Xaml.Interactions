using System;
using System.Globalization;
using System.Reflection;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// An action that will change a specified property to a specified value when invoked.
    /// </summary>
    internal class ChangePropertyAction : TargetedTriggerAction<object>
    {
        public static readonly DependencyProperty DurationProperty = DependencyProperty.Register("Duration", typeof(Duration), typeof(ChangePropertyAction), null);

        /// <summary>
        /// Gets or sets the duration of the animation that will occur when the ChangePropertyAction is invoked.  This is a dependency property.
        /// If the duration is unset, no animation will be applied.
        /// </summary>
        public Duration Duration
        {
            get
            {
                return (Duration)base.GetValue(DurationProperty);
            }
            set
            {
                base.SetValue(DurationProperty, value);
            }
        }

        public static readonly DependencyProperty EaseProperty = DependencyProperty.Register("Ease", typeof(EasingFunctionBase), typeof(ChangePropertyAction), null);
        
        /// <summary>
        /// Gets or sets the easing function to use with the animation when the ChangePropertyAction is invoked.  This is a dependency property.
        /// </summary>
        public EasingFunctionBase Ease
        {
            get
            {
                return (EasingFunctionBase)base.GetValue(EaseProperty);
            }
            set
            {
                base.SetValue(EaseProperty, value);
            }
        }

        public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register("Increment", typeof(bool), typeof(ChangePropertyAction), null);

        /// <summary>
        /// Increment by Value if true; otherwise, set the value directly. If the property cannot be incremented, it will instead try to set the value directly.
        /// </summary>
        public bool Increment
        {
            get
            {
                return (bool)base.GetValue(IncrementProperty);
            }
            set
            {
                base.SetValue(IncrementProperty, value);
            }
        }

        public static readonly DependencyProperty PropertyNameProperty = DependencyProperty.Register("PropertyName", typeof(string), typeof(ChangePropertyAction), null);

        /// <summary>
        /// Gets or sets the name of the property to change. This is a dependency property.
        /// </summary>
        /// <value>The name of the property to change.</value>
        public string PropertyName
        {
            get
            {
                return (string)base.GetValue(PropertyNameProperty);
            }
            set
            {
                base.SetValue(PropertyNameProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ChangePropertyAction), null);

        /// <summary>
        /// Gets or sets the value to set. This is a dependency property.
        /// </summary>
        /// <value>The value to set.</value>
        public object Value
        {
            get
            {
                return base.GetValue(ValueProperty);
            }
            set
            {
                base.SetValue(ValueProperty, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Core.ChangePropertyAction"/> class.
        /// </summary>
        public ChangePropertyAction()
        {
            
        }

        private void AnimatePropertyChange(PropertyInfo propertyInfo, object fromValue, object newValue)
        {
            Timeline timeline;
            Storyboard storyboard = new Storyboard();
            if (typeof(double).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
            {
                timeline = this.CreateDoubleAnimation((double)fromValue, (double)newValue);
            }
            else if (typeof(Color).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
            {
                timeline = this.CreateColorAnimation((Color)fromValue, (Color)newValue);
            }
            else if (typeof(Point).GetTypeInfo().IsAssignableFrom(propertyInfo.PropertyType.GetTypeInfo()))
            {
                timeline = this.CreatePointAnimation((Point)fromValue, (Point)newValue);
            }
            else
            {
                timeline = this.CreateKeyFrameAnimation(fromValue, newValue);
            }
            timeline.Duration = this.Duration;
            storyboard.Children.Add(timeline);
            Storyboard.SetTarget(storyboard, (DependencyObject)base.Target);
            Storyboard.SetTargetProperty(storyboard, propertyInfo.Name);
            storyboard.Completed += (o, e) => propertyInfo.SetValue(this.Target, newValue, new object[0]);
            storyboard.FillBehavior = FillBehavior.Stop;
            storyboard.Begin();
        }

        private Timeline CreateColorAnimation(Color fromValue, Color newValue)
        {
            return new ColorAnimation { From = new Color?(fromValue), To = new Color?(newValue), EasingFunction = this.Ease };
        }

        private Timeline CreateDoubleAnimation(double fromValue, double newValue)
        {
            return new DoubleAnimation { From = new double?(fromValue), To = new double?(newValue), EasingFunction = this.Ease };
        }

        private Timeline CreateKeyFrameAnimation(object newValue, object fromValue)
        {
            ObjectAnimationUsingKeyFrames frames = new ObjectAnimationUsingKeyFrames();
            DiscreteObjectKeyFrame frame = new DiscreteObjectKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(new TimeSpan(0L)),
                Value = fromValue
            };
            DiscreteObjectKeyFrame frame2 = new DiscreteObjectKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(this.Duration.TimeSpan),
                Value = newValue
            };
            frames.KeyFrames.Add(frame);
            frames.KeyFrames.Add(frame2);
            return frames;
        }

        private Timeline CreatePointAnimation(Point fromValue, Point newValue)
        {
            return new PointAnimation { From = new Point?(fromValue), To = new Point?(newValue), EasingFunction = this.Ease };
        }

        private static object GetCurrentPropertyValue(object target, PropertyInfo propertyInfo)
        {
            FrameworkElement element = target as FrameworkElement;
            target.GetType();
            object obj2 = propertyInfo.GetValue(target, null);
            if ((element == null) || (!(propertyInfo.Name == "Width") && !(propertyInfo.Name == "Height")))
            {
                return obj2;
            }
            if (!double.IsNaN((double)obj2))
            {
                return obj2;
            }
            if (propertyInfo.Name == "Width")
            {
                return element.ActualWidth;
            }
            return element.ActualHeight;
        }

        private object IncrementCurrentValue(PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.ChangePropertyActionCannotIncrementWriteOnlyPropertyExceptionMessage, new object[] { propertyInfo.Name }));
            }
            object currentValue = propertyInfo.GetValue(base.Target, null);
            object obj3 = currentValue;
            Type propertyType = propertyInfo.PropertyType;
            object obj4 = this.Value;
            if ((obj4 == null) || (currentValue == null))
            {
                return obj4;
            }
            if (typeof(double).GetTypeInfo().IsAssignableFrom(propertyType.GetTypeInfo()))
            {
                return (((double)currentValue) + ((double)obj4));
            }
            if (typeof(int).GetTypeInfo().IsAssignableFrom(propertyType.GetTypeInfo()))
            {
                return (((int)currentValue) + ((int)obj4));
            }
            if (typeof(float).GetTypeInfo().IsAssignableFrom(propertyType.GetTypeInfo()))
            {
                return (((float)currentValue) + ((float)obj4));
            }
            if (typeof(string).GetTypeInfo().IsAssignableFrom(propertyType.GetTypeInfo()))
            {
                return (((string)currentValue) + ((string)obj4));
            }
            return TryAddition(currentValue, obj4);
        }

        /// <summary>
        /// Invokes the action.
        /// </summary>
        /// <param name="parameter">The parameter of the action. If the action does not require a parameter, then the parameter may be set to a null reference.</param>
        /// <exception cref="System.ArgumentException">A property with <cref="XperiAndri.Expression.Interactivity.Core.ChangePropertyAction.PropertyName"/> could not be found on the Target.</exception>
        /// <exception cref="System.ArgumentException">Could not set <cref="XperiAndri.Expression.Interactivity.Core.ChangePropertyAction.PropertyName"/> to the value specified by <cref="XperiAndri.Expression.Interactivity.Core.ChangePropertyAction.Value"/>.</exception>
        protected override void Invoke(object parameter)
        {
            if (((base.AssociatedObject != null) && !string.IsNullOrEmpty(this.PropertyName)) && (base.Target != null))
            {
                Type targetType = base.Target.GetType();
                PropertyInfo property = targetType.GetRuntimeProperty(this.PropertyName);
                this.ValidateProperty(property);
                object newValue = this.Value;
                Exception innerException = null;
                try
                {
                    if (this.Duration.HasTimeSpan)
                    {
                        this.ValidateAnimationPossible(targetType);
                        object currentPropertyValue = GetCurrentPropertyValue(base.Target, property);
                        this.AnimatePropertyChange(property, currentPropertyValue, newValue);
                    }
                    else
                    {
                        if (this.Increment)
                        {
                            newValue = this.IncrementCurrentValue(property);
                        }
                        property.SetValue(base.Target, newValue, new object[0]);
                    }
                }
                catch (FormatException exception2)
                {
                    innerException = exception2;
                }
                catch (ArgumentException exception3)
                {
                    innerException = exception3;
                }

                //catch (MethodAccessException exception4)
                //{
                //    innerException = exception4;
                //}
                if (innerException != null)
                {
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.ChangePropertyActionCannotSetValueExceptionMessage, new object[] { (this.Value != null) ? this.Value.GetType().Name : "null", this.PropertyName, property.PropertyType.Name }), innerException);
                }
            }
        }

        private static object TryAddition(object currentValue, object value)
        {
            TypeInfo c = value.GetType().GetTypeInfo();
            Type type = currentValue.GetType();
            MethodInfo info = null;
            object obj3 = value;
            foreach (MethodInfo info2 in type.GetRuntimeMethods())
                if (string.Compare(info2.Name, "op_Addition", StringComparison.Ordinal) == 0)
                {
                    ParameterInfo[] parameters = info2.GetParameters();
                    if (parameters[0].ParameterType.GetTypeInfo().IsAssignableFrom(type.GetTypeInfo())
                        && parameters[1].ParameterType.GetTypeInfo().IsAssignableFrom(c))
                    {
                        info = info2;
                        break;
                    }
                }
            if (info != null)
            {
                return info.Invoke(null, new object[] { currentValue, obj3 });
            }
            return value;
        }

        private void ValidateAnimationPossible(Type targetType)
        {
            if (this.Increment)
            {
                throw new InvalidOperationException(ExceptionStringTableHelper.ChangePropertyActionCannotIncrementAnimatedPropertyChangeExceptionMessage);
            }
            if (!typeof(DependencyObject).GetTypeInfo().IsAssignableFrom(targetType.GetTypeInfo()))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.ChangePropertyActionCannotAnimateTargetTypeExceptionMessage, new object[] { targetType.Name }));
            }
        }

        private void ValidateProperty(PropertyInfo propertyInfo)
        {
            if (propertyInfo == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.ChangePropertyActionCannotFindPropertyNameExceptionMessage, new object[] { this.PropertyName, base.Target.GetType().Name }));
            }
            if (!propertyInfo.CanWrite)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTableHelper.ChangePropertyActionPropertyIsReadOnlyExceptionMessage, new object[] { this.PropertyName, base.Target.GetType().Name }));
            }
        }
    }
}