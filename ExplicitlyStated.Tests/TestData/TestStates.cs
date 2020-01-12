using System.Collections.Generic;

namespace ExplicitlyStated.Tests.Data
{
    public abstract class StateBase { }

    public class InitialState : StateBase { }

    public class TestState : StateBase { }

    public class DetectingState : StateBase
    {
        public bool WaitingCancellation { get; }

        public List<Device> DetectedDevices { get; }

        public DetectingState(bool waitingCancellation, List<Device> detectedDevices)
        {
            WaitingCancellation = waitingCancellation;
            DetectedDevices = detectedDevices;
        }
    }

    internal class DetectionSuccessState : StateBase
    {
        public List<Device> DetectedDevices { get; }

        public DetectionSuccessState(List<Device> detectedDevices)
        {
            DetectedDevices = detectedDevices;
        }
    }

    internal class DetectionFailedState : StateBase
    {

    }
}
