using System;
using System.Linq;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace XperiAndri.Expression.Interactivity.Media
{
    /// <summary>
    /// An abstract class that provides the ability to target a Storyboard.
    /// </summary>
    /// <remarks>
    /// For Trigger authors, this class provides a standard way to target a Storyboard. Design tools may choose to provide a
    /// special editing experience for classes that inherit from this trigger, thereby improving the designer experience.
    /// </remarks>
    public abstract class StoryboardTrigger : TriggerBase<FrameworkElement>
    {
        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(StoryboardTrigger), new PropertyMetadata(new PropertyChangedCallback(StoryboardTrigger.OnStoryboardChanged)));

        /// <summary>
        /// The targeted Storyboard. This is a dependency property.
        /// </summary>
        public Storyboard Storyboard
        {
            get
            {
                return (Storyboard)base.GetValue(StoryboardProperty);
            }
            set
            {
                base.SetValue(StoryboardProperty, value);
            }
        }

        protected StoryboardTrigger()
        {
        }

        /// <summary>
        /// This method is called when the Storyboard property is changed.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnStoryboardChanged(DependencyPropertyChangedEventArgs args)
        {
        }

        private static void OnStoryboardChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            StoryboardTrigger trigger = sender as StoryboardTrigger;
            if (trigger != null)
            {
                trigger.OnStoryboardChanged(args);
            }
        }
    }
}