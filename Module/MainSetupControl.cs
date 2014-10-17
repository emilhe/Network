using System;
using System.Windows.Forms;

namespace Controls
{
    public partial class MainSetupControl : UserControl
    {

        public event EventHandler RunSimulation;
        public ModelParameters ModelParameters { get { return _mModelSetupControl.ParseModelParameters(); } }
        public Panel MainPanel { get { return mainPanel; } }

        private readonly ModelSetupControl _mModelSetupControl;

        public MainSetupControl()
        {
            InitializeComponent();

            _mModelSetupControl = new ModelSetupControl();
            _mModelSetupControl.RunSimulation += ModelSetupControlOnRunSimulation;
            sidePanel.Controls.Add(_mModelSetupControl);
        }

        private void ModelSetupControlOnRunSimulation(object sender, EventArgs eventArgs)
        {
            if (RunSimulation != null) RunSimulation(sender, eventArgs);
        }
    }
}
