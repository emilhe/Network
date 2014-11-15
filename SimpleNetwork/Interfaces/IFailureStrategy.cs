using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;

namespace BusinessLogic.Interfaces
{
    public interface IFailureStrategy
    {

        bool Failure { get; }
        void Record(BalanceResult result);
        void Reset();

    }
}
