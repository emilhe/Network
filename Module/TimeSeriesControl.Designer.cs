namespace Controls
{
    partial class TimeSeriesControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.seriesListView = new System.Windows.Forms.ListView();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.cbxView = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // seriesListView
            // 
            this.seriesListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.seriesListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.seriesListView.Location = new System.Drawing.Point(835, 0);
            this.seriesListView.Name = "seriesListView";
            this.seriesListView.Size = new System.Drawing.Size(283, 292);
            this.seriesListView.TabIndex = 2;
            this.seriesListView.UseCompatibleStateImageBehavior = false;
            this.seriesListView.View = System.Windows.Forms.View.Details;
            // 
            // mainPanel
            // 
            this.mainPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(829, 319);
            this.mainPanel.TabIndex = 3;
            // 
            // cbxView
            // 
            this.cbxView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbxView.FormattingEnabled = true;
            this.cbxView.Location = new System.Drawing.Point(835, 298);
            this.cbxView.Name = "cbxView";
            this.cbxView.Size = new System.Drawing.Size(283, 21);
            this.cbxView.TabIndex = 4;
            this.cbxView.SelectedIndexChanged += new System.EventHandler(this.cbxView_SelectedIndexChanged);
            // 
            // TimeSeriesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbxView);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.seriesListView);
            this.Name = "TimeSeriesControl";
            this.Size = new System.Drawing.Size(1118, 319);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView seriesListView;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.ComboBox cbxView;

    }
}
