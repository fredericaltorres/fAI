using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace faiWinApp
{

    public partial class Form1 : Form
    {
        private string _lastImageFile = null;
        AppOptions _appOptions = new AppOptions();
        List<string> _dragAndDropFileSelection = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _appOptions.WorkFolder = this.WorkFolder.Text;
            _appOptions.ToFile();
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
            var newImageFile = ImageUtility.SaveImageFromClipboard(ImageFormat.Png, _appOptions.WorkFolder);
            if(newImageFile != null)
            {
                this.UserMessage($"Saved image to {newImageFile}");
                this.UserMessage(ImageUtility.GetImageInfo(newImageFile, _appOptions.WorkFolder));
                ImageUtility.ViewFile(newImageFile);
                _lastImageFile = newImageFile;
            }
            else MessageBox.Show("No image found in clipboard.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _appOptions = AppOptions.FromFile($@".\{Application.ProductName}.config.json");
            this.WorkFolder.Text = _appOptions.WorkFolder;
            this.UserMessage("Ready...");
        }

        private void sliceBy4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newImages = ImageUtility.SliceImageBy4(_lastImageFile, _appOptions.WorkFolder);
            newImages.ForEach(newImage =>
            {
                this.UserMessage(ImageUtility.GetImageInfo(newImage, _appOptions.WorkFolder));
                ImageUtility.ViewFile(newImage);
            });
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            // Can only drop files, so check
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            _dragAndDropFileSelection.Clear();

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
                _dragAndDropFileSelection.Add(file);

            this.UserMessage($"File Selection: {_dragAndDropFileSelection.Count}");
            _dragAndDropFileSelection.ForEach(fileSelected =>
            {
                this.UserMessage("   "+fileSelected);
            });
        }
    }
}
