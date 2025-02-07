namespace AccessGdb
{
    partial class SecondForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecondForm));
            this.startDebuggingButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.stopDebuggingButton = new System.Windows.Forms.Button();
            this.inputTextBox = new System.Windows.Forms.TextBox();
            this.setBreakpointButton = new System.Windows.Forms.Button();
            this.customCommandButton = new System.Windows.Forms.Button();
            this.addPathButton = new System.Windows.Forms.Button();
            this.addSourceButton = new System.Windows.Forms.Button();
            this.asyncNotifyTextBox = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.logOutputTextBox = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.asyncExecTextBox = new System.Windows.Forms.TextBox();
            this.loadParameterListButton = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // startDebuggingButton
            // 
            this.startDebuggingButton.Location = new System.Drawing.Point(18, 12);
            this.startDebuggingButton.Name = "startDebuggingButton";
            this.startDebuggingButton.Size = new System.Drawing.Size(112, 23);
            this.startDebuggingButton.TabIndex = 0;
            this.startDebuggingButton.Text = "Start debugging";
            this.startDebuggingButton.UseVisualStyleBackColor = true;
            this.startDebuggingButton.Click += new System.EventHandler(this.StartDebuggingButtonClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.outputTextBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 108);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(305, 526);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Console output";
            // 
            // outputTextBox
            // 
            this.outputTextBox.Location = new System.Drawing.Point(6, 19);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(290, 501);
            this.outputTextBox.TabIndex = 0;
            // 
            // clearButton
            // 
            this.clearButton.Location = new System.Drawing.Point(811, 653);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(72, 23);
            this.clearButton.TabIndex = 9;
            this.clearButton.Text = "Clear form";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.ClearButtonClick);
            // 
            // stopDebuggingButton
            // 
            this.stopDebuggingButton.Location = new System.Drawing.Point(18, 41);
            this.stopDebuggingButton.Name = "stopDebuggingButton";
            this.stopDebuggingButton.Size = new System.Drawing.Size(112, 23);
            this.stopDebuggingButton.TabIndex = 6;
            this.stopDebuggingButton.Text = "Stop debugging";
            this.stopDebuggingButton.UseVisualStyleBackColor = true;
            this.stopDebuggingButton.Click += new System.EventHandler(this.StopDebuggingButtonClick);
            // 
            // inputTextBox
            // 
            this.inputTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.inputTextBox.Location = new System.Drawing.Point(167, 15);
            this.inputTextBox.Name = "inputTextBox";
            this.inputTextBox.Size = new System.Drawing.Size(292, 20);
            this.inputTextBox.TabIndex = 0;
            // 
            // setBreakpointButton
            // 
            this.setBreakpointButton.Location = new System.Drawing.Point(590, 15);
            this.setBreakpointButton.Name = "setBreakpointButton";
            this.setBreakpointButton.Size = new System.Drawing.Size(112, 23);
            this.setBreakpointButton.TabIndex = 8;
            this.setBreakpointButton.Text = "Set breakpoint";
            this.setBreakpointButton.UseVisualStyleBackColor = true;
            this.setBreakpointButton.Click += new System.EventHandler(this.SetBreakpointButtonClick);
            // 
            // customCommandButton
            // 
            this.customCommandButton.Location = new System.Drawing.Point(472, 15);
            this.customCommandButton.Name = "customCommandButton";
            this.customCommandButton.Size = new System.Drawing.Size(112, 23);
            this.customCommandButton.TabIndex = 1;
            this.customCommandButton.Text = "Send command";
            this.customCommandButton.UseVisualStyleBackColor = true;
            this.customCommandButton.Click += new System.EventHandler(this.CustomCommandButtonClick);
            // 
            // addPathButton
            // 
            this.addPathButton.Location = new System.Drawing.Point(590, 45);
            this.addPathButton.Name = "addPathButton";
            this.addPathButton.Size = new System.Drawing.Size(112, 23);
            this.addPathButton.TabIndex = 9;
            this.addPathButton.Text = "Add path";
            this.addPathButton.UseVisualStyleBackColor = true;
            this.addPathButton.Click += new System.EventHandler(this.AddPathButtonClick);
            // 
            // addSourceButton
            // 
            this.addSourceButton.Location = new System.Drawing.Point(472, 44);
            this.addSourceButton.Name = "addSourceButton";
            this.addSourceButton.Size = new System.Drawing.Size(112, 23);
            this.addSourceButton.TabIndex = 9;
            this.addSourceButton.Text = "Add source";
            this.addSourceButton.UseVisualStyleBackColor = true;
            this.addSourceButton.Click += new System.EventHandler(this.AddSourceButtonClick);
            // 
            // asyncNotifyTextBox
            // 
            this.asyncNotifyTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.asyncNotifyTextBox.Location = new System.Drawing.Point(6, 19);
            this.asyncNotifyTextBox.Multiline = true;
            this.asyncNotifyTextBox.Name = "asyncNotifyTextBox";
            this.asyncNotifyTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.asyncNotifyTextBox.Size = new System.Drawing.Size(255, 225);
            this.asyncNotifyTextBox.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.asyncNotifyTextBox);
            this.groupBox2.Location = new System.Drawing.Point(323, 108);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(276, 256);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Async notify output";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.logOutputTextBox);
            this.groupBox3.Location = new System.Drawing.Point(605, 108);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(276, 256);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Log output";
            // 
            // logOutputTextBox
            // 
            this.logOutputTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.logOutputTextBox.Location = new System.Drawing.Point(6, 19);
            this.logOutputTextBox.Multiline = true;
            this.logOutputTextBox.Name = "logOutputTextBox";
            this.logOutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logOutputTextBox.Size = new System.Drawing.Size(255, 225);
            this.logOutputTextBox.TabIndex = 0;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.asyncExecTextBox);
            this.groupBox4.Location = new System.Drawing.Point(329, 370);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(552, 277);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Async execute output";
            // 
            // asyncExecTextBox
            // 
            this.asyncExecTextBox.BackColor = System.Drawing.SystemColors.Control;
            this.asyncExecTextBox.Location = new System.Drawing.Point(6, 19);
            this.asyncExecTextBox.Multiline = true;
            this.asyncExecTextBox.Name = "asyncExecTextBox";
            this.asyncExecTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.asyncExecTextBox.Size = new System.Drawing.Size(531, 245);
            this.asyncExecTextBox.TabIndex = 0;
            // 
            // loadParameterListButton
            // 
            this.loadParameterListButton.Location = new System.Drawing.Point(167, 41);
            this.loadParameterListButton.Name = "loadParameterListButton";
            this.loadParameterListButton.Size = new System.Drawing.Size(112, 23);
            this.loadParameterListButton.TabIndex = 13;
            this.loadParameterListButton.Text = "Load parameter list";
            this.loadParameterListButton.UseVisualStyleBackColor = true;
            this.loadParameterListButton.Click += new System.EventHandler(this.LoadParameterListButtonClick);
            // 
            // openFileDialog
            // 
            this.openFileDialog.FileName = "openFileDialog";
            // 
            // SecondForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(904, 688);
            this.Controls.Add(this.loadParameterListButton);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.addSourceButton);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.addPathButton);
            this.Controls.Add(this.customCommandButton);
            this.Controls.Add(this.setBreakpointButton);
            this.Controls.Add(this.inputTextBox);
            this.Controls.Add(this.stopDebuggingButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.startDebuggingButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(920, 726);
            this.MinimumSize = new System.Drawing.Size(920, 726);
            this.Name = "SecondForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GDB command interface";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SecondFormFormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button startDebuggingButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button stopDebuggingButton;
        private System.Windows.Forms.TextBox inputTextBox;
        private System.Windows.Forms.Button setBreakpointButton;
        private System.Windows.Forms.Button customCommandButton;
        private System.Windows.Forms.Button clearButton;
        private System.Windows.Forms.Button addPathButton;
        private System.Windows.Forms.Button addSourceButton;
        private System.Windows.Forms.TextBox asyncNotifyTextBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox logOutputTextBox;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox asyncExecTextBox;
        private System.Windows.Forms.Button loadParameterListButton;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}