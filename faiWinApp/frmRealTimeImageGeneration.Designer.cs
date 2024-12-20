namespace faiWinApp
{
    partial class frmRealTimeImageGeneration
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmRealTimeImageGeneration));
            this.txtUserOutput = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPrompt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.renderImage = new System.Windows.Forms.ToolStripMenuItem();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cboModels = new System.Windows.Forms.ComboBox();
            this.lblModelDescription = new System.Windows.Forms.Label();
            this.chkAlchemy = new System.Windows.Forms.CheckBox();
            this.chkPhotoReal = new System.Windows.Forms.CheckBox();
            this.chkPromptMagic = new System.Windows.Forms.CheckBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtUserOutput
            // 
            this.txtUserOutput.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtUserOutput.Location = new System.Drawing.Point(15, 613);
            this.txtUserOutput.Margin = new System.Windows.Forms.Padding(6);
            this.txtUserOutput.Multiline = true;
            this.txtUserOutput.Name = "txtUserOutput";
            this.txtUserOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtUserOutput.Size = new System.Drawing.Size(1350, 317);
            this.txtUserOutput.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 588);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 19);
            this.label4.TabIndex = 16;
            this.label4.Text = "Output:";
            // 
            // txtPrompt
            // 
            this.txtPrompt.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPrompt.Location = new System.Drawing.Point(15, 186);
            this.txtPrompt.Margin = new System.Windows.Forms.Padding(6);
            this.txtPrompt.Multiline = true;
            this.txtPrompt.Name = "txtPrompt";
            this.txtPrompt.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPrompt.Size = new System.Drawing.Size(729, 396);
            this.txtPrompt.TabIndex = 17;
            this.txtPrompt.Text = resources.GetString("txtPrompt.Text");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 143);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 19);
            this.label1.TabIndex = 18;
            this.label1.Text = "Prompt:";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.imageToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1380, 28);
            this.menuStrip1.TabIndex = 19;
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
            // imageToolStripMenuItem
            // 
            this.imageToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.renderImage});
            this.imageToolStripMenuItem.Name = "imageToolStripMenuItem";
            this.imageToolStripMenuItem.Size = new System.Drawing.Size(63, 24);
            this.imageToolStripMenuItem.Text = "Image";
            // 
            // renderImage
            // 
            this.renderImage.Name = "renderImage";
            this.renderImage.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.renderImage.Size = new System.Drawing.Size(149, 24);
            this.renderImage.Text = "Render";
            this.renderImage.Click += new System.EventHandler(this.renderImage_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(808, 47);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(512, 512);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 20;
            this.pictureBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(21, 47);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 19);
            this.label2.TabIndex = 21;
            this.label2.Text = "Model:";
            // 
            // cboModels
            // 
            this.cboModels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboModels.FormattingEnabled = true;
            this.cboModels.Location = new System.Drawing.Point(93, 44);
            this.cboModels.Name = "cboModels";
            this.cboModels.Size = new System.Drawing.Size(553, 27);
            this.cboModels.TabIndex = 22;
            this.cboModels.SelectedIndexChanged += new System.EventHandler(this.cboModels_SelectedIndexChanged);
            // 
            // lblModelDescription
            // 
            this.lblModelDescription.AutoSize = true;
            this.lblModelDescription.Location = new System.Drawing.Point(100, 86);
            this.lblModelDescription.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.lblModelDescription.Name = "lblModelDescription";
            this.lblModelDescription.Size = new System.Drawing.Size(162, 19);
            this.lblModelDescription.TabIndex = 23;
            this.lblModelDescription.Text = "Model Description";
            // 
            // chkAlchemy
            // 
            this.chkAlchemy.AutoSize = true;
            this.chkAlchemy.Location = new System.Drawing.Point(25, 117);
            this.chkAlchemy.Name = "chkAlchemy";
            this.chkAlchemy.Size = new System.Drawing.Size(91, 23);
            this.chkAlchemy.TabIndex = 24;
            this.chkAlchemy.Text = "Alchemy";
            this.chkAlchemy.UseVisualStyleBackColor = true;
            // 
            // chkPhotoReal
            // 
            this.chkPhotoReal.AutoSize = true;
            this.chkPhotoReal.Location = new System.Drawing.Point(122, 117);
            this.chkPhotoReal.Name = "chkPhotoReal";
            this.chkPhotoReal.Size = new System.Drawing.Size(109, 23);
            this.chkPhotoReal.TabIndex = 25;
            this.chkPhotoReal.Text = "PhotoReal";
            this.chkPhotoReal.UseVisualStyleBackColor = true;
            // 
            // chkPromptMagic
            // 
            this.chkPromptMagic.AutoSize = true;
            this.chkPromptMagic.Location = new System.Drawing.Point(237, 117);
            this.chkPromptMagic.Name = "chkPromptMagic";
            this.chkPromptMagic.Size = new System.Drawing.Size(127, 23);
            this.chkPromptMagic.TabIndex = 26;
            this.chkPromptMagic.Text = "PromptMagic";
            this.chkPromptMagic.UseVisualStyleBackColor = true;
            // 
            // frmRealTimeImageGeneration
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1380, 936);
            this.Controls.Add(this.chkPromptMagic);
            this.Controls.Add(this.chkPhotoReal);
            this.Controls.Add(this.chkAlchemy);
            this.Controls.Add(this.lblModelDescription);
            this.Controls.Add(this.cboModels);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtPrompt);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtUserOutput);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "frmRealTimeImageGeneration";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmRealTimeImageGeneration";
            this.Load += new System.EventHandler(this.frmRealTimeImageGeneration_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtUserOutput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtPrompt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem renderImage;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cboModels;
        private System.Windows.Forms.Label lblModelDescription;
        private System.Windows.Forms.CheckBox chkAlchemy;
        private System.Windows.Forms.CheckBox chkPhotoReal;
        private System.Windows.Forms.CheckBox chkPromptMagic;
    }
}