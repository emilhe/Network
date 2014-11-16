using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface ITickListener
    {

        /// <summary>
        /// Signal that the tick has changed; the measureable should sample!
        /// </summary>
        void TickChanged(int tick);

    }
}
