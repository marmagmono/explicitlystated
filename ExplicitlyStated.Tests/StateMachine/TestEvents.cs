using System;
using System.Collections.Generic;
using System.Text;

namespace ExplicitlyStated.Tests.StateMachine
{
    internal class EventBase { }

    internal class TestCommand : EventBase { }

    internal class StartDetectionCommand : EventBase { }

    internal class MediumUnavailableEvent : EventBase { }

    internal class GoBackCommand : EventBase { }

    internal class DeviceDetected : EventBase
    {
        public Device Device { get; }

        public DeviceDetected(Device device)
        {
            Device = device;
        }
    }

    internal class DetectionCompletedSuccess : EventBase
    {
    }

    internal class DetectionCancelled : EventBase { }

    internal class DetectionCompletedFailure : EventBase { }
}
