using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace BusinessLogic.Cost.Transmission
{
    
    public interface ITransmissionCostModel
    {

        double Eval(Dictionary<string, double> transmissionMap);

    }

}
