using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleImporter
{

    public enum TsSource : byte
    {
        ISET = 0,
        VE = 1,
        CALCULATED = 2,
        VE50PCT = 3,
    }

    public enum TsType : byte
    {
        [Description("Load")] Load = 0,
        [Description("Wind Generation")] Wind = 1,
        [Description("Solar Generation")] Solar = 2,
        [Description("Custom")] Custom = 3,
        [Description("Onshore Wind Generation")] OnshoreWind = 4,
        [Description("Offshore Wind Generation")] OffshoreWind = 5
    }

    public enum ExportScheme : byte
    {
        [Description("No Export")] None = 0,
        [Description("Unconstrained synchronized")] UnconstrainedSynchronized = 1,
        //[Description("Unconstrained localized")] UnconstrainedLocalized = 2,
        [Description("Constrained synchronized")] ConstrainedSynchronized = 3,
        [Description("Constrained localized")] ConstrainedLocalized = 4,
    }

}
