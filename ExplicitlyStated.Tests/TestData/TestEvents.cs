namespace ExplicitlyStated.Tests.Data
{
    public class EventBase { }

    internal class TestCommand : EventBase { }

    internal class TransitionEventCommand : EventBase
    {
        public bool GenerateEvent { get; }

        public TransitionEventCommand(bool generateEvent)
        {
            GenerateEvent = generateEvent;
        }
    }

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
