using System;
using System.Threading.Tasks;
using ExplicitlyStated.StateMachine;
using ExplicitlyStated.Tests.Data;
using ExplicitlyStated.Tests.Utils;
using Moq;
using Xunit;

namespace ExplicitlyStated.Tests.StateMachineBase
{
    /// <summary>
    /// Basic set of facts for every async state machine.
    /// </summary>
    public abstract class AsyncStateMachineFactsBase
    {
        protected static readonly TimeSpan TimeoutDuration = TimeSpan.FromSeconds(1000);

        protected Mock<IDetectionManager> detectionManager;
        protected Mock<ITestTransitions> transitionsTester;
        protected IStateMachine<StateBase, EventBase> sut;

        protected AsyncStateMachineFactsBase()
        {
            this.detectionManager = new Mock<IDetectionManager>(MockBehavior.Strict);
            this.transitionsTester = new Mock<ITestTransitions>();
        }

        protected internal abstract IStateMachine<StateBase, EventBase> CreateSut();

        [Fact]
        public void FirstState_IsInitialState()
        {
            // Arrange
            this.sut = CreateSut();

            // Act

            // Assert
            Assert.IsType<InitialState>(this.sut.CurrentState);
        }

        [Fact]
        public async Task When_TestCommand_Transitions_To_TestState()
        {
            // Arrange
            this.sut = CreateSut();
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
            this.sut = CreateSut();
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
            this.sut = CreateSut();
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
            this.sut = CreateSut();
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
            this.sut = CreateSut();
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

        protected async Task DoStateChange(EventBase @event)
        {
            var transition = NextStateChanged().Timeout(TimeoutDuration);
            this.sut.Process(@event);
            await transition;
        }

        protected Task<StateChangedEventArgs<StateBase>> NextStateChanged() =>
            TaskEventsUtils.FromEvent<StateChangedEventArgs<StateBase>>(
                subscribe: h => this.sut.StateChanged += h,
                unsubscribe: h => this.sut.StateChanged -= h);
    }
}
