using BusinessLogic.ExportStrategies;

namespace BusinessLogic.Interfaces
{
    public interface IFailureStrategy
    {

        bool Failure { get; }
        void Record(bool failure);
        void Reset();

    }
}
