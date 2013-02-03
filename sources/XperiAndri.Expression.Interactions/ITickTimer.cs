using System;
using System.Linq;

namespace XperiAndri.Expression.Interactivity
{
    internal interface ITickTimer
    {
        // Events
        event EventHandler<object> Tick;

        // Methods
        void Start();

        void Stop();

        // Properties
        TimeSpan Interval { get; set; }
    }
}