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
            System.Windows.Forms.Application.DoEvents();
        }

        private void pasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newImageFile = ImageUtility.SaveImageFromClipboard(ImageFormat.Png, _appOptions.WorkFolder);
            if(newImageFile != null)
            {
                this.UserMessage($"Saved image to {newImageFile}");
                this.UserMessage(ImageUtility.GetImageInfo(newImageFile, _appOptions.WorkFolder));
                ViewFile(newImageFile);
                _lastImageFile = newImageFile;
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




        string GigName => System.IO.Path.Combine(_appOptions.WorkFolder, "Animated.gif");

        private void createGifAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateGif2();
            return;
            using (MagickImageCollection collection = new MagickImageCollection())
            {
                int index = 0;
                var delay = 2 * 2000;
                _dragAndDropFileSelection.ForEach(imageFile => {
                    collection.Add(imageFile);
                    collection[index].AnimationDelay = delay;
                    index += 1;
                });
                
                //// Add second image, set the animation delay to 100ms and flip the image
                //collection.Add("Snakeware.png");
                //collection[1].AnimationDelay = 100;
                //collection[1].Flip();

                // Optionally reduce colors
                var settings = new QuantizeSettings();
                //settings.Colors = 256;
                collection.Quantize(settings);
                collection.Optimize();
                if (File.Exists(GigName))
                    File.Delete(GigName);
                collection.Write(GigName);
            }
        }

        public bool GenerateGif2()
        {
            DeleteFile(GigName);
            using (var collection = new MagickImageCollection())
            {
                var i = 0;
                foreach (var fileName in _dragAndDropFileSelection)
                {
                    var image = new MagickImage(File.ReadAllBytes(fileName));
                    //image.Draw(new Drawables().FontPointSize(72).Text(128, 128 * 2, sub));
                    collection.Add(image);
                    collection[i].AnimationDelay = 100;
                    i += 1;
                }

                //var settings = new QuantizeSettings();
                //settings.Colors = 256;
                //collection.Quantize(settings);
                //collection.Optimize();

                collection.Write(GigName);
                this.UserMessage($"Created {GigName}");
                ViewFile(GigName);
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
            ImageUtility.ViewFile(GigName);
        }

        private void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        private string testString = "Hello, world!";
        public bool NewInstance()
        {
            DeleteFile(GigName);
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
                collection.Write(GigName);
                this.UserMessage($"Created {GigName}");
                ViewFile(GigName);
            }

            return true;
        }

        private void tESTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewInstance();
        }
    }
}
