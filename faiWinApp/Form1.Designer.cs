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
            this.txtUserOutput = new System.Windows.Forms.TextBox();
            this.WorkFolder = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tESTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mosaicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
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
            this.menuStrip1.Size = new System.Drawing.Size(1177, 30);
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
            this.tESTToolStripMenuItem,
            this.mosaicToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(47, 24);
            this.editToolStripMenuItem.Text = "Edit";
            // 
            // pasteImageToolStripMenuItem
            // 
            this.pasteImageToolStripMenuItem.Name = "pasteImageToolStripMenuItem";
            this.pasteImageToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.pasteImageToolStripMenuItem.Size = new System.Drawing.Size(217, 24);
            this.pasteImageToolStripMenuItem.Text = "Paste Image";
            this.pasteImageToolStripMenuItem.Click += new System.EventHandler(this.pasToolStripMenuItem_Click);
            // 
            // sliceBy4ToolStripMenuItem
            // 
            this.sliceBy4ToolStripMenuItem.Name = "sliceBy4ToolStripMenuItem";
            this.sliceBy4ToolStripMenuItem.Size = new System.Drawing.Size(217, 24);
            this.sliceBy4ToolStripMenuItem.Text = "Slice By 4";
            this.sliceBy4ToolStripMenuItem.Click += new System.EventHandler(this.sliceBy4ToolStripMenuItem_Click);
            // 
            // createGifAnimationToolStripMenuItem
            // 
            this.createGifAnimationToolStripMenuItem.Name = "createGifAnimationToolStripMenuItem";
            this.createGifAnimationToolStripMenuItem.Size = new System.Drawing.Size(217, 24);
            this.createGifAnimationToolStripMenuItem.Text = "Create Gif Animation";
            this.createGifAnimationToolStripMenuItem.Click += new System.EventHandler(this.createGifAnimationToolStripMenuItem_Click);
            // 
            // txtUserOutput
            // 
            this.txtUserOutput.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserOutput.Location = new System.Drawing.Point(17, 89);
            this.txtUserOutput.Margin = new System.Windows.Forms.Padding(4);
            this.txtUserOutput.Multiline = true;
            this.txtUserOutput.Name = "txtUserOutput";
            this.txtUserOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtUserOutput.Size = new System.Drawing.Size(991, 487);
            this.txtUserOutput.TabIndex = 1;
            // 
            // WorkFolder
            // 
            this.WorkFolder.Location = new System.Drawing.Point(155, 42);
            this.WorkFolder.Margin = new System.Windows.Forms.Padding(4);
            this.WorkFolder.Name = "WorkFolder";
            this.WorkFolder.Size = new System.Drawing.Size(984, 26);
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
            // tESTToolStripMenuItem
            // 
            this.tESTToolStripMenuItem.Name = "tESTToolStripMenuItem";
            this.tESTToolStripMenuItem.Size = new System.Drawing.Size(217, 24);
            this.tESTToolStripMenuItem.Text = "Test";
            this.tESTToolStripMenuItem.Click += new System.EventHandler(this.tESTToolStripMenuItem_Click);
            // 
            // mosaicToolStripMenuItem
            // 
            this.mosaicToolStripMenuItem.Name = "mosaicToolStripMenuItem";
            this.mosaicToolStripMenuItem.Size = new System.Drawing.Size(217, 24);
            this.mosaicToolStripMenuItem.Text = "Mosaic";
            this.mosaicToolStripMenuItem.Click += new System.EventHandler(this.mosaicToolStripMenuItem_Click);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1177, 608);
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
        private System.Windows.Forms.ToolStripMenuItem tESTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem mosaicToolStripMenuItem;
    }
}

