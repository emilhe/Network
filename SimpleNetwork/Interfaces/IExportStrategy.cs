using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SimpleNetwork
{
    public interface IExportStrategy
    {

        void Respond(int tick);

        List<Node> Nodes { get; }
        double Mismatch { get; }
        double Curtailment { get; }
        bool Failure { get; }

    }
}
