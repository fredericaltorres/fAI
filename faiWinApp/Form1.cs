
using ImageMagick;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace faiWinApp
{

    public partial class Form1 : Form
    {
        private string _lastImageFile = null;
        AppOptions _appOptions = new AppOptions();
        List<string> _dragAndDropFileSelection = new List<string>() {
            @"C:\DVT\AI\images\549902968.Png",
            @"C:\DVT\AI\images\549897484.Png"
        };

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
            System.Windows.Forms.Application.DoEvents();
        }

        private void pasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newImageFile = ImageUtility.SaveImageFromClipboard(ImageFormat.Png, _appOptions.WorkFolder);
            if(newImageFile != null)
            {
                this.UserMessage($"Saved image to {newImageFile}");
                this.UserMessage(ImageUtility.GetImageInfo(newImageFile, _appOptions.WorkFolder));
                ViewFile(newImageFile);                _lastImageFile = newImageFile;
            }
            else MessageBox.Show("No image found in clipboard.");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _appOptions = AppOptions.FromFile($@".\{System.Windows.Forms.Application.ProductName}.config.json");
            this.WorkFolder.Text = _appOptions.WorkFolder;
            this.UserMessage("Ready...");
        }

        private void sliceBy4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newImages = ImageUtility.SliceImageBy4(_lastImageFile, _appOptions.WorkFolder);
            newImages.ForEach(newImage =>
            {
                this.UserMessage(ImageUtility.GetImageInfo(newImage, _appOptions.WorkFolder));
                ViewFile(newImage);
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
            {
                _dragAndDropFileSelection.Add(file);
                DisplayImageInfo(file);
            }

            this.UserMessage($"File Selection: {_dragAndDropFileSelection.Count}");
            _dragAndDropFileSelection.ForEach(fileSelected =>
            {
                this.UserMessage("   "+fileSelected);
            });
        }




        string GifName => System.IO.Path.Combine(_appOptions.WorkFolder, "Animated.gif");
        string PngName => System.IO.Path.Combine(_appOptions.WorkFolder, "Output.png");

        private void createGifAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileNames = _dragAndDropFileSelection.Select(file => Path.GetFileName(file)).ToList();
            ImageUtility.GenerateGif(this.GifName, this._dragAndDropFileSelection, messages: fileNames, messageX: -1, messageY: -1);
            //ImageUtility.GenerateGif(this.GifName, this._dragAndDropFileSelection, messages: fileNames, messageX: 80, messageY: 100);
            this.ViewFile(this.GifName);
        }

        public bool GenerateMosaic()
        {
            DeleteFile(PngName);
            using (var collection = new MagickImageCollection())
            {
                var i = 0;
                foreach (var fileName in _dragAndDropFileSelection)
                {
                    var image = new MagickImage(File.ReadAllBytes(fileName));
                    collection.Add(image);
                    i += 1;
                }

                using (var result = collection.Mosaic())
                {
                    result.Write(PngName);
                }
                    

                //collection.Write(PngName);

                this.UserMessage($"Created {PngName}");
                ViewFile(PngName);
            }

            return true;
        }



        private void DisplayImageInfo(string fileName)
        {

            var len = new FileInfo(fileName).Length / 1024.0 / 1024.0;
            Bitmap image = null;
            try
            {
                image = new Bitmap(fileName);

                int width = image.Width;
                int height = image.Height;
                PixelFormat pixelFormat = image.PixelFormat;
                //int colorDepth =  System.Drawing.Image.GetPixelFormatSize(pixelFormat);
                this.UserMessage($"Image:{Path.GetFileName(fileName)}, size:{len:0.0} Mb, width: {width}, height: {height}, pixelFormat: {pixelFormat}");
            }
            catch(Exception ex)
            {
                this.UserMessage($"Error: {ex.Message}");
            }
            finally
            {
                if(image != null)
                    image.Dispose();
            }
        }

        private void ViewFile(string fileName)
        {
            DisplayImageInfo(fileName);
            ImageUtility.ViewFile(fileName);
        }

        private void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        private string testString = "Hello, world!";

        public bool NewInstance()
        {
            DeleteFile(GifName);
            using (var collection = new MagickImageCollection())
            {
                for (var i = 1; i < testString.Length; i++)
                {
                    var sub = testString.Substring(0, i);
                    var image = new MagickImage(File.ReadAllBytes(@"C:\DVT\AI\images\549902968.Png"));
                    image.Draw(new Drawables().FontPointSize(72).Text(128, 128*2, sub));
                    collection.Add(image);
                    collection[i - 1].AnimationDelay = 20;
                }
                collection.Write(GifName);
                this.UserMessage($"Created {GifName}");
                ViewFile(GifName);
            }

            return true;
        }

        private void tESTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewInstance();
        }

        private void mosaicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateMosaic();
        }
    }
}
