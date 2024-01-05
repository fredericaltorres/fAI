namespace faiWinApp
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sliceBy4ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createGifAnimationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.batchModesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createGifAnimationZoomToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.txtUserOutput = new System.Windows.Forms.TextBox();
            this.WorkFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cbGifRepeat = new System.Windows.Forms.CheckBox();
            this.chkViewFileAfterWork = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtZoomImageCount = new System.Windows.Forms.TextBox();
            this.rdoZoomIn = new System.Windows.Forms.RadioButton();
            this.label2 = new System.Windows.Forms.Label();
            this.txtGifDelay = new System.Windows.Forms.TextBox();
            this.rdoGifFade6 = new System.Windows.Forms.RadioButton();
            this.rdoGifFade1 = new System.Windows.Forms.RadioButton();
            this.rdgGifNoFade = new System.Windows.Forms.RadioButton();
            this.butOpenWorkFolder = new System.Windows.Forms.Button();
            this.ckGenerateMP4 = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.editToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(1274, 30);
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
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pasteImageToolStripMenuItem,
            this.sliceBy4ToolStripMenuItem,
            this.createGifAnimationToolStripMenuItem,
            this.batchModesToolStripMenuItem,
            this.toolStripMenuItem1,
            this.clearToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(47, 24);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // pasteImageToolStripMenuItem
            // 
            this.pasteImageToolStripMenuItem.Name = "pasteImageToolStripMenuItem";
            this.pasteImageToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteImageToolStripMenuItem.Size = new System.Drawing.Size(269, 24);
            this.pasteImageToolStripMenuItem.Text = "Paste Image";
            this.pasteImageToolStripMenuItem.Click += new System.EventHandler(this.pasToolStripMenuItem_Click);
            // 
            // sliceBy4ToolStripMenuItem
            // 
            this.sliceBy4ToolStripMenuItem.Name = "sliceBy4ToolStripMenuItem";
            this.sliceBy4ToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.D4)));
            this.sliceBy4ToolStripMenuItem.Size = new System.Drawing.Size(269, 24);
            this.sliceBy4ToolStripMenuItem.Text = "Paste and Slice By 4";
            this.sliceBy4ToolStripMenuItem.Click += new System.EventHandler(this.sliceBy4ToolStripMenuItem_Click);
            // 
            // createGifAnimationToolStripMenuItem
            // 
            this.createGifAnimationToolStripMenuItem.Name = "createGifAnimationToolStripMenuItem";
            this.createGifAnimationToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
            this.createGifAnimationToolStripMenuItem.Size = new System.Drawing.Size(269, 24);
            this.createGifAnimationToolStripMenuItem.Text = "Create Gif Animation";
            this.createGifAnimationToolStripMenuItem.Click += new System.EventHandler(this.createGifAnimationToolStripMenuItem_Click);
            // 
            // batchModesToolStripMenuItem
            // 
            this.batchModesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.createGifAnimationZoomToolStripMenuItem});
            this.batchModesToolStripMenuItem.Name = "batchModesToolStripMenuItem";
            this.batchModesToolStripMenuItem.Size = new System.Drawing.Size(269, 24);
            this.batchModesToolStripMenuItem.Text = "Batch Modes";
            // 
            // createGifAnimationZoomToolStripMenuItem
            // 
            this.createGifAnimationZoomToolStripMenuItem.Name = "createGifAnimationZoomToolStripMenuItem";
            this.createGifAnimationZoomToolStripMenuItem.Size = new System.Drawing.Size(271, 24);
            this.createGifAnimationZoomToolStripMenuItem.Text = "Create Gif Animation - Zoom";
            this.createGifAnimationZoomToolStripMenuItem.Click += new System.EventHandler(this.createGifAnimationZoomToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(266, 6);
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F6;
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(269, 24);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // txtUserOutput
            // 
            this.txtUserOutput.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserOutput.Location = new System.Drawing.Point(26, 281);
            this.txtUserOutput.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserOutput.Multiline = true;
            this.txtUserOutput.Name = "txtUserOutput";
            this.txtUserOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtUserOutput.Size = new System.Drawing.Size(1221, 408);
            this.txtUserOutput.TabIndex = 1;
            // 
            // WorkFolder
            // 
            this.WorkFolder.Location = new System.Drawing.Point(155, 42);
            this.WorkFolder.Margin = new System.Windows.Forms.Padding(4);
            this.WorkFolder.Name = "WorkFolder";
            this.WorkFolder.Size = new System.Drawing.Size(797, 26);
            this.WorkFolder.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 45);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(117, 19);
            this.label1.TabIndex = 3;
            this.label1.Text = "Work Folder:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ckGenerateMP4);
            this.groupBox1.Controls.Add(this.cbGifRepeat);
            this.groupBox1.Controls.Add(this.chkViewFileAfterWork);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtZoomImageCount);
            this.groupBox1.Controls.Add(this.rdoZoomIn);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtGifDelay);
            this.groupBox1.Controls.Add(this.rdoGifFade6);
            this.groupBox1.Controls.Add(this.rdoGifFade1);
            this.groupBox1.Controls.Add(this.rdgGifNoFade);
            this.groupBox1.Location = new System.Drawing.Point(26, 85);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1139, 177);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Gif Animation";
            // 
            // cbGifRepeat
            // 
            this.cbGifRepeat.AutoSize = true;
            this.cbGifRepeat.Location = new System.Drawing.Point(701, 40);
            this.cbGifRepeat.Name = "cbGifRepeat";
            this.cbGifRepeat.Size = new System.Drawing.Size(82, 23);
            this.cbGifRepeat.TabIndex = 11;
            this.cbGifRepeat.Text = "Repeat";
            this.cbGifRepeat.UseVisualStyleBackColor = true;
            // 
            // chkViewFileAfterWork
            // 
            this.chkViewFileAfterWork.AutoSize = true;
            this.chkViewFileAfterWork.Location = new System.Drawing.Point(14, 131);
            this.chkViewFileAfterWork.Name = "chkViewFileAfterWork";
            this.chkViewFileAfterWork.Size = new System.Drawing.Size(208, 23);
            this.chkViewFileAfterWork.TabIndex = 10;
            this.chkViewFileAfterWork.Text = "View File After Work";
            this.chkViewFileAfterWork.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(953, 38);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 19);
            this.label3.TabIndex = 7;
            this.label3.Text = "Image Count:";
            // 
            // txtZoomImageCount
            // 
            this.txtZoomImageCount.Location = new System.Drawing.Point(1092, 33);
            this.txtZoomImageCount.Margin = new System.Windows.Forms.Padding(4);
            this.txtZoomImageCount.Name = "txtZoomImageCount";
            this.txtZoomImageCount.Size = new System.Drawing.Size(40, 26);
            this.txtZoomImageCount.TabIndex = 6;
            this.txtZoomImageCount.Text = "200";
            // 
            // rdoZoomIn
            // 
            this.rdoZoomIn.AutoSize = true;
            this.rdoZoomIn.Location = new System.Drawing.Point(836, 36);
            this.rdoZoomIn.Name = "rdoZoomIn";
            this.rdoZoomIn.Size = new System.Drawing.Size(90, 23);
            this.rdoZoomIn.TabIndex = 5;
            this.rdoZoomIn.Text = "Zoom In";
            this.rdoZoomIn.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(551, 40);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 19);
            this.label2.TabIndex = 4;
            this.label2.Text = "Delay:";
            // 
            // txtGifDelay
            // 
            this.txtGifDelay.Location = new System.Drawing.Point(622, 39);
            this.txtGifDelay.Margin = new System.Windows.Forms.Padding(4);
            this.txtGifDelay.Name = "txtGifDelay";
            this.txtGifDelay.Size = new System.Drawing.Size(45, 26);
            this.txtGifDelay.TabIndex = 3;
            this.txtGifDelay.Text = "200";
            // 
            // rdoGifFade6
            // 
            this.rdoGifFade6.AutoSize = true;
            this.rdoGifFade6.Location = new System.Drawing.Point(345, 39);
            this.rdoGifFade6.Name = "rdoGifFade6";
            this.rdoGifFade6.Size = new System.Drawing.Size(198, 23);
            this.rdoGifFade6.TabIndex = 2;
            this.rdoGifFade6.Text = "6 Transition Images";
            this.rdoGifFade6.UseVisualStyleBackColor = true;
            // 
            // rdoGifFade1
            // 
            this.rdoGifFade1.AutoSize = true;
            this.rdoGifFade1.Location = new System.Drawing.Point(129, 39);
            this.rdoGifFade1.Name = "rdoGifFade1";
            this.rdoGifFade1.Size = new System.Drawing.Size(189, 23);
            this.rdoGifFade1.TabIndex = 1;
            this.rdoGifFade1.Text = "1 Transition Image";
            this.rdoGifFade1.UseVisualStyleBackColor = true;
            // 
            // rdgGifNoFade
            // 
            this.rdgGifNoFade.AutoSize = true;
            this.rdgGifNoFade.Checked = true;
            this.rdgGifNoFade.Location = new System.Drawing.Point(14, 39);
            this.rdgGifNoFade.Name = "rdgGifNoFade";
            this.rdgGifNoFade.Size = new System.Drawing.Size(90, 23);
            this.rdgGifNoFade.TabIndex = 0;
            this.rdgGifNoFade.TabStop = true;
            this.rdgGifNoFade.Text = "No Fade";
            this.rdgGifNoFade.UseVisualStyleBackColor = true;
            // 
            // butOpenWorkFolder
            // 
            this.butOpenWorkFolder.Location = new System.Drawing.Point(983, 42);
            this.butOpenWorkFolder.Name = "butOpenWorkFolder";
            this.butOpenWorkFolder.Size = new System.Drawing.Size(182, 37);
            this.butOpenWorkFolder.TabIndex = 5;
            this.butOpenWorkFolder.Text = "Open In Explorer";
            this.butOpenWorkFolder.UseVisualStyleBackColor = true;
            this.butOpenWorkFolder.Click += new System.EventHandler(this.butOpenWorkFolder_Click);
            // 
            // ckGenerateMP4
            // 
            this.ckGenerateMP4.AutoSize = true;
            this.ckGenerateMP4.Location = new System.Drawing.Point(14, 83);
            this.ckGenerateMP4.Name = "ckGenerateMP4";
            this.ckGenerateMP4.Size = new System.Drawing.Size(127, 23);
            this.ckGenerateMP4.TabIndex = 12;
            this.ckGenerateMP4.Text = "GenerateMP4";
            this.ckGenerateMP4.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1274, 714);
            this.Controls.Add(this.butOpenWorkFolder);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.WorkFolder);
            this.Controls.Add(this.txtUserOutput);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "fAi Win Utility App";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteImageToolStripMenuItem;
        private System.Windows.Forms.TextBox txtUserOutput;
        private System.Windows.Forms.ToolStripMenuItem sliceBy4ToolStripMenuItem;
        private System.Windows.Forms.TextBox WorkFolder;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem createGifAnimationToolStripMenuItem;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rdoGifFade1;
        private System.Windows.Forms.RadioButton rdgGifNoFade;
        private System.Windows.Forms.RadioButton rdoGifFade6;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtGifDelay;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.RadioButton rdoZoomIn;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtZoomImageCount;
        private System.Windows.Forms.ToolStripMenuItem batchModesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createGifAnimationZoomToolStripMenuItem;
        private System.Windows.Forms.Button butOpenWorkFolder;
        private System.Windows.Forms.CheckBox chkViewFileAfterWork;
        private System.Windows.Forms.CheckBox cbGifRepeat;
        private System.Windows.Forms.CheckBox ckGenerateMP4;
    }
}

