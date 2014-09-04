namespace Controls
{
    partial class ModelSetupControl
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
            this.label4 = new System.Windows.Forms.Label();
            this.numYears = new System.Windows.Forms.NumericUpDown();
            this.cbExport = new System.Windows.Forms.ComboBox();
            this.lblExport = new System.Windows.Forms.Label();
            this.lblTime = new System.Windows.Forms.Label();
            this.cbSource = new System.Windows.Forms.ComboBox();
            this.lblData = new System.Windows.Forms.Label();
            this.cbFlow = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numYears)).BeginInit();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-3, 80);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 15;
            // 
            // numYears
            // 
            this.numYears.Location = new System.Drawing.Point(0, 122);
            this.numYears.Maximum = new decimal(new int[] {
            32,
            0,
            0,
            0});
            this.numYears.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numYears.Name = "numYears";
            this.numYears.ReadOnly = true;
            this.numYears.Size = new System.Drawing.Size(120, 20);
            this.numYears.TabIndex = 13;
            this.numYears.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // cbExport
            // 
            this.cbExport.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbExport.FormattingEnabled = true;
            this.cbExport.Location = new System.Drawing.Point(0, 56);
            this.cbExport.Name = "cbExport";
            this.cbExport.Size = new System.Drawing.Size(121, 21);
            this.cbExport.TabIndex = 12;
            // 
            // lblExport
            // 
            this.lblExport.AutoSize = true;
            this.lblExport.Location = new System.Drawing.Point(-3, 40);
            this.lblExport.Name = "lblExport";
            this.lblExport.Size = new System.Drawing.Size(79, 13);
            this.lblExport.TabIndex = 11;
            this.lblExport.Text = "Export Strategy";
            // 
            // lblTime
            // 
            this.lblTime.AutoSize = true;
            this.lblTime.Location = new System.Drawing.Point(-3, 106);
            this.lblTime.Name = "lblTime";
            this.lblTime.Size = new System.Drawing.Size(64, 13);
            this.lblTime.TabIndex = 10;
            this.lblTime.Text = "Time [years]";
            // 
            // cbSource
            // 
            this.cbSource.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSource.FormattingEnabled = true;
            this.cbSource.Location = new System.Drawing.Point(0, 16);
            this.cbSource.Name = "cbSource";
            this.cbSource.Size = new System.Drawing.Size(121, 21);
            this.cbSource.TabIndex = 9;
            // 
            // lblData
            // 
            this.lblData.AutoSize = true;
            this.lblData.Location = new System.Drawing.Point(-3, 0);
            this.lblData.Name = "lblData";
            this.lblData.Size = new System.Drawing.Size(67, 13);
            this.lblData.TabIndex = 8;
            this.lblData.Text = "Data Source";
            // 
            // cbFlow
            // 
            this.cbFlow.AutoSize = true;
            this.cbFlow.Location = new System.Drawing.Point(3, 83);
            this.cbFlow.Name = "cbFlow";
            this.cbFlow.Size = new System.Drawing.Size(91, 17);
            this.cbFlow.TabIndex = 17;
            this.cbFlow.Text = "Minimize Flow";
            this.cbFlow.UseVisualStyleBackColor = true;
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(0, 148);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(121, 23);
            this.btnRun.TabIndex = 18;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // ModelSetupControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.cbFlow);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.numYears);
            this.Controls.Add(this.cbExport);
            this.Controls.Add(this.lblExport);
            this.Controls.Add(this.lblTime);
            this.Controls.Add(this.cbSource);
            this.Controls.Add(this.lblData);
            this.Name = "ModelSetupControl";
            this.Size = new System.Drawing.Size(121, 172);
            ((System.ComponentModel.ISupportInitialize)(this.numYears)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numYears;
        private System.Windows.Forms.ComboBox cbExport;
        private System.Windows.Forms.Label lblExport;
        private System.Windows.Forms.Label lblTime;
        private System.Windows.Forms.ComboBox cbSource;
        private System.Windows.Forms.Label lblData;
        private System.Windows.Forms.CheckBox cbFlow;
        private System.Windows.Forms.Button btnRun;
    }
}
