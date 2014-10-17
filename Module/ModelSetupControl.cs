using System;
using System.Windows.Forms;
using SimpleImporter;
using BusinessLogic.ExportStrategies;
using BusinessLogic.ExportStrategies.DistributionStrategies;
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
            foreach (ExportStrategy value in Enum.GetValues(typeof(ExportStrategy)))
                cbExport.Items.Add(value.GetDescription());
        }

        public ModelParameters ParseModelParameters()
        {
            return new ModelParameters
            {
                Years = (int)numYears.Value,
                Source = (TsSource)cbSource.SelectedIndex,
                ExportStrategy = MapExport(cbExport.SelectedItem)
            };
        }

        private IExportStrategy MapExport(object selectedItem)
        {
            if (selectedItem.Equals(ExportStrategy.None.GetDescription())) 
                return new NoExportStrategy();
            if (selectedItem.Equals(ExportStrategy.Selfish.GetDescription()))
                return new SelfishExportStrategy(MapDistribution());
            if (selectedItem.Equals(ExportStrategy.Cooperative.GetDescription()))
                return new CooperativeExportStrategy(MapDistribution());
            throw new ArgumentException("Unable to map export strategy.");
        }

        private IDistributionStrategy MapDistribution()
        {
            return cbFlow.Checked ? (IDistributionStrategy) new MinimalFlowStrategy(null, null) : new SkipFlowStrategy();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            if(RunSimulation != null) RunSimulation(this, e);
        }
    }

    public class ModelParameters
    {
        public IExportStrategy ExportStrategy { get; set; }
        public TsSource Source { get; set; }
        public int Years { get; set; }
    }
}
