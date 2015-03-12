using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SimpleImporter;
using BusinessLogic.ExportStrategies;
using BusinessLogic.Interfaces;
using Utils;

namespace Controls
{
    public partial class ModelSetupControl : UserControl
    {

        public event EventHandler RunSimulation;

        public ModelSetupControl()
        {
            InitializeComponent();

            if (System.Diagnostics.Process.GetCurrentProcess().ProcessName.Equals("devenv")) return;
            
            // Bind sources.
            cbSource.Items.Clear();
            foreach (TsSource value in Enum.GetValues(typeof(TsSource)))
                cbSource.Items.Add(value.GetDescription());
            // Bind strategies.
            cbExport.Items.Clear();
            foreach (ExportScheme value in Enum.GetValues(typeof(ExportScheme)))
                cbExport.Items.Add(value.GetDescription());
        }

        public ModelParameters ParseModelParameters()
        {
            return new ModelParameters
            {
                Years = (int)numYears.Value,
                Source = (TsSource)cbSource.SelectedIndex,
                ExportScheme = MapExport(cbExport.SelectedItem)
            };
        }

        private IExportScheme MapExport(object selectedItem)
        {
            if (selectedItem.Equals(ExportScheme.None.GetDescription())) 
                return new NoExportScheme();
            //if (selectedItem.Equals(ExportScheme.Selfish.GetDescription()))
            //    return new SelfishExportStrategy(MapDistribution());
            //if (selectedItem.Equals(ExportScheme.Cooperative.GetDescription()))
            //    return new CooperativeExportStrategy(MapDistribution());
            throw new ArgumentException("Unable to map export scheme.");
        }

        //private IDistributionStrategy MapDistribution()
        //{
        //    return cbFlow.Checked ? (IDistributionStrategy) new MinimalFlowStrategy(new List<INode>(), null) : new SkipFlowStrategy();
        //}

        private void btnRun_Click(object sender, EventArgs e)
        {
            if(RunSimulation != null) RunSimulation(this, e);
        }
    }

    public class ModelParameters
    {
        public IExportScheme ExportScheme { get; set; }
        public TsSource Source { get; set; }
        public int Years { get; set; }
    }
}
