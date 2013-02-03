using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Interactivity;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace XperiAndri.Expression.Interactivity.Media
{
    /// <summary>
    /// An action that will play a sound to completion.
    /// </summary>
    /// <remarks>
    /// This action is intended for use with short sound effects that don't need to be stopped or controlled. If you're trying 
    /// to create a music player or game, it may not meet your needs.
    /// </remarks>
    public class PlaySoundAction : TriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(Uri), typeof(PlaySoundAction), null);

        /// <summary>
        /// A Uri defining the location of the sound file. This is used to set the source property of the MediaElement. This is a dependency property.
        /// </summary>
        /// <remarks>
        /// The sound can be any file format supported by MediaElement. In the case of a video, it will play only the
        /// audio portion.
        /// </remarks>
        public Uri Source
        {
            get
            {
                return (Uri)base.GetValue(SourceProperty);
            }
            set
            {
                base.SetValue(SourceProperty, value);
            }
        }

        public static readonly DependencyProperty VolumeProperty = DependencyProperty.Register("Volume", typeof(double), typeof(PlaySoundAction), new PropertyMetadata(0.5));

        /// <summary>
        /// Control the volume of the sound. This is used to set the Volume property of the MediaElement. This is a dependency property.
        /// </summary>
        public double Volume
        {
            get
            {
                return (double)base.GetValue(VolumeProperty);
            }
            set
            {
                base.SetValue(VolumeProperty, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Media.PlaySoundAction"/> class.
        /// </summary>
        public PlaySoundAction()
        {
            
        }

        /// <summary>
        /// This method is called when some criteria are met and the action should be invoked. 
        /// </summary>
        /// <remarks>
        /// Each invocation of the Action plays a new sound. Although the implementation is subject-to-change, the caller should 
        /// anticipate that this will create a new MediaElement that will be cleaned up when the sound completes or if the media 
        /// fails to play.
        /// </remarks>
        /// <param name="parameter"></param>
        protected override void Invoke(object parameter)
        {
            Popup popup;
            if ((this.Source != null) && (base.AssociatedObject != null))
            {
                popup = new Popup();
                MediaElement mediaElement = new MediaElement();
                popup.Child = mediaElement;
                mediaElement.Visibility = Visibility.Collapsed;
                this.SetMediaElementProperties(mediaElement);
                mediaElement.MediaEnded += (param0, param1) =>
                {
                    popup.Child = null;
                    popup.IsOpen = false;
                };
                mediaElement.MediaFailed += (param0, param1) =>
                {
                    popup.Child = null;
                    popup.IsOpen = false;
                };
                popup.IsOpen = true;
            }
        }

        /// <summary>
        /// When the action is invoked, this method is used to customize the dynamically created MediaElement.
        /// </summary>
        /// <remarks>
        /// This method may be useful for Action authors who wish to extend PlaySoundAction. If you want to control the 
        /// MediaElement Balance property, you could inherit from PlaySoundAction and override this method.
        /// </remarks>
        /// <param name="mediaElement"></param>
        protected virtual void SetMediaElementProperties(MediaElement mediaElement)
        {
            if (mediaElement != null)
            {
                mediaElement.Source = this.Source;
                mediaElement.Volume = this.Volume;
            }
        }
    }
}
