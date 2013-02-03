using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;

namespace XperiAndri.Expression.Interactivity.Core
{
    /// <summary>
    /// A trigger that is triggered by a specified event occurring on its source and fires after a delay when that event is fired.
    /// </summary>
    public class TimerTrigger : Windows.UI.Interactivity.EventTrigger
    {
        private object eventArgs;
        private int tickCount;
        private readonly ITickTimer timer;

        public static readonly DependencyProperty MillisecondsPerTickProperty = DependencyProperty.Register("MillisecondsPerTick", typeof(double), typeof(TimerTrigger), new PropertyMetadata(1000.0));

        /// <summary>
        /// Gets or sets the number of milliseconds to wait between ticks. This is a dependency property.
        /// </summary>
        public double MillisecondsPerTick
        {
            get
            {
                return (double)base.GetValue(MillisecondsPerTickProperty);
            }
            set
            {
                base.SetValue(MillisecondsPerTickProperty, value);
            }
        }

        public static readonly DependencyProperty TotalTicksProperty = DependencyProperty.Register("TotalTicks", typeof(int), typeof(TimerTrigger), new PropertyMetadata(-1));

        /// <summary>
        /// Gets or sets the total number of ticks to be fired before the trigger is finished.  This is a dependency property.
        /// </summary>
        public int TotalTicks
        {
            get
            {
                return (int)base.GetValue(TotalTicksProperty);
            }
            set
            {
                base.SetValue(TotalTicksProperty, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XperiAndri.Expression.Interactivity.Core.TimerTrigger"/> class.
        /// </summary>
        public TimerTrigger()
            : this(new DispatcherTickTimer())
        {
        }

        internal TimerTrigger(ITickTimer timer)
        {
            this.timer = timer;
        }

        protected override void OnDetaching()
        {
            this.StopTimer();
            base.OnDetaching();
        }

        protected override void OnEvent(object eventArgs)
        {
            this.StopTimer();
            this.eventArgs = eventArgs;
            this.tickCount = 0;
            this.StartTimer();
        }

        private void OnTimerTick(object sender, object e)
        {
            if ((this.TotalTicks > 0) && (++this.tickCount >= this.TotalTicks))
            {
                this.StopTimer();
            }
            base.InvokeActions(this.eventArgs);
        }

        internal void StartTimer()
        {
            if (this.timer != null)
            {
                this.timer.Interval = TimeSpan.FromMilliseconds(this.MillisecondsPerTick);
                this.timer.Tick += this.OnTimerTick;
                this.timer.Start();
            }
        }

        internal void StopTimer()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Tick -= this.OnTimerTick;
            }
        }

        // Nested Types
        internal class DispatcherTickTimer : ITickTimer
        {
            // Fields
            private readonly DispatcherTimer dispatcherTimer = new DispatcherTimer();

            // Events
            public event EventHandler<object> Tick
            {
                add
                {
                    this.dispatcherTimer.Tick += value;
                }
                remove
                {
                    this.dispatcherTimer.Tick -= value;
                }
            }

            // Methods
            public void Start()
            {
                this.dispatcherTimer.Start();
            }

            public void Stop()
            {
                this.dispatcherTimer.Stop();
            }

            // Properties
            public TimeSpan Interval
            {
                get
                {
                    return this.dispatcherTimer.Interval;
                }
                set
                {
                    this.dispatcherTimer.Interval = value;
                }
            }
        }
    }
}