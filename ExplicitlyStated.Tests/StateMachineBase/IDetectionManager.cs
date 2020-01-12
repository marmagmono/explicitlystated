using System.Threading.Tasks;

namespace ExplicitlyStated.Tests.StateMachineBase
{
    public interface IDetectionManager
    {
        Task Detect();

        void Abort();
    }
}
