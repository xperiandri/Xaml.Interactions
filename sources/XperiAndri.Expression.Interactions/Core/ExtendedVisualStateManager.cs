using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// ExtendedVisualStateManager is a custom VisualStateManager that can smooth out the animation of layout properties.
    /// With this custom VisualStateManager, states can include changes to properties like Grid.Column, can change element heights to or from Auto, and so on.
    /// These changes will be smoothed out over time using the GeneratedDuration and GeneratedEasingFunction of the appropriate transition.
    /// See the "VisualStateManager overrides" region below for a general description of the algorithm.
    /// </summary>
    public class ExtendedVisualStateManager : VisualStateManager
    {
        /// <summary>
        /// A VisualStateGroup can use Fluid Layout or not.
        /// </summary>
        public static readonly DependencyProperty UseFluidLayoutProperty = DependencyProperty.RegisterAttached("UseFluidLayout", typeof(bool), typeof(ExtendedVisualStateManager), new PropertyMetadata(false));

        /// <summary>
        /// Visibility is shadowed by a custom attached property at runtime.
        /// </summary>
        public static readonly DependencyProperty RuntimeVisibilityPropertyProperty = DependencyProperty.RegisterAttached("RuntimeVisibilityProperty", typeof(DependencyProperty), typeof(ExtendedVisualStateManager), new PropertyMetadata(default(DependencyProperty)));

        /// <summary>
        /// A VisualStateGroup keeps a list of these original values in an attached property.
        /// </summary>
        internal static readonly DependencyProperty OriginalLayoutValuesProperty = DependencyProperty.RegisterAttached("OriginalLayoutValues", typeof(List<OriginalLayoutValueRecord>), typeof(ExtendedVisualStateManager), new PropertyMetadata(default(List<OriginalLayoutValueRecord>)));

        /// <summary>
        /// For every state, the layout-specific properties get extracted and then are attached to the state. These properties are removed from the state itself.
        /// </summary>
        internal static readonly DependencyProperty LayoutStoryboardProperty = DependencyProperty.RegisterAttached("LayoutStoryboard", typeof(Storyboard), typeof(ExtendedVisualStateManager), new PropertyMetadata(default(Storyboard)));

        /// <summary>
        /// Remember the current state.
        /// </summary>
        internal static readonly DependencyProperty CurrentStateProperty = DependencyProperty.RegisterAttached("CurrentState", typeof(VisualState), typeof(ExtendedVisualStateManager), new PropertyMetadata(default(VisualState)));

        /// <summary>
        /// This is the set of elements that are currently in motion.
        /// </summary>
        private static List<FrameworkElement> MovingElements;

        /// <summary>
        /// This is the storyboard that is animating the transition.
        /// </summary>
        private static Storyboard LayoutTransitionStoryboard;

        /// <summary>
        /// This list contains all the known layout properties.
        /// </summary>
        private readonly static List<DependencyProperty> LayoutProperties = new List<DependencyProperty> { Grid.ColumnProperty, Grid.ColumnSpanProperty, Grid.RowProperty, Grid.RowSpanProperty, Canvas.LeftProperty, Canvas.TopProperty, FrameworkElement.WidthProperty, FrameworkElement.HeightProperty, FrameworkElement.MinWidthProperty, FrameworkElement.MinHeightProperty, FrameworkElement.MaxWidthProperty, FrameworkElement.MaxHeightProperty, FrameworkElement.MarginProperty, FrameworkElement.HorizontalAlignmentProperty, FrameworkElement.VerticalAlignmentProperty, UIElement.VisibilityProperty, StackPanel.OrientationProperty };

        /// <summary>
        /// Silverlight does not provide a direct means of getting a DependencyProperty from a PropertyPath, so this structure is used
        /// to locate tracked paths.
        /// </summary>
        private readonly static Dictionary<string, DependencyProperty> PathToPropertyMap;
        
        private readonly static List<DependencyProperty> ChildAffectingLayoutProperties = new List<DependencyProperty> { StackPanel.OrientationProperty };

        private static DependencyProperty RuntimeVisibility;
        
        static ExtendedVisualStateManager()
        {
            Dictionary<string, DependencyProperty> dictionary = new Dictionary<string, DependencyProperty>();
            dictionary.Add("(Grid.Column)", Grid.ColumnProperty);
            dictionary.Add("(Grid.ColumnSpan)", Grid.ColumnSpanProperty);
            dictionary.Add("(Grid.Row)", Grid.RowProperty);
            dictionary.Add("(Grid.RowSpan)", Grid.RowSpanProperty);
            dictionary.Add("(Canvas.Left)", Canvas.LeftProperty);
            dictionary.Add("(Canvas.Top)", Canvas.TopProperty);
            dictionary.Add("(FrameworkElement.Width)", FrameworkElement.WidthProperty);
            dictionary.Add("(FrameworkElement.Height)", FrameworkElement.HeightProperty);
            dictionary.Add("(FrameworkElement.MinWidth)", FrameworkElement.MinWidthProperty);
            dictionary.Add("(FrameworkElement.MinHeight)", FrameworkElement.MinHeightProperty);
            dictionary.Add("(FrameworkElement.MaxWidth)", FrameworkElement.MaxWidthProperty);
            dictionary.Add("(FrameworkElement.MaxHeight)", FrameworkElement.MaxHeightProperty);
            dictionary.Add("(FrameworkElement.Margin)", FrameworkElement.MarginProperty);
            dictionary.Add("(FrameworkElement.HorizontalAlignment)", FrameworkElement.HorizontalAlignmentProperty);
            dictionary.Add("(FrameworkElement.VerticalAlignment)", FrameworkElement.VerticalAlignmentProperty);
            dictionary.Add("(UIElement.Visibility)", UIElement.VisibilityProperty);
            dictionary.Add("(Control.Width)", FrameworkElement.WidthProperty);
            dictionary.Add("(Control.Height)", FrameworkElement.HeightProperty);
            dictionary.Add("(Control.MinWidth)", FrameworkElement.MinWidthProperty);
            dictionary.Add("(Control.MinHeight)", FrameworkElement.MinHeightProperty);
            dictionary.Add("(Control.MaxWidth)", FrameworkElement.MaxWidthProperty);
            dictionary.Add("(Control.MaxHeight)", FrameworkElement.MaxHeightProperty);
            dictionary.Add("(Control.Margin)", FrameworkElement.MarginProperty);
            dictionary.Add("(Control.HorizontalAlignment)", FrameworkElement.HorizontalAlignmentProperty);
            dictionary.Add("(Control.VerticalAlignment)", FrameworkElement.VerticalAlignmentProperty);
            dictionary.Add("(Control.Visibility)", UIElement.VisibilityProperty);
            dictionary.Add("(StackPanel.Orientation)", StackPanel.OrientationProperty);
            PathToPropertyMap = dictionary;
        }

        private static object CacheActualValueHelper(DependencyObject dependencyObject, DependencyProperty property)
        {
            return dependencyObject.GetValue(property);
        }

        private static object CacheLocalValueHelper(DependencyObject dependencyObject, DependencyProperty property)
        {
            return dependencyObject.ReadLocalValue(property);
        }

        private static void control_LayoutUpdated(object sender, object e)
        {
            if (LayoutTransitionStoryboard != null)
            {
                foreach (FrameworkElement element in MovingElements)
                {
                    WrapperCanvas parent = element.Parent as WrapperCanvas;
                    if (parent != null)
                    {
                        Rect layoutRect = GetLayoutRect(parent);
                        Rect newRect = parent.NewRect;
                        TranslateTransform renderTransform = parent.RenderTransform as TranslateTransform;
                        double transformX = (renderTransform == null) ? 0.0 : renderTransform.X;
                        double transformY = (renderTransform == null) ? 0.0 : renderTransform.Y;
                        double offsetX = newRect.Left - layoutRect.Left;
                        double offsetY = newRect.Top - layoutRect.Top;
                        if ((transformX != offsetX) || (transformY != offsetY))
                        {
                            if (renderTransform == null)
                            {
                                renderTransform = new TranslateTransform();
                                parent.RenderTransform = renderTransform;
                            }
                            renderTransform.X = offsetX;
                            renderTransform.Y = offsetY;
                        }
                    }
                }
            }
        }

        private static object ConvertValueToExpectedType(DependencyProperty property, object value)
        {
            if (IsVisibilityProperty(property) && !(value is Visibility))
            {
                value = Enum.Parse(typeof(Visibility), value.ToString(), true);
                return value;
            }
            if ((property == FrameworkElement.HorizontalAlignmentProperty) && !(value is HorizontalAlignment))
            {
                value = Enum.Parse(typeof(HorizontalAlignment), value.ToString(), true);
                return value;
            }
            if ((property == FrameworkElement.VerticalAlignmentProperty) && !(value is VerticalAlignment))
            {
                value = Enum.Parse(typeof(VerticalAlignment), value.ToString(), true);
                return value;
            }
            if ((property == StackPanel.OrientationProperty) && !(value is Orientation))
            {
                value = Enum.Parse(typeof(Orientation), value.ToString(), true);
                return value;
            }
            if ((property == FrameworkElement.MarginProperty) && !(value is Thickness))
            {
                string[] strArray = value.ToString().Split(new char[] { ',' });
                double left = double.Parse(strArray[0], CultureInfo.InvariantCulture);
                double top = (strArray.Length < 1) ? left : double.Parse(strArray[1], CultureInfo.InvariantCulture);
                double right = (strArray.Length < 2) ? left : double.Parse(strArray[2], CultureInfo.InvariantCulture);
                double bottom = (strArray.Length < 3) ? top : double.Parse(strArray[3], CultureInfo.InvariantCulture);
                value = new Thickness(left, top, right, bottom);
            }
            return value;
        }

        /// <summary>
        /// Copy the layout properties from the source element to the target element, clearing them from the source.
        /// </summary>
        /// <param name="source">The source of the layout properties</param>
        /// <param name="target">The destination of the layout properties</param>
        private static void CopyLayoutProperties(FrameworkElement source, FrameworkElement target, bool restoring)
        {
            WrapperCanvas canvas = (restoring ? ((WrapperCanvas)source) : ((WrapperCanvas)target)) as WrapperCanvas;
            if (canvas.LocalValueCache == null)
                canvas.LocalValueCache = new Dictionary<DependencyProperty, object>();

            foreach (DependencyProperty property in LayoutProperties)
            {
                if (!ChildAffectingLayoutProperties.Contains(property))
                {
                    object actualValue = CacheActualValueHelper(source, property);
                    object localValue = CacheLocalValueHelper(source, property);
                    if (actualValue != localValue)
                        return;

                    if (restoring)
                    {
                        ReplaceCachedLocalValueHelper(target, property, canvas.LocalValueCache[property]);
                    }
                    else
                    {
                        object targetValue = target.GetValue(property);
                        canvas.LocalValueCache[property] = localValue;
                        if (IsVisibilityProperty(property))
                        {
                            canvas.DestinationVisibilityCache = (Visibility)source.GetValue(property);
                        }
                        else
                        {
                            target.SetValue(property, source.GetValue(property));
                        }
                        source.SetValue(property, targetValue);
                    }
                }
            }
        }

        /// <summary>
        /// Create the actual storyboard that will be used to animate the transition. Use all previously calculated results.
        /// </summary>
        /// <param name="duration">The duration of the animation</param>
        /// <param name="ease">The easing function to be used in the animation</param>
        /// <param name="movingElements">The set of elements that will be moving</param>
        /// <param name="oldOpacities">The old opacities of the elements whose viisibility is changing</param>
        /// <returns>The storyboard</returns>
        private static Storyboard CreateLayoutTransitionStoryboard(VisualTransition transition, List<FrameworkElement> movingElements, Dictionary<FrameworkElement, double> oldOpacities)
        {
            Duration duration = (transition != null) ? transition.GeneratedDuration : new Duration(TimeSpan.Zero);
            EasingFunctionBase function = (transition != null) ? transition.GeneratedEasingFunction : null;
            Storyboard storyboard = new Storyboard
            {
                Duration = duration
            };
            foreach (FrameworkElement element in movingElements)
            {
                WrapperCanvas parent = element.Parent as WrapperCanvas;
                if (parent != null)
                {
                    DoubleAnimation timeline = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = duration,
                        EasingFunction = function
                    };
                    Storyboard.SetTarget(timeline, parent);
                    Storyboard.SetTargetProperty(timeline, "SimulationProgress" /*WrapperCanvas.SimulationProgressProperty*/);
                    storyboard.Children.Add(timeline);
                    parent.SimulationProgress = 1.0;
                    Rect newRect = parent.NewRect;
                    if (!IsClose(parent.Width, newRect.Width))
                    {
                        DoubleAnimation animation3 = new DoubleAnimation
                        {
                            From = new double?(newRect.Width),
                            To = new double?(newRect.Width),
                            Duration = duration
                        };
                        Storyboard.SetTarget(animation3, parent);
                        Storyboard.SetTargetProperty(animation3, "Width" /*FrameworkElement.WidthProperty*/);
                        storyboard.Children.Add(animation3);
                    }
                    if (!IsClose(parent.Height, newRect.Height))
                    {
                        DoubleAnimation animation5 = new DoubleAnimation
                        {
                            From = new double?(newRect.Height),
                            To = new double?(newRect.Height),
                            Duration = duration
                        };
                        Storyboard.SetTarget(animation5, parent);
                        Storyboard.SetTargetProperty(animation5, "Height" /*FrameworkElement.HeightProperty*/);
                        storyboard.Children.Add(animation5);
                    }
                    if (parent.DestinationVisibilityCache == Visibility.Collapsed)
                    {
                        Thickness margin = parent.Margin;
                        if ((!IsClose(margin.Left, 0.0) || !IsClose(margin.Top, 0.0)) || (!IsClose(margin.Right, 0.0) || !IsClose(margin.Bottom, 0.0)))
                        {
                            ObjectAnimationUsingKeyFrames frames = new ObjectAnimationUsingKeyFrames
                            {
                                Duration = duration
                            };
                            DiscreteObjectKeyFrame frame2 = new DiscreteObjectKeyFrame
                            {
                                KeyTime = TimeSpan.Zero
                            };
                            Thickness thickness2 = new Thickness();
                            frame2.Value = thickness2;
                            DiscreteObjectKeyFrame frame = frame2;
                            frames.KeyFrames.Add(frame);
                            Storyboard.SetTarget(frames, parent);
                            Storyboard.SetTargetProperty(frames, "Margin" /*FrameworkElement.MarginProperty*/);
                            storyboard.Children.Add(frames);
                        }
                        if (!IsClose(parent.MinWidth, 0.0))
                        {
                            DoubleAnimation animation7 = new DoubleAnimation
                            {
                                From = 0.0,
                                To = 0.0,
                                Duration = duration
                            };
                            Storyboard.SetTarget(animation7, parent);
                            Storyboard.SetTargetProperty(animation7, "MinWidth" /*FrameworkElement.MinWidthProperty*/);
                            storyboard.Children.Add(animation7);
                        }
                        if (!IsClose(parent.MinHeight, 0.0))
                        {
                            DoubleAnimation animation9 = new DoubleAnimation
                            {
                                From = 0.0,
                                To = 0.0,
                                Duration = duration
                            };
                            Storyboard.SetTarget(animation9, parent);
                            Storyboard.SetTargetProperty(animation9, "MinHeight" /*FrameworkElement.MinHeightProperty*/);
                            storyboard.Children.Add(animation9);
                        }
                    }
                }
            }
            foreach (FrameworkElement element2 in oldOpacities.Keys)
            {
                WrapperCanvas target = element2.Parent as WrapperCanvas;
                if (target != null)
                {
                    double a = oldOpacities[element2];
                    double num2 = (target.DestinationVisibilityCache == Visibility.Visible) ? 1.0 : 0.0;
                    if (!IsClose(a, 1.0) || !IsClose(num2, 1.0))
                    {
                        DoubleAnimation animation11 = new DoubleAnimation
                        {
                            From = new double?(a),
                            To = new double?(num2),
                            Duration = duration,
                            EasingFunction = function
                        };
                        Storyboard.SetTarget(animation11, target);
                        Storyboard.SetTargetProperty(animation11, "Opacity" /*UIElement.OpacityProperty*/);
                        storyboard.Children.Add(animation11);
                    }
                }
            }
            return storyboard;
        }

        /// <summary>
        /// Remove all layout-affecting properties from the Storyboard for the state and cache them in an attached property.
        /// </summary>
        /// <param name="state">The state you are moving to</param>
        /// <returns>A Storyboard containing the layout properties in that state</returns>
        private static Storyboard ExtractLayoutStoryboard(VisualState state)
        {
            Storyboard layoutStoryboard = null;
            if (state.Storyboard != null)
            {
                layoutStoryboard = GetLayoutStoryboard(state.Storyboard);
                if (layoutStoryboard == null)
                {
                    layoutStoryboard = new Storyboard();
                    for (int i = state.Storyboard.Children.Count - 1; i >= 0; i--)
                    {
                        Timeline timeline = state.Storyboard.Children[i];
                        if (LayoutPropertyFromTimeline(timeline, false) != null)
                        {
                            state.Storyboard.Children.RemoveAt(i);
                            layoutStoryboard.Children.Add(timeline);
                        }
                    }
                    SetLayoutStoryboard(state.Storyboard, layoutStoryboard);
                }
            }
            if (layoutStoryboard == null)
            {
                return new Storyboard();
            }
            return layoutStoryboard;
        }

        /// <summary>
        /// The set of target elements is the set of all elements that might have moved in a layout transition. This set is the closure of:
        ///  - Elements with layout properties animated in the state.
        ///  - Siblings of elements in the set.
        ///  - Parents of elements in the set.
        ///  
        /// Subsequent code will check these rectangles both before and after the layout change.
        /// </summary>
        /// <param name="control">The control whose layout is changing state</param>
        /// <param name="layoutStoryboard">The storyboard containing the layout changes</param>
        /// <param name="originalValueRecords">Any previous values from previous state navigations that might be reverted</param>
        /// <param name="movingElements">The set of elements currently in motion, if there is a state change transition ongoing</param>
        /// <returns>The full set of elements whose layout may have changed</returns>
        private static List<FrameworkElement> FindTargetElements(Control control, FrameworkElement templateRoot, Storyboard layoutStoryboard, List<OriginalLayoutValueRecord> originalValueRecords, List<FrameworkElement> movingElements)
        {
            List<FrameworkElement> list = new List<FrameworkElement>();
            if (movingElements != null)
            {
                list.AddRange(movingElements);
            }
            foreach (Timeline timeline in layoutStoryboard.Children)
            {
                FrameworkElement item = (FrameworkElement)GetTimelineTarget(control, templateRoot, timeline);
                if (item != null)
                {
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                    if (ChildAffectingLayoutProperties.Contains(LayoutPropertyFromTimeline(timeline, false)))
                    {
                        Panel panel = item as Panel;
                        if (panel != null)
                        {
                            foreach (FrameworkElement element2 in panel.Children)
                            {
                                if (!list.Contains(element2) && !(element2 is WrapperCanvas))
                                {
                                    list.Add(element2);
                                }
                            }
                        }
                    }
                }
            }
            foreach (OriginalLayoutValueRecord record in originalValueRecords)
            {
                if (!list.Contains(record.Element))
                {
                    list.Add(record.Element);
                }
                if (ChildAffectingLayoutProperties.Contains(record.Property))
                {
                    Panel element = record.Element as Panel;
                    if (element != null)
                    {
                        foreach (FrameworkElement element3 in element.Children)
                        {
                            if (!list.Contains(element3) && !(element3 is WrapperCanvas))
                            {
                                list.Add(element3);
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < list.Count; i++)
            {
                FrameworkElement reference = list[i];
                FrameworkElement parent = VisualTreeHelper.GetParent(reference) as FrameworkElement;
                if (((movingElements != null) && movingElements.Contains(reference)) && (parent is WrapperCanvas))
                {
                    parent = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                }
                if (parent != null)
                {
                    if (!list.Contains(parent))
                    {
                        list.Add(parent);
                    }
                    for (int j = 0; j < VisualTreeHelper.GetChildrenCount(parent); j++)
                    {
                        FrameworkElement child = VisualTreeHelper.GetChild(parent, j) as FrameworkElement;
                        if (((child != null) && !list.Contains(child)) && !(child is WrapperCanvas))
                        {
                            list.Add(child);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Locate the transition that VisualStateManager will use to animate the change, so that the layout animation can match the duration and ease.
        /// </summary>
        /// <param name="group">The group in which the transition is taking place</param>
        /// <param name="previousState">The state that you are coming from</param>
        /// <param name="state">The state you are going to</param>
        /// <returns>The transition</returns>
        private static VisualTransition FindTransition(VisualStateGroup group, VisualState previousState, VisualState state)
        {
            string str = (previousState != null) ? previousState.Name : string.Empty;
            string str2 = (state != null) ? state.Name : string.Empty;
            int num = -1;
            VisualTransition transition = null;
            if (group.Transitions != null)
            {
                foreach (VisualTransition transition2 in group.Transitions)
                {
                    int num2 = 0;
                    if (transition2.From == str)
                    {
                        num2++;
                    }
                    else if (!string.IsNullOrEmpty(transition2.From))
                    {
                        continue;
                    }
                    if (transition2.To == str2)
                    {
                        num2 += 2;
                    }
                    else if (!string.IsNullOrEmpty(transition2.To))
                    {
                        continue;
                    }
                    if (num2 > num)
                    {
                        num = num2;
                        transition = transition2;
                    }
                }
            }
            return transition;
        }

        internal static VisualState GetCurrentState(DependencyObject obj)
        {
            return (VisualState)obj.GetValue(CurrentStateProperty);
        }

        /// <summary>
        /// Get the layout rectangle of an element, by getting the layout slot and then computing which portion of the slot is being used.
        /// </summary>
        /// <param name="element">The element whose rect we want to get</param>
        /// <returns>The layout rect of that element</returns>
        internal static Rect GetLayoutRect(FrameworkElement element)
        {
            double actualWidth = element.ActualWidth;
            double actualHeight = element.ActualHeight;
            if ((element is Image) || (element is MediaElement))
            {
                if (element.Parent is Canvas)
                {
                    actualWidth = double.IsNaN(element.Width) ? actualWidth : element.Width;
                    actualHeight = double.IsNaN(element.Height) ? actualHeight : element.Height;
                }
                else
                {
                    actualWidth = element.RenderSize.Width;
                    actualHeight = element.RenderSize.Height;
                }
            }
            actualWidth = (element.Visibility == Visibility.Collapsed) ? 0.0 : actualWidth;
            actualHeight = (element.Visibility == Visibility.Collapsed) ? 0.0 : actualHeight;
            Thickness margin = element.Margin;
            Rect layoutSlot;
            if (element.Parent is Canvas)
                layoutSlot = new Rect(Canvas.GetLeft(element), Canvas.GetTop(element), actualWidth, actualHeight);
            else
                layoutSlot = LayoutInformation.GetLayoutSlot(element);
            double x = 0.0;
            double y = 0.0;
            switch (element.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    x = layoutSlot.Left + margin.Left;
                    break;

                case HorizontalAlignment.Center:
                    x = ((((layoutSlot.Left + margin.Left) + layoutSlot.Right) - margin.Right) / 2.0) - (actualWidth / 2.0);
                    break;

                case HorizontalAlignment.Right:
                    x = (layoutSlot.Right - margin.Right) - actualWidth;
                    break;

                case HorizontalAlignment.Stretch:
                    x = Math.Max((double)(layoutSlot.Left + margin.Left), (double)(((((layoutSlot.Left + margin.Left) + layoutSlot.Right) - margin.Right) / 2.0) - (actualWidth / 2.0)));
                    break;
            }
            switch (element.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    y = layoutSlot.Top + margin.Top;
                    break;

                case VerticalAlignment.Center:
                    y = ((((layoutSlot.Top + margin.Top) + layoutSlot.Bottom) - margin.Bottom) / 2.0) - (actualHeight / 2.0);
                    break;

                case VerticalAlignment.Bottom:
                    y = (layoutSlot.Bottom - margin.Bottom) - actualHeight;
                    break;

                case VerticalAlignment.Stretch:
                    y = Math.Max((double)(layoutSlot.Top + margin.Top), (double)(((((layoutSlot.Top + margin.Top) + layoutSlot.Bottom) - margin.Bottom) / 2.0) - (actualHeight / 2.0)));
                    break;
            }
            return new Rect(x, y, actualWidth, actualHeight);
        }

        internal static Storyboard GetLayoutStoryboard(DependencyObject obj)
        {
            return (Storyboard)obj.GetValue(LayoutStoryboardProperty);
        }

        /// <summary>
        /// Get the opacities of elements at the time of the state change, instead of visibilities, because the state change may be in process and the current value is the most important.
        /// </summary>
        /// <param name="control">The control whose state is changing</param>
        /// <param name="layoutStoryboard">The storyboard with the layout properties</param>
        /// <param name="originalValueRecords">The set of original values</param>
        /// <returns></returns>
        private static Dictionary<FrameworkElement, double> GetOldOpacities(Control control, FrameworkElement templateRoot, Storyboard layoutStoryboard, List<OriginalLayoutValueRecord> originalValueRecords, List<FrameworkElement> movingElements)
        {
            Dictionary<FrameworkElement, double> dictionary = new Dictionary<FrameworkElement, double>();
            if (movingElements != null)
            {
                foreach (FrameworkElement element in movingElements)
                {
                    WrapperCanvas parent = element.Parent as WrapperCanvas;
                    if (parent != null)
                    {
                        dictionary.Add(element, parent.Opacity);
                    }
                }
            }
            for (int i = originalValueRecords.Count - 1; i >= 0; i--)
            {
                double num2;
                OriginalLayoutValueRecord record = originalValueRecords[i];
                if (IsVisibilityProperty(record.Property) && !dictionary.TryGetValue(record.Element, out num2))
                {
                    num2 = (((Visibility)record.Element.GetValue(record.Property)) == Visibility.Visible) ? 1.0 : 0.0;
                    dictionary.Add(record.Element, num2);
                }
            }
            foreach (Timeline timeline in layoutStoryboard.Children)
            {
                double num3;
                FrameworkElement key = (FrameworkElement)GetTimelineTarget(control, templateRoot, timeline);
                DependencyProperty property = LayoutPropertyFromTimeline(timeline, true);
                if (((key != null) && IsVisibilityProperty(property)) && !dictionary.TryGetValue(key, out num3))
                {
                    num3 = (((Visibility)key.GetValue(property)) == Visibility.Visible) ? 1.0 : 0.0;
                    dictionary.Add(key, num3);
                }
            }
            return dictionary;
        }

        internal static List<OriginalLayoutValueRecord> GetOriginalLayoutValues(DependencyObject obj)
        {
            return (List<OriginalLayoutValueRecord>)obj.GetValue(OriginalLayoutValuesProperty);
        }

        /// <summary>
        /// Get a set of rectangles for all the elements in the target list.
        /// </summary>
        /// <param name="targets">The set of elements to consider</param>
        /// <param name="movingElements">The set of elements currently in motion</param>
        /// <returns>A Dictionary mapping elements to their rects</returns>
        private static Dictionary<FrameworkElement, Rect> GetRectsOfTargets(List<FrameworkElement> targets, List<FrameworkElement> movingElements)
        {
            Dictionary<FrameworkElement, Rect> dictionary = new Dictionary<FrameworkElement, Rect>();
            foreach (FrameworkElement element in targets)
            {
                Rect layoutRect;
                if (((movingElements != null) && movingElements.Contains(element)) && (element.Parent is WrapperCanvas))
                {
                    WrapperCanvas parent = element.Parent as WrapperCanvas;
                    layoutRect = GetLayoutRect(parent);
                    TranslateTransform renderTransform = parent.RenderTransform as TranslateTransform;
                    double left = Canvas.GetLeft(element);
                    double top = Canvas.GetTop(element);
                    layoutRect = new Rect((layoutRect.Left + (double.IsNaN(left) ? 0.0 : left)) + ((renderTransform == null) ? 0.0 : renderTransform.X), (layoutRect.Top + (double.IsNaN(top) ? 0.0 : top)) + ((renderTransform == null) ? 0.0 : renderTransform.Y), element.ActualWidth, element.ActualHeight);
                }
                else
                {
                    layoutRect = GetLayoutRect(element);
                }
                dictionary.Add(element, layoutRect);
            }
            return dictionary;
        }

        public static DependencyProperty GetRuntimeVisibilityProperty(DependencyObject obj)
        {
            return (DependencyProperty)obj.GetValue(RuntimeVisibilityPropertyProperty);
        }

        private static object GetTimelineTarget(Control control, FrameworkElement templateRoot, Timeline timeline)
        {
            string targetName = Storyboard.GetTargetName(timeline);
            if (string.IsNullOrEmpty(targetName))
            {
                return null;
            }
            if (control is UserControl)
            {
                return control.FindName(targetName);
            }
            return templateRoot.FindName(targetName);
        }

        public static bool GetUseFluidLayout(DependencyObject obj)
        {
            return (bool)obj.GetValue(UseFluidLayoutProperty);
        }

        public static bool GoToElementState(FrameworkElement root, string stateName, bool useTransitions)
        {
            ExtendedVisualStateManager customVisualStateManager = VisualStateManager.GetCustomVisualStateManager(root) as ExtendedVisualStateManager;
            return ((customVisualStateManager != null) && customVisualStateManager.GoToStateInternal(root, stateName, useTransitions));
        }

        protected override bool GoToStateCore(Control control, FrameworkElement templateRoot, string stateName, VisualStateGroup group, VisualState state, bool useTransitions)
        {
            if (control == null)
            {
                control = new ContentControl();
            }
            if ((group == null) || (state == null))
            {
                return false;
            }
            if (!GetUseFluidLayout(group))
            {
                return base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
            }
            VisualState currentState = GetCurrentState(group);
            if (currentState == state)
            {
                if (!useTransitions && (LayoutTransitionStoryboard != null))
                {
                    StopAnimations();
                }
                return true;
            }
            SetCurrentState(group, state);
            Storyboard layoutStoryboard = ExtractLayoutStoryboard(state);
            List<OriginalLayoutValueRecord> originalLayoutValues = GetOriginalLayoutValues(group);
            if (originalLayoutValues == null)
            {
                originalLayoutValues = new List<OriginalLayoutValueRecord>();
                SetOriginalLayoutValues(group, originalLayoutValues);
            }
            if (!useTransitions)
            {
                if (LayoutTransitionStoryboard != null)
                {
                    StopAnimations();
                }
                base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
                SetLayoutStoryboardProperties(control, templateRoot, layoutStoryboard, originalLayoutValues);
                return true;
            }
            if ((layoutStoryboard.Children.Count == 0) && (originalLayoutValues.Count == 0))
            {
                return base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
            }
            templateRoot.UpdateLayout();
            List<FrameworkElement> targets = FindTargetElements(control, templateRoot, layoutStoryboard, originalLayoutValues, MovingElements);
            Dictionary<FrameworkElement, Rect> rectsOfTargets = GetRectsOfTargets(targets, MovingElements);
            Dictionary<FrameworkElement, double> oldOpacities = GetOldOpacities(control, templateRoot, layoutStoryboard, originalLayoutValues, MovingElements);
            if (LayoutTransitionStoryboard != null)
            {
                templateRoot.LayoutUpdated -= ExtendedVisualStateManager.control_LayoutUpdated;
                StopAnimations();
                templateRoot.UpdateLayout();
            }
            base.GoToStateCore(control, templateRoot, stateName, group, state, useTransitions);
            SetLayoutStoryboardProperties(control, templateRoot, layoutStoryboard, originalLayoutValues);
            templateRoot.UpdateLayout();
            Dictionary<FrameworkElement, Rect> newRects = GetRectsOfTargets(targets, null);
            MovingElements = new List<FrameworkElement>();
            foreach (FrameworkElement element in targets)
            {
                if (rectsOfTargets[element] != newRects[element])
                {
                    MovingElements.Add(element);
                }
            }
            foreach (FrameworkElement element2 in oldOpacities.Keys)
            {
                if (!MovingElements.Contains(element2))
                {
                    MovingElements.Add(element2);
                }
            }
            WrapMovingElementsInCanvases(MovingElements, rectsOfTargets, newRects);
            VisualTransition transition = FindTransition(group, currentState, state);
            templateRoot.LayoutUpdated += ExtendedVisualStateManager.control_LayoutUpdated;
            LayoutTransitionStoryboard = CreateLayoutTransitionStoryboard(transition, MovingElements, oldOpacities);
            LayoutTransitionStoryboard.Completed += (sender, args) =>
            {
                templateRoot.LayoutUpdated -= ExtendedVisualStateManager.control_LayoutUpdated;
                StopAnimations();
            };
            LayoutTransitionStoryboard.Begin();
            return true;
        }

        private bool GoToStateInternal(FrameworkElement stateGroupsRoot, string stateName, bool useTransitions)
        {
            VisualStateGroup group;
            VisualState state;
            return (TryGetState(stateGroupsRoot, stateName, out group, out state) && this.GoToStateCore(null, stateGroupsRoot, stateName, group, state, useTransitions));
        }

        private static bool IsClose(double a, double b)
        {
            return (Math.Abs((double)(a - b)) < 1E-07);
        }

        private static bool IsVisibilityProperty(DependencyProperty property)
        {
            return (((RuntimeVisibility != null) && (property == RuntimeVisibility)) || (property == UIElement.VisibilityProperty));
        }

        private static DependencyProperty LayoutPropertyFromTimeline(Timeline timeline, bool forceRuntimeProperty)
        {
            DependencyProperty property;
            string targetProperty = Storyboard.GetTargetProperty(timeline);
            DependencyProperty runtimeVisibilityProperty = GetRuntimeVisibilityProperty(timeline);
            if (runtimeVisibilityProperty != null)
            {
                if (RuntimeVisibility == null)
                {
                    LayoutProperties.Add(runtimeVisibilityProperty);
                    RuntimeVisibility = runtimeVisibilityProperty;
                }
                if (forceRuntimeProperty)
                {
                    return runtimeVisibilityProperty;
                }
                return UIElement.VisibilityProperty;
            }
            if (((targetProperty != null) && PathToPropertyMap.TryGetValue(targetProperty, out property)) && LayoutProperties.Contains(property))
            {
                return property;
            }
            return null;
        }

        private static void ReplaceCachedLocalValueHelper(FrameworkElement element, DependencyProperty property, object value)
        {
            if (value == DependencyProperty.UnsetValue)
            {
                element.ClearValue(property);
            }
            else
            {
                //BindingExpression expression = value as BindingExpression;
                //if (expression != null)
                //{
                //    element.SetBinding(property, expression);
                //}
                //else
                //{
                value = ConvertValueToExpectedType(property, value);
                element.SetValue(property, value);

                //}
            }
        }

        internal static void SetCurrentState(DependencyObject obj, VisualState value)
        {
            obj.SetValue(CurrentStateProperty, value);
        }

        internal static void SetLayoutStoryboard(DependencyObject obj, Storyboard value)
        {
            obj.SetValue(LayoutStoryboardProperty, value);
        }

        /// <summary>
        /// Go through the layout Storyboard and set all the properties by using SetValue to enable calling UpdateLayout without
        /// ticking the timeline, which would cause a render.
        /// All values that are overwritten will be stored in the collection of OriginalValueRecords so that they can be replaced later.
        /// </summary>
        /// <param name="control">The control whose state is changing</param>
        /// <param name="layoutStoryboard">The storyboard holding the layout properties</param>
        /// <param name="originalValueRecords">The store of original values</param>
        private static void SetLayoutStoryboardProperties(Control control, FrameworkElement templateRoot, Storyboard layoutStoryboard, List<OriginalLayoutValueRecord> originalValueRecords)
        {
            foreach (OriginalLayoutValueRecord record in originalValueRecords)
            {
                ReplaceCachedLocalValueHelper(record.Element, record.Property, record.Value);
            }
            originalValueRecords.Clear();
            foreach (Timeline timeline in layoutStoryboard.Children)
            {
                FrameworkElement dependencyObject = (FrameworkElement)GetTimelineTarget(control, templateRoot, timeline);
                DependencyProperty property = LayoutPropertyFromTimeline(timeline, true);
                if ((dependencyObject != null) && (property != null))
                {
                    object to = null;
                    bool flag = false;
                    ObjectAnimationUsingKeyFrames frames = timeline as ObjectAnimationUsingKeyFrames;
                    if (frames != null)
                    {
                        flag = true;
                        to = frames.KeyFrames[0].Value;
                    }
                    else
                    {
                        DoubleAnimationUsingKeyFrames frames2 = timeline as DoubleAnimationUsingKeyFrames;
                        if (frames2 != null)
                        {
                            flag = true;
                            to = frames2.KeyFrames[0].Value;
                        }
                        else
                        {
                            DoubleAnimation animation = timeline as DoubleAnimation;
                            if (animation != null)
                            {
                                flag = true;
                                to = animation.To;
                            }
                        }
                    }
                    to = ConvertValueToExpectedType(property, to);
                    if (((property == FrameworkElement.WidthProperty) || (property == FrameworkElement.HeightProperty)) && (((double)to) == -1.0))
                    {
                        to = (double)1.0 / (double)0.0;
                    }
                    if (flag)
                    {
                        OriginalLayoutValueRecord item = new OriginalLayoutValueRecord
                        {
                            Element = dependencyObject,
                            Property = property,
                            Value = CacheLocalValueHelper(dependencyObject, property)
                        };
                        originalValueRecords.Add(item);
                        dependencyObject.SetValue(property, to);
                    }
                }
            }
        }

        internal static void SetOriginalLayoutValues(DependencyObject obj, List<OriginalLayoutValueRecord> value)
        {
            obj.SetValue(OriginalLayoutValuesProperty, value);
        }

        public static void SetRuntimeVisibilityProperty(DependencyObject obj, DependencyProperty value)
        {
            obj.SetValue(RuntimeVisibilityPropertyProperty, value);
        }

        public static void SetUseFluidLayout(DependencyObject obj, bool value)
        {
            obj.SetValue(UseFluidLayoutProperty, value);
        }

        /// <summary>
        /// Stop the animation and replace the layout changes that were made to support that animation.
        /// </summary>
        private static void StopAnimations()
        {
            if (LayoutTransitionStoryboard != null)
            {
                LayoutTransitionStoryboard.Stop();
                LayoutTransitionStoryboard = null;
            }
            if (MovingElements != null)
            {
                UnwrapMovingElementsFromCanvases(MovingElements);
                MovingElements = null;
            }
        }

        private static bool TryGetState(FrameworkElement element, string stateName, out VisualStateGroup group, out VisualState state)
        {
            group = null;
            state = null;
            foreach (VisualStateGroup group2 in VisualStateManager.GetVisualStateGroups(element))
            {
                foreach (VisualState state2 in group2.States)
                {
                    if (state2.Name == stateName)
                    {
                        group = group2;
                        state = state2;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Take all the elements that have been moving as a result of the layout animation, and unwrap them from their Canvases.
        /// </summary>
        /// <param name="movingElements">The set of elements that have been moving</param>
        private static void UnwrapMovingElementsFromCanvases(List<FrameworkElement> movingElements)
        {
            foreach (FrameworkElement element in movingElements)
            {
                WrapperCanvas parent = element.Parent as WrapperCanvas;
                if (parent != null)
                {
                    FrameworkElement element2 = VisualTreeHelper.GetParent(parent) as FrameworkElement;
                    parent.Children.Remove(element);
                    Panel panel = element2 as Panel;
                    if (panel != null)
                    {
                        int index = panel.Children.IndexOf(parent);
                        panel.Children.RemoveAt(index);
                        panel.Children.Insert(index, element);
                    }
                    else
                    {
                        Border border = element2 as Border;
                        if (border != null)
                        {
                            border.Child = element;
                        }
                    }
                    CopyLayoutProperties(parent, element, true);
                }
            }
        }

        /// <summary>
        /// Take all the elements that will be moving as a result of the layout animation, and wrap them in Canvases so that
        /// they do not affect their sibling elements.
        /// </summary>
        /// <param name="movingElements">The set of elements that will be moving</param>
        private static void WrapMovingElementsInCanvases(List<FrameworkElement> movingElements, Dictionary<FrameworkElement, Rect> oldRects, Dictionary<FrameworkElement, Rect> newRects)
        {
            foreach (FrameworkElement element in movingElements)
            {
                FrameworkElement parent = VisualTreeHelper.GetParent(element) as FrameworkElement;
                WrapperCanvas canvas = new WrapperCanvas
                {
                    OldRect = oldRects[element],
                    NewRect = newRects[element]
                };
                foreach (DependencyProperty property in LayoutProperties)
                {
                    if (!ChildAffectingLayoutProperties.Contains(property) && (CacheLocalValueHelper(element, property) != CacheActualValueHelper(element, property)))
                    {
                        break;
                    }
                }
                bool flag = true;
                Panel panel = parent as Panel;
                if ((panel != null) && !panel.IsItemsHost)
                {
                    int index = panel.Children.IndexOf(element);
                    panel.Children.RemoveAt(index);
                    panel.Children.Insert(index, canvas);
                }
                else
                {
                    Border border = parent as Border;
                    if (border != null)
                    {
                        border.Child = canvas;
                    }
                    else
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    canvas.Children.Add(element);
                    CopyLayoutProperties(element, canvas, false);
                }
            }
        }

        // Properties
        public static bool IsRunningFluidLayoutTransition
        {
            get
            {
                return (LayoutTransitionStoryboard != null);
            }
        }

        // Nested Types
        internal class OriginalLayoutValueRecord
        {
            // Properties
            public FrameworkElement Element { get; set; }

            public DependencyProperty Property { get; set; }

            public object Value { get; set; }
        }

        internal class WrapperCanvas : Canvas
        {
            // Fields
            internal static readonly DependencyProperty SimulationProgressProperty = DependencyProperty.Register("SimulationProgress", typeof(double), typeof(ExtendedVisualStateManager.WrapperCanvas), new PropertyMetadata(0.0, ExtendedVisualStateManager.WrapperCanvas.SimulationProgressChanged));

            // Methods
            private static void SimulationProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            {
                ExtendedVisualStateManager.WrapperCanvas canvas = d as ExtendedVisualStateManager.WrapperCanvas;
                double newValue = (double)e.NewValue;
                if ((canvas != null) && (canvas.Children.Count > 0))
                {
                    FrameworkElement element = canvas.Children[0] as FrameworkElement;
                    element.Width = Math.Max((double)0.0, (double)((canvas.OldRect.Width * newValue) + (canvas.NewRect.Width * (1.0 - newValue))));
                    element.Height = Math.Max((double)0.0, (double)((canvas.OldRect.Height * newValue) + (canvas.NewRect.Height * (1.0 - newValue))));
                    Canvas.SetLeft(element, newValue * (canvas.OldRect.Left - canvas.NewRect.Left));
                    Canvas.SetTop(element, newValue * (canvas.OldRect.Top - canvas.NewRect.Top));
                }
            }

            // Properties
            public Visibility DestinationVisibilityCache { get; set; }

            public Dictionary<DependencyProperty, object> LocalValueCache { get; set; }

            public Rect NewRect { get; set; }

            public Rect OldRect { get; set; }

            public double SimulationProgress
            {
                get
                {
                    return (double)base.GetValue(SimulationProgressProperty);
                }
                set
                {
                    base.SetValue(SimulationProgressProperty, value);
                }
            }
        }
    }
}