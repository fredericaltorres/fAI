﻿namespace faiRealTimeConversation
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.butTalk = new System.Windows.Forms.Button();
            this.txtUserOutput = new System.Windows.Forms.TextBox();
            this.cboInputDevices = new System.Windows.Forms.ComboBox();
            this.cboOutputDevices = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.butTest = new System.Windows.Forms.Button();
            this.tmr_TTS = new System.Windows.Forms.Timer(this.components);
            this.button1 = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1053, 30);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Q)));
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(159, 24);
            this.quitToolStripMenuItem.Text = "Quit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // butTalk
            // 
            this.butTalk.Location = new System.Drawing.Point(926, 415);
            this.butTalk.Name = "butTalk";
            this.butTalk.Size = new System.Drawing.Size(115, 58);
            this.butTalk.TabIndex = 1;
            this.butTalk.Text = "Talk";
            this.butTalk.UseVisualStyleBackColor = true;
            this.butTalk.Click += new System.EventHandler(this.butTalk_Click);
            // 
            // txtUserOutput
            // 
            this.txtUserOutput.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserOutput.Location = new System.Drawing.Point(24, 45);
            this.txtUserOutput.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserOutput.Multiline = true;
            this.txtUserOutput.Name = "txtUserOutput";
            this.txtUserOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtUserOutput.Size = new System.Drawing.Size(985, 347);
            this.txtUserOutput.TabIndex = 2;
            // 
            // cboInputDevices
            // 
            this.cboInputDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboInputDevices.FormattingEnabled = true;
            this.cboInputDevices.Location = new System.Drawing.Point(149, 415);
            this.cboInputDevices.Name = "cboInputDevices";
            this.cboInputDevices.Size = new System.Drawing.Size(429, 27);
            this.cboInputDevices.TabIndex = 4;
            // 
            // cboOutputDevices
            // 
            this.cboOutputDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOutputDevices.FormattingEnabled = true;
            this.cboOutputDevices.Location = new System.Drawing.Point(149, 478);
            this.cboOutputDevices.Name = "cboOutputDevices";
            this.cboOutputDevices.Size = new System.Drawing.Size(429, 27);
            this.cboOutputDevices.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 415);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 19);
            this.label1.TabIndex = 6;
            this.label1.Text = "Input:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 481);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 19);
            this.label2.TabIndex = 7;
            this.label2.Text = "Output:";
            // 
            // butTest
            // 
            this.butTest.Location = new System.Drawing.Point(673, 399);
            this.butTest.Name = "butTest";
            this.butTest.Size = new System.Drawing.Size(104, 35);
            this.butTest.TabIndex = 8;
            this.butTest.Text = "Test";
            this.butTest.UseVisualStyleBackColor = true;
            this.butTest.Click += new System.EventHandler(this.butTest_Click);
            // 
            // tmr_TTS
            // 
            this.tmr_TTS.Tick += new System.EventHandler(this.tmr_TTS_Tick);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(673, 470);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(189, 35);
            this.button1.TabIndex = 9;
            this.button1.Text = "TTS Answer";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1053, 537);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.butTest);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboOutputDevices);
            this.Controls.Add(this.cboInputDevices);
            this.Controls.Add(this.txtUserOutput);
            this.Controls.Add(this.butTalk);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "fAI Real Time Conversation";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.Button butTalk;
        private System.Windows.Forms.TextBox txtUserOutput;
        private System.Windows.Forms.ComboBox cboInputDevices;
        private System.Windows.Forms.ComboBox cboOutputDevices;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button butTest;
        private System.Windows.Forms.Timer tmr_TTS;
        private System.Windows.Forms.Button button1;
    }
}

