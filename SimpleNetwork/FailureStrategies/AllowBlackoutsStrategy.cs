using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;

namespace BusinessLogic.FailureStrategies
{
    public class AllowBlackoutsStrategy : IFailureStrategy
    {

        public bool Failure { get { return _mBlackouts > _mBlackoutsAllowed; } }

        public void Record(BalanceResult result)
        {
            if (result.Failure) _mBlackouts++;
        }

        public void Reset()
        {
            _mBlackouts = 0;
        }

        private readonly int _mBlackoutsAllowed;    
        private int _mBlackouts;

        public AllowBlackoutsStrategy(int blackoutsAllowed)
        {
            _mBlackoutsAllowed = blackoutsAllowed;
        }

    }
}
