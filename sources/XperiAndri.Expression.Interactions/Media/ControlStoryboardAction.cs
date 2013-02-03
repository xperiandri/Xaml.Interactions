using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

namespace XperiAndri.Expression.Interactivity.Media
{
    /// <summary>
    /// An action that will change the state of a targeted storyboard when invoked.
    /// </summary>
    [CLSCompliant(false)]
    public class ControlStoryboardAction : StoryboardAction
    {
        private bool isPaused;

        public static readonly DependencyProperty ControlStoryboardProperty = DependencyProperty.Register("ControlStoryboardOption", typeof(ControlStoryboardOption), typeof(ControlStoryboardAction), null);

        public ControlStoryboardOption ControlStoryboardOption
        {
            get
            {
                return (ControlStoryboardOption)base.GetValue(ControlStoryboardProperty);
            }
            set
            {
                base.SetValue(ControlStoryboardProperty, value);
            }
        }

        /// <summary>
        /// This method is called when some criteria is met and the action should be invoked. This method will attempt to
        /// change the targeted storyboard in a way defined by the ControlStoryboardOption.
        /// </summary>
        /// <param name="parameter"></param>
        protected override void Invoke(object parameter)
        {
            if ((base.AssociatedObject != null) && (base.Storyboard != null))
            {
                switch (this.ControlStoryboardOption)
                {
                    case ControlStoryboardOption.Play:
                        base.Storyboard.Begin();
                        return;

                    case ControlStoryboardOption.Stop:
                        base.Storyboard.Stop();
                        return;

                    case ControlStoryboardOption.TogglePlayPause:
                        {
                            ClockState stopped = ClockState.Stopped;
                            try
                            {
                                stopped = base.Storyboard.GetCurrentState();
                            }
                            catch (InvalidOperationException)
                            {
                            }
                            if (stopped == ClockState.Stopped)
                            {
                                this.isPaused = false;
                                base.Storyboard.Begin();
                                return;
                            }
                            if (this.isPaused)
                            {
                                this.isPaused = false;
                                base.Storyboard.Resume();
                                return;
                            }
                            this.isPaused = true;
                            base.Storyboard.Pause();
                            return;
                        }
                    case ControlStoryboardOption.Pause:
                        base.Storyboard.Pause();
                        return;

                    case ControlStoryboardOption.Resume:
                        base.Storyboard.Resume();
                        return;

                    case ControlStoryboardOption.SkipToFill:
                        base.Storyboard.SkipToFill();
                        break;

                    default:
                        return;
                }
            }
        }

        protected override void OnStoryboardChanged(DependencyPropertyChangedEventArgs args)
        {
            this.isPaused = false;
        }
    }
}