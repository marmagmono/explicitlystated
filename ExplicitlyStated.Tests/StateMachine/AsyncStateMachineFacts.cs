using System;
using System.Collections.Generic;
using ExplicitlyStated.StateMachine;
using ExplicitlyStated.Tests.Data;
using ExplicitlyStated.Tests.StateMachineBase;

namespace ExplicitlyStated.Tests.StateMachine
{
    public class AsyncStateMachineFacts : AsyncStateMachineFactsBase
    {
        protected internal override IStateMachine<StateBase, EventBase> CreateSut()
        {
            var configuration = StateMachineConfigurationFactory.CreateAsync<StateBase, EventBase>();
            configuration.ConfigureState<InitialState>(1)
                .Transition<TestCommand>((s, e) => new TestState())
                .OnLeave(s => this.transitionsTester.Object.OnLeaveInitial(s));

            configuration.ConfigureState<TestState>(1)
                .OnEnter(s => this.transitionsTester.Object.OnEnterTest(s))
                .Transition<StartDetectionCommand>((s, e) => new DetectingState(false, new List<Device>()))
                .Transition<TestCommand>((s, e) => new TestState())
                .OnLeave(s => this.transitionsTester.Object.OnLeaveTest(s));

            configuration.ConfigureAsyncState<DetectingState>(6)
                .RunAsync(async s =>
                {
                    try
                    {
                        await this.detectionManager.Object.Detect();
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
                .Transition<TestCommand>((s, e) => new DetectingState(s.WaitingCancellation, s.DetectedDevices))
                .Transition<DeviceDetected>((s, e) =>
                {
                    s.DetectedDevices.Add(e.Device);
                    return new DetectingState(s.WaitingCancellation, s.DetectedDevices);
                })
                .Transition<DetectionCompletedSuccess>((s, e) => new DetectionSuccessState(s.DetectedDevices))
                .OnLeave(s => this.transitionsTester.Object.OnLeaveDetecting(s));

            configuration.ConfigureState<DetectionSuccessState>(2)
                .Transition<StartDetectionCommand>((s, e) => new DetectingState(false, new List<Device>()))
                .Transition<GoBackCommand>((s, e) => new InitialState());

            return StateMachineFactory.CreateAsync(new InitialState(), configuration);
        }
    }
}
