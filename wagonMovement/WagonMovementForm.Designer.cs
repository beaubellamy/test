namespace wagonMovement
{
    partial class WagonMovementForm
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
            this.wagonFile = new System.Windows.Forms.TextBox();
            this.SelectFileButton = new System.Windows.Forms.Button();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.executionTime = new System.Windows.Forms.Label();
            this.destinationDirectoryButton = new System.Windows.Forms.Button();
            this.destinationDirectory = new System.Windows.Forms.TextBox();
            this.TimerLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // wagonFile
            // 
            this.wagonFile.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.wagonFile.Location = new System.Drawing.Point(173, 48);
            this.wagonFile.Name = "wagonFile";
            this.wagonFile.Size = new System.Drawing.Size(631, 20);
            this.wagonFile.TabIndex = 1;
            this.wagonFile.Text = "<Select a file>";
            // 
            // SelectFileButton
            // 
            this.SelectFileButton.Location = new System.Drawing.Point(12, 41);
            this.SelectFileButton.Name = "SelectFileButton";
            this.SelectFileButton.Size = new System.Drawing.Size(155, 33);
            this.SelectFileButton.TabIndex = 2;
            this.SelectFileButton.Text = "Select the Wagon Data";
            this.SelectFileButton.UseVisualStyleBackColor = true;
            this.SelectFileButton.Click += new System.EventHandler(this.SelectDataFile_Click);
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(173, 126);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(216, 46);
            this.ProcessButton.TabIndex = 3;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.processWagonData);
            // 
            // executionTime
            // 
            this.executionTime.AutoSize = true;
            this.executionTime.Location = new System.Drawing.Point(170, 203);
            this.executionTime.Name = "executionTime";
            this.executionTime.Size = new System.Drawing.Size(26, 13);
            this.executionTime.TabIndex = 4;
            this.executionTime.Text = "time";
            // 
            // destinationDirectoryButton
            // 
            this.destinationDirectoryButton.Location = new System.Drawing.Point(12, 80);
            this.destinationDirectoryButton.Name = "destinationDirectoryButton";
            this.destinationDirectoryButton.Size = new System.Drawing.Size(155, 29);
            this.destinationDirectoryButton.TabIndex = 5;
            this.destinationDirectoryButton.Text = "Select Destination";
            this.destinationDirectoryButton.UseVisualStyleBackColor = true;
            this.destinationDirectoryButton.Click += new System.EventHandler(this.destinationDirectoryButton_Click);
            // 
            // destinationDirectory
            // 
            this.destinationDirectory.Location = new System.Drawing.Point(173, 85);
            this.destinationDirectory.Name = "destinationDirectory";
            this.destinationDirectory.Size = new System.Drawing.Size(631, 20);
            this.destinationDirectory.TabIndex = 6;
            this.destinationDirectory.Text = "<Default>";
            // 
            // TimerLabel
            // 
            this.TimerLabel.AutoSize = true;
            this.TimerLabel.Location = new System.Drawing.Point(81, 203);
            this.TimerLabel.Name = "TimerLabel";
            this.TimerLabel.Size = new System.Drawing.Size(83, 13);
            this.TimerLabel.TabIndex = 7;
            this.TimerLabel.Text = "Execution Time:";
            // 
            // WagonMovementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 233);
            this.Controls.Add(this.TimerLabel);
            this.Controls.Add(this.destinationDirectory);
            this.Controls.Add(this.destinationDirectoryButton);
            this.Controls.Add(this.executionTime);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.SelectFileButton);
            this.Controls.Add(this.wagonFile);
            this.Name = "WagonMovementForm";
            this.Text = "WagonMovementForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox wagonFile;
        private System.Windows.Forms.Button SelectFileButton;
        private System.Windows.Forms.Button ProcessButton;
        private System.Windows.Forms.Label executionTime;
        private System.Windows.Forms.Button destinationDirectoryButton;
        private System.Windows.Forms.TextBox destinationDirectory;
        private System.Windows.Forms.Label TimerLabel;
    }
}