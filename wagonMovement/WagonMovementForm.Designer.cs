﻿namespace wagonMovement
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WagonMovementForm));
            this.wagonFile = new System.Windows.Forms.TextBox();
            this.SelectFileButton = new System.Windows.Forms.Button();
            this.ProcessButton = new System.Windows.Forms.Button();
            this.executionTime = new System.Windows.Forms.Label();
            this.destinationDirectoryButton = new System.Windows.Forms.Button();
            this.destinationDirectory = new System.Windows.Forms.TextBox();
            this.TimerLabel = new System.Windows.Forms.Label();
            this.formFromDate = new System.Windows.Forms.DateTimePicker();
            this.formToDate = new System.Windows.Forms.DateTimePicker();
            this.fromLabel = new System.Windows.Forms.Label();
            this.toLabel = new System.Windows.Forms.Label();
            this.financialYear = new System.Windows.Forms.CheckBox();
            this.volumeModelFlag = new System.Windows.Forms.CheckBox();
            this.addIntermodalAndSteelToInterstate = new System.Windows.Forms.CheckBox();
            this.UseSQLCommand = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // wagonFile
            // 
            this.wagonFile.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.wagonFile.Location = new System.Drawing.Point(173, 87);
            this.wagonFile.Name = "wagonFile";
            this.wagonFile.Size = new System.Drawing.Size(539, 20);
            this.wagonFile.TabIndex = 1;
            this.wagonFile.Text = "<Select a file>";
            // 
            // SelectFileButton
            // 
            this.SelectFileButton.Location = new System.Drawing.Point(12, 80);
            this.SelectFileButton.Name = "SelectFileButton";
            this.SelectFileButton.Size = new System.Drawing.Size(155, 33);
            this.SelectFileButton.TabIndex = 2;
            this.SelectFileButton.Text = "Select the Wagon Data";
            this.SelectFileButton.UseVisualStyleBackColor = true;
            this.SelectFileButton.Click += new System.EventHandler(this.SelectDataFile_Click);
            // 
            // ProcessButton
            // 
            this.ProcessButton.Location = new System.Drawing.Point(568, 160);
            this.ProcessButton.Name = "ProcessButton";
            this.ProcessButton.Size = new System.Drawing.Size(236, 46);
            this.ProcessButton.TabIndex = 3;
            this.ProcessButton.Text = "Process";
            this.ProcessButton.UseVisualStyleBackColor = true;
            this.ProcessButton.Click += new System.EventHandler(this.processWagonData);
            // 
            // executionTime
            // 
            this.executionTime.AutoSize = true;
            this.executionTime.Location = new System.Drawing.Point(726, 211);
            this.executionTime.Name = "executionTime";
            this.executionTime.Size = new System.Drawing.Size(26, 13);
            this.executionTime.TabIndex = 4;
            this.executionTime.Text = "time";
            // 
            // destinationDirectoryButton
            // 
            this.destinationDirectoryButton.Location = new System.Drawing.Point(12, 119);
            this.destinationDirectoryButton.Name = "destinationDirectoryButton";
            this.destinationDirectoryButton.Size = new System.Drawing.Size(155, 29);
            this.destinationDirectoryButton.TabIndex = 5;
            this.destinationDirectoryButton.Text = "Select Destination";
            this.destinationDirectoryButton.UseVisualStyleBackColor = true;
            this.destinationDirectoryButton.Click += new System.EventHandler(this.destinationDirectoryButton_Click);
            // 
            // destinationDirectory
            // 
            this.destinationDirectory.Location = new System.Drawing.Point(173, 124);
            this.destinationDirectory.Name = "destinationDirectory";
            this.destinationDirectory.Size = new System.Drawing.Size(631, 20);
            this.destinationDirectory.TabIndex = 6;
            this.destinationDirectory.Text = "<Default>";
            // 
            // TimerLabel
            // 
            this.TimerLabel.AutoSize = true;
            this.TimerLabel.Location = new System.Drawing.Point(565, 211);
            this.TimerLabel.Name = "TimerLabel";
            this.TimerLabel.Size = new System.Drawing.Size(83, 13);
            this.TimerLabel.TabIndex = 7;
            this.TimerLabel.Text = "Execution Time:";
            // 
            // formFromDate
            // 
            this.formFromDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.formFromDate.Location = new System.Drawing.Point(173, 13);
            this.formFromDate.Name = "formFromDate";
            this.formFromDate.Size = new System.Drawing.Size(105, 20);
            this.formFromDate.TabIndex = 8;
            this.formFromDate.Value = new System.DateTime(2016, 7, 1, 0, 0, 0, 0);
            // 
            // formToDate
            // 
            this.formToDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.formToDate.Location = new System.Drawing.Point(318, 13);
            this.formToDate.Name = "formToDate";
            this.formToDate.Size = new System.Drawing.Size(99, 20);
            this.formToDate.TabIndex = 9;
            this.formToDate.Value = new System.DateTime(2017, 7, 1, 0, 0, 0, 0);
            // 
            // fromLabel
            // 
            this.fromLabel.AutoSize = true;
            this.fromLabel.Location = new System.Drawing.Point(131, 19);
            this.fromLabel.Name = "fromLabel";
            this.fromLabel.Size = new System.Drawing.Size(33, 13);
            this.fromLabel.TabIndex = 10;
            this.fromLabel.Text = "From:";
            // 
            // toLabel
            // 
            this.toLabel.AutoSize = true;
            this.toLabel.Location = new System.Drawing.Point(289, 19);
            this.toLabel.Name = "toLabel";
            this.toLabel.Size = new System.Drawing.Size(23, 13);
            this.toLabel.TabIndex = 11;
            this.toLabel.Text = "To:";
            // 
            // financialYear
            // 
            this.financialYear.AutoSize = true;
            this.financialYear.Location = new System.Drawing.Point(459, 15);
            this.financialYear.Name = "financialYear";
            this.financialYear.Size = new System.Drawing.Size(116, 17);
            this.financialYear.TabIndex = 12;
            this.financialYear.Text = "Last Financial Year";
            this.financialYear.UseVisualStyleBackColor = true;
            this.financialYear.CheckedChanged += new System.EventHandler(this.financialYear_CheckedChanged);
            // 
            // volumeModelFlag
            // 
            this.volumeModelFlag.AutoSize = true;
            this.volumeModelFlag.Location = new System.Drawing.Point(605, 15);
            this.volumeModelFlag.Name = "volumeModelFlag";
            this.volumeModelFlag.Size = new System.Drawing.Size(93, 17);
            this.volumeModelFlag.TabIndex = 13;
            this.volumeModelFlag.Text = "Volume Model";
            this.volumeModelFlag.UseVisualStyleBackColor = true;
            this.volumeModelFlag.CheckedChanged += new System.EventHandler(this.volumeModelFlag_CheckedChanged);
            // 
            // addIntermodalAndSteelToInterstate
            // 
            this.addIntermodalAndSteelToInterstate.AutoSize = true;
            this.addIntermodalAndSteelToInterstate.Location = new System.Drawing.Point(173, 160);
            this.addIntermodalAndSteelToInterstate.Name = "addIntermodalAndSteelToInterstate";
            this.addIntermodalAndSteelToInterstate.Size = new System.Drawing.Size(234, 17);
            this.addIntermodalAndSteelToInterstate.TabIndex = 14;
            this.addIntermodalAndSteelToInterstate.Text = "Combine Intermodal and Steel into Interstate";
            this.addIntermodalAndSteelToInterstate.UseVisualStyleBackColor = true;
            this.addIntermodalAndSteelToInterstate.CheckedChanged += new System.EventHandler(this.addIntermodalAndSteelToInterstate_CheckedChanged);
            // 
            // UseSQLCommand
            // 
            this.UseSQLCommand.AutoSize = true;
            this.UseSQLCommand.Location = new System.Drawing.Point(173, 64);
            this.UseSQLCommand.Name = "UseSQLCommand";
            this.UseSQLCommand.Size = new System.Drawing.Size(152, 17);
            this.UseSQLCommand.TabIndex = 15;
            this.UseSQLCommand.Text = "Access Data Automatically";
            this.UseSQLCommand.UseVisualStyleBackColor = true;
            this.UseSQLCommand.CheckedChanged += new System.EventHandler(this.AutoDataChecked);
            // 
            // WagonMovementForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(823, 233);
            this.Controls.Add(this.UseSQLCommand);
            this.Controls.Add(this.addIntermodalAndSteelToInterstate);
            this.Controls.Add(this.volumeModelFlag);
            this.Controls.Add(this.financialYear);
            this.Controls.Add(this.toLabel);
            this.Controls.Add(this.fromLabel);
            this.Controls.Add(this.formToDate);
            this.Controls.Add(this.formFromDate);
            this.Controls.Add(this.TimerLabel);
            this.Controls.Add(this.destinationDirectory);
            this.Controls.Add(this.destinationDirectoryButton);
            this.Controls.Add(this.executionTime);
            this.Controls.Add(this.ProcessButton);
            this.Controls.Add(this.SelectFileButton);
            this.Controls.Add(this.wagonFile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
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
        private System.Windows.Forms.DateTimePicker formFromDate;
        private System.Windows.Forms.DateTimePicker formToDate;
        private System.Windows.Forms.Label fromLabel;
        private System.Windows.Forms.Label toLabel;
        private System.Windows.Forms.CheckBox financialYear;
        private System.Windows.Forms.CheckBox volumeModelFlag;
        private System.Windows.Forms.CheckBox addIntermodalAndSteelToInterstate;
        private System.Windows.Forms.CheckBox UseSQLCommand;
    }
}