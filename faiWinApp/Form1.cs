using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace faiWinApp
{
    public partial class Form1 : Form
    {
        private string _lastImageFile = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void UserMessage(string m)
        {
            this.txtUserOutput.Text += $"{m}{(m.EndsWith(Environment.NewLine) ? "" : Environment.NewLine)}";
            this.txtUserOutput.SelectionStart = this.txtUserOutput.TextLength;
            this.txtUserOutput.ScrollToCaret();
            Application.DoEvents();
        }

        private void pasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newImageFile = ImageUtility.SaveImageFromClipboard(ImageFormat.Png);
            if(newImageFile != null)
            {
                this.UserMessage($"Saved image to {newImageFile}");
                this.UserMessage(ImageUtility.GetImageInfo(newImageFile));
                ImageUtility.ViewFile(newImageFile);
                _lastImageFile = newImageFile;
            }
            else MessageBox.Show("No image found in clipboard.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.UserMessage("Ready...");
        }

        private void sliceBy4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newImages = ImageUtility.SliceImageBy4(_lastImageFile);
            newImages.ForEach(newImage =>
            {
                this.UserMessage(ImageUtility.GetImageInfo(newImage));
                ImageUtility.ViewFile(newImage);
            });
        }
    }
}
