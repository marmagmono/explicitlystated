using ExplicitlyStated.Tests.Data;

namespace ExplicitlyStated.Tests.StateMachineBase
{
    public interface ITestTransitions
    {
        void OnLeaveInitial(InitialState s);

        void OnEnterTest(TestState s);

        void OnLeaveTest(TestState s);

        void OnLeaveDetecting(DetectingState s);
    }
}
