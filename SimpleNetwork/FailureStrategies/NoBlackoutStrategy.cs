using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;

namespace BusinessLogic.FailureStrategies
{
    public class NoBlackoutStrategy : IFailureStrategy
    {

        public bool Failure { get; private set; }

        public void Record(bool failure)
        {
            if (failure) Failure = true;
        }

        public void Reset()
        {
            Failure = false;
        }
    }
}
