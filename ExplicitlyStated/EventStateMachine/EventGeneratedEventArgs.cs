using System;

namespace ExplicitlyStated.EventStateMachine
{
    public class EventGeneratedEventArgs<TGeneratedEvent> : EventArgs
    {
        public TGeneratedEvent Event { get; }

        public EventGeneratedEventArgs(TGeneratedEvent e)
        {
            Event = e;
        }
    }
}
