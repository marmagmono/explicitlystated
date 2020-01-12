using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExplicitlyStated.EventStateMachine;
using ExplicitlyStated.StateMachine;
using ExplicitlyStated.Tests.Data;
using ExplicitlyStated.Tests.StateMachineBase;
using ExplicitlyStated.Tests.Utils;
using Moq;
using Xunit;

namespace ExplicitlyStated.Tests.EventStateMachine
{
    public class EventStateMachineFacts : AsyncStateMachineFactsBase
    {
        public abstract class GeneratedEventBase { }

        public class FirstEvent : GeneratedEventBase { }

        public class SecondEvent : GeneratedEventBase { }

        protected internal override IStateMachine<StateBase, EventBase> CreateSut()
        {
            var configuration = StateMachineConfigurationFactory.CreateEvent<
                StateBase,
                EventBase,
                GeneratedEventBase>();

            configuration.ConfigureState<InitialState>(1)
                .Transition<TestCommand>((s, e) => new TestState())
                .TransitionWithEvent<TransitionEventCommand>((s, e) =>
                {
                    if (e.GenerateEvent)
                    {
                        return (new TestState(), new FirstEvent());
                    }
                    else
                    {
                        return (new TestState(), null);
                    }
                })
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
                .TransitionWithEvent<TransitionEventCommand>((s, e) =>
                {
                    if (e.GenerateEvent)
                    {
                        return (new DetectingState(s.WaitingCancellation, s.DetectedDevices), new FirstEvent());
                    }
                    else
                    {
                        return (new DetectingState(s.WaitingCancellation, s.DetectedDevices), null);
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

            return StateMachineFactory.CreateEvent(new InitialState(), configuration);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task When_TransitionEventCommand_Than_Transitions_To_TestState_And_GeneratedEventIsRaisedWhenNecessary(bool generateEvent)
        {
            // Arrange
            var eventSut = CreateEventSut();
            this.sut = eventSut;

            var seq = new MockSequence();
            this.transitionsTester.InSequence(seq).Setup(m => m.OnLeaveInitial(It.IsAny<InitialState>()));
            this.transitionsTester.InSequence(seq).Setup(m => m.OnEnterTest(It.IsAny<TestState>()));

            var transition = NextStateChanged().Timeout(TimeoutDuration);

            EventGeneratedEventArgs<GeneratedEventBase> generatedEvent = null;
            eventSut.EventGenerated += (s, e) => { generatedEvent = e; };

            // Act
            this.sut.Process(new TransitionEventCommand(generateEvent));
            var e = await transition;

            // Assert
            Assert.IsType<TestState>(this.sut.CurrentState);
            Assert.IsType<TestState>(e.CurrentState);
            Assert.IsType<InitialState>(e.PreviousState);
            if (generateEvent)
            {
                Assert.IsType<FirstEvent>(generatedEvent.Event);
            }
            else
            {
                Assert.Null(generatedEvent);
            }

            this.transitionsTester.Verify(m => m.OnLeaveInitial(It.IsAny<InitialState>()), Times.Once);
            this.transitionsTester.Verify(m => m.OnEnterTest(It.IsAny<TestState>()), Times.Once);
        }

        [Fact]
        public async Task When_TransitionEventCommandGenerateEvent_AndNoEventHandlerRegistered_Than_DoesNotCrash()
        {
            // Arrange
            var eventSut = CreateEventSut();
            this.sut = eventSut;

            var seq = new MockSequence();
            this.transitionsTester.InSequence(seq).Setup(m => m.OnLeaveInitial(It.IsAny<InitialState>()));
            this.transitionsTester.InSequence(seq).Setup(m => m.OnEnterTest(It.IsAny<TestState>()));

            var transition = NextStateChanged().Timeout(TimeoutDuration);

            // Act
            this.sut.Process(new TransitionEventCommand(true));
            var e = await transition;

            // Assert
            // No crash
        }

        [Fact]
        public async Task When_InAsyncState_AndTransitionWithEvent_WithinTheSameState_Than_RunAsync_IsNotCalledAgain()
        {
            // Arrange
            this.sut = CreateSut();
            var detectTaskCompletionSource = new TaskCompletionSource<bool>();
            this.detectionManager.Setup(m => m.Detect()).Returns(detectTaskCompletionSource.Task);

            await DoStateChange(new TestCommand());
            await DoStateChange(new StartDetectionCommand());

            this.detectionManager.Reset();

            // Act
            await DoStateChange(new TransitionEventCommand(true));

            // Assert
            this.detectionManager.Verify(m => m.Detect(), Times.Never);
            this.transitionsTester.Verify(m => m.OnLeaveDetecting(It.IsAny<DetectingState>()), Times.Never);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task When_InAsyncState_AndTransitionWithEvent_Than_GeneratedEventIsRaisedWhenNecessary(bool generateEvent)
        {
            // Arrange
            var eventSut = CreateEventSut();
            this.sut = eventSut;
            var detectTaskCompletionSource = new TaskCompletionSource<bool>();
            this.detectionManager.Setup(m => m.Detect()).Returns(detectTaskCompletionSource.Task);

            await DoStateChange(new TestCommand());
            await DoStateChange(new StartDetectionCommand());

            this.detectionManager.Reset();

            EventGeneratedEventArgs<GeneratedEventBase> generatedEvent = null;
            eventSut.EventGenerated += (s, e) => { generatedEvent = e; };

            // Act
            await DoStateChange(new TransitionEventCommand(generateEvent));

            // Assert
            if (generateEvent)
            {
                Assert.IsType<FirstEvent>(generatedEvent.Event);
            }
            else
            {
                Assert.Null(generatedEvent);
            }
        }

        private IEventStateMachine<StateBase, EventBase, GeneratedEventBase> CreateEventSut() =>
            (IEventStateMachine<StateBase, EventBase, GeneratedEventBase>)CreateSut();
    }
}
