namespace AccessGdb
{
    partial class MainForm
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
            this.sendCommandButton = new System.Windows.Forms.Button();
            this.sendCommandTextBox = new System.Windows.Forms.TextBox();
            this.useDefaultCmd = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.clearOutputButton = new System.Windows.Forms.Button();
            this.klausButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // sendCommandButton
            // 
            this.sendCommandButton.Location = new System.Drawing.Point(420, 30);
            this.sendCommandButton.Name = "sendCommandButton";
            this.sendCommandButton.Size = new System.Drawing.Size(75, 23);
            this.sendCommandButton.TabIndex = 1;
            this.sendCommandButton.Text = "Send";
            this.sendCommandButton.UseVisualStyleBackColor = true;
            this.sendCommandButton.Click += new System.EventHandler(this.SendCommandButtonClick);
            // 
            // sendCommandTextBox
            // 
            this.sendCommandTextBox.Location = new System.Drawing.Point(24, 32);
            this.sendCommandTextBox.Name = "sendCommandTextBox";
            this.sendCommandTextBox.Size = new System.Drawing.Size(390, 20);
            this.sendCommandTextBox.TabIndex = 0;
            // 
            // useDefaultCmd
            // 
            this.useDefaultCmd.AutoSize = true;
            this.useDefaultCmd.Location = new System.Drawing.Point(24, 58);
            this.useDefaultCmd.Name = "useDefaultCmd";
            this.useDefaultCmd.Size = new System.Drawing.Size(101, 17);
            this.useDefaultCmd.TabIndex = 2;
            this.useDefaultCmd.Text = "use default cmd";
            this.useDefaultCmd.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.outputTextBox);
            this.groupBox1.Location = new System.Drawing.Point(12, 89);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(494, 477);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Output";
            // 
            // outputTextBox
            // 
            this.outputTextBox.Location = new System.Drawing.Point(12, 19);
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.Size = new System.Drawing.Size(471, 447);
            this.outputTextBox.TabIndex = 0;
            // 
            // clearOutputButton
            // 
            this.clearOutputButton.Location = new System.Drawing.Point(420, 58);
            this.clearOutputButton.Name = "clearOutputButton";
            this.clearOutputButton.Size = new System.Drawing.Size(75, 23);
            this.clearOutputButton.TabIndex = 3;
            this.clearOutputButton.Text = "Clear";
            this.clearOutputButton.UseVisualStyleBackColor = true;
            this.clearOutputButton.Click += new System.EventHandler(this.ClearOutputButtonClick);
            // 
            // klausButton
            // 
            this.klausButton.Location = new System.Drawing.Point(339, 58);
            this.klausButton.Name = "klausButton";
            this.klausButton.Size = new System.Drawing.Size(75, 23);
            this.klausButton.TabIndex = 5;
            this.klausButton.Text = "Klaus";
            this.klausButton.UseVisualStyleBackColor = true;
            this.klausButton.Click += new System.EventHandler(this.KlausButtonClick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 585);
            this.Controls.Add(this.klausButton);
            this.Controls.Add(this.clearOutputButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.useDefaultCmd);
            this.Controls.Add(this.sendCommandTextBox);
            this.Controls.Add(this.sendCommandButton);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(535, 623);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(535, 623);
            this.Name = "MainForm";
            this.Text = "GDB Communicator";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button sendCommandButton;
        private System.Windows.Forms.TextBox sendCommandTextBox;
        private System.Windows.Forms.CheckBox useDefaultCmd;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button clearOutputButton;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.Windows.Forms.Button klausButton;
    }
}

