using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExplicitlyStated.StateMachine;
using ExplicitlyStated.Tests.Utils;
using Moq;
using Xunit;

namespace ExplicitlyStated.Tests.StateMachine
{
    public class AsyncStateMachineFacts
    {
        private static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(1000);

        private IStateMachine<StateBase, EventBase> sut;

        private Mock<IDetectionManager> detectionManager;
        private Mock<ITestTransitions> transitionsTester;

        public interface ITestTransitions
        {
            void OnLeaveInitial(InitialState s);

            void OnEnterTest(TestState s);

            void OnLeaveTest(TestState s);

            void OnLeaveDetecting(DetectingState s);
        }

        public interface IDetectionManager
        {
            Task Detect();

            void Abort();
        }

        public AsyncStateMachineFacts()
        {
            this.detectionManager = new Mock<IDetectionManager>(MockBehavior.Strict);
            this.transitionsTester = new Mock<ITestTransitions>();

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
                .Transition<GoBackCommand>((s, e) =>
                {
                    if (!s.WaitingCancellation)
                    {
                        this.detectionManager.Object.Abort();
                    }

                    return new DetectingState(true, s.DetectedDevices);
                })
                .Transition<MediumUnavailableEvent>((s, e) =>
                {
                    if (!s.WaitingCancellation)
                    {
                        this.detectionManager.Object.Abort();
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
                .Transition<DetectionCompletedFailure>((s, e) => new DetectionFailedState())
                .OnLeave(s => this.transitionsTester.Object.OnLeaveDetecting(s));

            configuration.ConfigureState<DetectionSuccessState>(2)
                .Transition<StartDetectionCommand>((s, e) => new DetectingState(false, new List<Device>()))
                .Transition<GoBackCommand>((s, e) => new InitialState());

            configuration.ConfigureState<DetectionFailedState>(2)
                .Transition<StartDetectionCommand>((s, e) => new DetectingState(false, new List<Device>()))
                .Transition<GoBackCommand>((s, e) => new InitialState());

            sut = StateMachineFactory.CreateAsync(new InitialState(), configuration);
        }

        [Fact]
        public void FirstState_IsInitialState()
        {
            // Arrange

            // Act

            // Assert
            Assert.IsType<InitialState>(this.sut.CurrentState);
        }

        [Fact]
        public async Task When_TestCommand_Transitions_To_TestState()
        {
            // Arrange
            var seq = new MockSequence();
            this.transitionsTester.InSequence(seq).Setup(m => m.OnLeaveInitial(It.IsAny<InitialState>()));
            this.transitionsTester.InSequence(seq).Setup(m => m.OnEnterTest(It.IsAny<TestState>()));

            var transition = NextStateChanged().Timeout(TimeoutDuration);

            // Act
            this.sut.Process(new TestCommand());
            var e = await transition;

            // Assert
            Assert.IsType<TestState>(this.sut.CurrentState);
            Assert.IsType<TestState>(e.CurrentState);
            Assert.IsType<InitialState>(e.PreviousState);

            this.transitionsTester.Verify(m => m.OnLeaveInitial(It.IsAny<InitialState>()), Times.Once);
            this.transitionsTester.Verify(m => m.OnEnterTest(It.IsAny<TestState>()), Times.Once);
        }

        [Fact]
        public async Task When_Transition_To_New_StateOfTheSameTypeButNotEqual_Than_OnEnterAndOnLeaveAreNotCalled_ButStateChangedIs()
        {
            // Arrange
            var seq = new MockSequence();

            var transition = NextStateChanged().Timeout(TimeoutDuration);
            this.sut.Process(new TestCommand());
            await transition;
            Assert.IsType<TestState>(this.sut.CurrentState);
            this.transitionsTester.Reset();

            transition = NextStateChanged().Timeout(TimeoutDuration);
            // Act
            this.sut.Process(new TestCommand());
            await transition;

            // Assert

            this.transitionsTester.Verify(m => m.OnLeaveTest(It.IsAny<TestState>()), Times.Never);
            this.transitionsTester.Verify(m => m.OnEnterTest(It.IsAny<TestState>()), Times.Never);
        }

        [Fact]
        public async Task When_Transition_To_AsyncState_Than_RunAsyncIsCalled()
        {
            // Arrange
            var detectTaskCompletionSource = new TaskCompletionSource<bool>();
            this.detectionManager.Setup(m => m.Detect()).Returns(detectTaskCompletionSource.Task);

            await DoStateChange(new TestCommand());

            // Act
            await DoStateChange(new StartDetectionCommand());

            // Assert
            this.detectionManager.Verify(m => m.Detect(), Times.Once);
        }

        [Fact]
        public async Task When_InAsyncState_AndTransitionWithinTheSameState_Than_RunAsync_IsNotCalledAgain()
        {
            // Arrange
            var detectTaskCompletionSource = new TaskCompletionSource<bool>();
            this.detectionManager.Setup(m => m.Detect()).Returns(detectTaskCompletionSource.Task);

            await DoStateChange(new TestCommand());
            await DoStateChange(new StartDetectionCommand());

            this.detectionManager.Reset();

            // Act
            await DoStateChange(new DeviceDetected(new Device()));

            // Assert
            this.detectionManager.Verify(m => m.Detect(), Times.Never);
            this.transitionsTester.Verify(m => m.OnLeaveDetecting(It.IsAny<DetectingState>()), Times.Never);
        }

        [Fact]
        public async Task When_InAsyncState_AndAsyncOperationCompletes_Than_TransitionsToStateAssociatedWithActionResultEventIsCalled()
        {
            // Arrange
            var detectTaskCompletionSource = new TaskCompletionSource<bool>();
            this.detectionManager.Setup(m => m.Detect()).Returns(detectTaskCompletionSource.Task);

            await DoStateChange(new TestCommand());
            await DoStateChange(new StartDetectionCommand());

            this.detectionManager.Reset();

            // Act
            var transition = NextStateChanged().Timeout(TimeoutDuration);
            detectTaskCompletionSource.SetResult(true);

            await transition;

            // Assert
            Assert.IsType<DetectionSuccessState>(this.sut.CurrentState);
            this.transitionsTester.Verify(m => m.OnLeaveDetecting(It.IsAny<DetectingState>()), Times.Once);
        }

        private async Task DoStateChange(EventBase @event)
        {
            var transition = NextStateChanged().Timeout(TimeoutDuration);
            this.sut.Process(@event);
            await transition;
        }

        private Task<StateChangedEventArgs<StateBase>> NextStateChanged() =>
            TaskEventsUtils.FromEvent<StateChangedEventArgs<StateBase>>(
                subscribe: h => this.sut.StateChanged += h,
                unsubscribe: h => this.sut.StateChanged -= h);
    }
}
