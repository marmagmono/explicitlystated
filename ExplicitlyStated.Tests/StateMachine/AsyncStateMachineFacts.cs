using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExplicitlyStated.Tests.StateMachine
{
    public class AsyncStateMachineFacts
    {
        private class DetectionManager
        {
            public Task Detect() => Task.CompletedTask;

            public void Abort() { }
        }

        private class Device { }

        private abstract class StateBase { }

        private class InitialState : StateBase { }

        private class DetectingState : StateBase
        {
            public bool WaitingCancellation { get; }

            public List<Device> DetectedDevices { get; }

            public DetectingState(bool waitingCancellation, List<Device> detectedDevices)
            {
                WaitingCancellation = waitingCancellation;
                DetectedDevices = detectedDevices;
            }
        }

        private class DetectionSuccessState : StateBase
        {
            public List<Device> DetectedDevices { get; }

            public DetectionSuccessState(List<Device> detectedDevices)
            {
                DetectedDevices = detectedDevices;
            }
        }

        private class DetectionFailedState : StateBase
        {

        }

        private class EventBase { }

        private class StartDetectionCommand : EventBase { }

        private class MediumUnavailableEvent : EventBase { }

        private class GoBackCommand : EventBase { }

        private class DeviceDetected : EventBase
        {
            public Device Device { get; }

            public DeviceDetected(Device device)
            {
                Device = device;
            }
        }

        private class DetectionCompletedSuccess : EventBase
        {
        }

        private class DetectionCancelled : EventBase { }

        private class DetectionCompletedFailure : EventBase { }

        public AsyncStateMachineFacts()
        {
            var dm = new DetectionManager();

            var configuration = StateMachineConfigurationFactory.CreateAsync<StateBase, EventBase>();
            configuration.ConfigureState<InitialState>(1)
                .Transition<StartDetectionCommand>((s, e) => new DetectingState(false, new List<Device>()));

            configuration.ConfigureAsyncState<DetectingState>(6)
                .RunAsync(async s =>
                {
                    try
                    {
                        await dm.Detect();
                        return new DetectionCompletedSuccess();
                    }
                    catch (OperationCanceledException)
                    {
                        return new DetectionCancelled();
                    }
                    catch (Exception)
                    {
                        return new DetectionCompletedFailure();
                    }
                })
                .Transition<GoBackCommand>((s, e) =>
                {
                    if (!s.WaitingCancellation)
                    {
                        dm.Abort();
                    }

                    return new DetectingState(true, s.DetectedDevices);
                })
                .Transition<MediumUnavailableEvent>((s, e) =>
                {
                    if (!s.WaitingCancellation)
                    {
                        dm.Abort();
                    }

                    return new DetectingState(true, s.DetectedDevices);
                })
                .Transition<DeviceDetected>((s, e) =>
                {
                    s.DetectedDevices.Add(e.Device);
                    return new DetectingState(s.WaitingCancellation, s.DetectedDevices);
                })
                .Transition<DetectionCompletedSuccess>((s, e) => new DetectionSuccessState(s.DetectedDevices))
                .Transition<DetectionCancelled>((s, e) => new InitialState())
                .Transition<DetectionCompletedFailure>((s, e) => new DetectionFailedState());

            configuration.ConfigureState<DetectionSuccessState>(2)
                .Transition<StartDetectionCommand>((s, e) => new DetectingState(false, new List<Device>()))
                .Transition<GoBackCommand>((s, e) => new InitialState());

            configuration.ConfigureState<DetectionFailedState>(2)
                .Transition<StartDetectionCommand>((s, e) => new DetectingState(false, new List<Device>()))
                .Transition<GoBackCommand>((s, e) => new InitialState());

            var machine = StateMachineFactory.CreateAsync(new InitialState(), configuration);
        }
    }
}
