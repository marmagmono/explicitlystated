using ExplicitlyStated.StateMachine;
using Moq;
using Xunit;

namespace ExplicitlyStated.Tests.StateMachine
{
    public class SimpleStateMachineFacts
    {
        private ISimpleStateMachine<StateBase, EventBase> sut;

        private Mock<ITestTransitions> transitionsTester;

        public interface ITestTransitions
        {
            void OnLeaveInitial(InitialState s);

            void OnEnterTest(TestState s);

            void OnLeaveTest(TestState s);
        }

        public SimpleStateMachineFacts()
        {
            this.transitionsTester = new Mock<ITestTransitions>();

            var configuration = StateMachineConfigurationFactory.Create<StateBase, EventBase>();
            configuration.ConfigureState<InitialState>(1)
                .Transition<TestCommand>((s, e) => new TestState())
                .OnLeave(s => this.transitionsTester.Object.OnLeaveInitial(s));

            configuration.ConfigureState<TestState>(1)
                .OnEnter(s => this.transitionsTester.Object.OnEnterTest(s))
                .Transition<TestCommand>((s, e) => new TestState())
                .OnLeave(s => this.transitionsTester.Object.OnLeaveTest(s));

            sut = StateMachineFactory.CreateSimple(new InitialState(), configuration);
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
        public void When_ProcessTestCommand_Transitions_To_TestState()
        {
            // Arrange
            var seq = new MockSequence();
            this.transitionsTester.InSequence(seq).Setup(m => m.OnLeaveInitial(It.IsAny<InitialState>()));
            this.transitionsTester.InSequence(seq).Setup(m => m.OnEnterTest(It.IsAny<TestState>()));

            // Act
            StateChangedEventArgs<StateBase> ev = null;
            this.sut.StateChanged += (s, e) => ev = e;
            this.sut.Process(new TestCommand());

            // Assert
            Assert.IsType<TestState>(this.sut.CurrentState);
            Assert.IsType<TestState>(ev.CurrentState);
            Assert.IsType<InitialState>(ev.PreviousState);

            this.transitionsTester.Verify(m => m.OnLeaveInitial(It.IsAny<InitialState>()), Times.Once);
            this.transitionsTester.Verify(m => m.OnEnterTest(It.IsAny<TestState>()), Times.Once);
        }

        [Fact]
        public void When_TransitionTestCommand_Transitions_To_TestState()
        {
            // Arrange
            var seq = new MockSequence();
            this.transitionsTester.InSequence(seq).Setup(m => m.OnLeaveInitial(It.IsAny<InitialState>()));
            this.transitionsTester.InSequence(seq).Setup(m => m.OnEnterTest(It.IsAny<TestState>()));

            // Act
            bool transitioned = this.sut.Transition(new TestCommand(), out var newState);

            // Assert
            Assert.True(transitioned);
            Assert.IsType<TestState>(newState);
            Assert.IsType<TestState>(this.sut.CurrentState);
            this.transitionsTester.Verify(m => m.OnLeaveInitial(It.IsAny<InitialState>()), Times.Once);
            this.transitionsTester.Verify(m => m.OnEnterTest(It.IsAny<TestState>()), Times.Once);
        }

        [Fact]
        public void When_TransitionDetectionCompletedSuccess_Than_Does_NotTransition()
        {
            // Arrange
            var seq = new MockSequence();
            this.transitionsTester.InSequence(seq).Setup(m => m.OnLeaveInitial(It.IsAny<InitialState>()));
            this.transitionsTester.InSequence(seq).Setup(m => m.OnEnterTest(It.IsAny<TestState>()));

            // Act
            bool transitioned = this.sut.Transition(new DetectionCompletedSuccess(), out var newState);

            // Assert
            Assert.False(transitioned);
            Assert.IsType<InitialState>(this.sut.CurrentState);
            this.transitionsTester.Verify(m => m.OnLeaveInitial(It.IsAny<InitialState>()), Times.Never);
        }

        [Fact]
        public void When_Transition_To_New_StateOfTheSameTypeButNotEqual_Than_OnEnterAndOnLeaveAreNotCalled_ButStateChangedIs()
        {
            // Arrange
            var seq = new MockSequence();

            this.sut.Process(new TestCommand());
            Assert.IsType<TestState>(this.sut.CurrentState);
            this.transitionsTester.Reset();

            // Act
            bool transitioned = this.sut.Transition(new TestCommand(), out var newState);

            // Assert
            Assert.True(transitioned);
            Assert.IsType<TestState>(newState);

            this.transitionsTester.Verify(m => m.OnLeaveTest(It.IsAny<TestState>()), Times.Never);
            this.transitionsTester.Verify(m => m.OnEnterTest(It.IsAny<TestState>()), Times.Never);
        }
    }
}
