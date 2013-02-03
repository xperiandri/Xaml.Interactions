using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace XperiAndri.Expression.Interactivity.Media
{
    /// <summary>
    /// A trigger that listens for the completion of a Storyboard.
    /// </summary>
    public class StoryboardCompletedTrigger : StoryboardTrigger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Media.StoryboardCompletedTrigger"/> class.
        /// </summary>
        public StoryboardCompletedTrigger()
        {
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (base.Storyboard != null)
            {
                base.Storyboard.Completed -= this.Storyboard_Completed;
            }
        }

        protected override void OnStoryboardChanged(DependencyPropertyChangedEventArgs args)
        {
            Storyboard oldValue = args.OldValue as Storyboard;
            Storyboard newValue = args.NewValue as Storyboard;
            if (oldValue != newValue)
            {
                if (oldValue != null)
                {
                    oldValue.Completed -= this.Storyboard_Completed;
                }
                if (newValue != null)
                {
                    newValue.Completed += this.Storyboard_Completed;
                }
            }
        }

        private void Storyboard_Completed(object sender, object e)
        {
            base.InvokeActions(e);
        }
    }
}