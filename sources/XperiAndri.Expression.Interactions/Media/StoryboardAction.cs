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
    /// For action authors, this class provides a standard way to target a Storyboard. Design tools may choose to provide a
    /// special editing experience for classes that inherit from this action, thereby improving the designer experience.
    /// </remarks>
    public abstract class StoryboardAction : TriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty StoryboardProperty = DependencyProperty.Register("Storyboard", typeof(Storyboard), typeof(StoryboardAction), new PropertyMetadata(default(StoryboardAction), new PropertyChangedCallback(StoryboardAction.OnStoryboardChanged)));

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

        protected StoryboardAction()
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
            StoryboardAction action = sender as StoryboardAction;
            if (action != null)
            {
                action.OnStoryboardChanged(args);
            }
        }
    }
}