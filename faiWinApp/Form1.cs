
using DynamicSugar;
using ImageMagick;
using LogViewer.Net;
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
            _appOptions.GifFade1 = this.rdoGifFade1.Checked;
            _appOptions.GifFade6 = this.rdoGifFade6.Checked;
            _appOptions.GifDelay = this.txtGifDelay.Text;
            _appOptions.GifRepeat = this.txtGifRepeat.Text;
            _appOptions.ZoomIn = this.rdoZoomIn.Checked;
            _appOptions.ZoomInImageCount = this.txtZoomImageCount.Text;
            _appOptions.ViewImageAfterWork = this.chkViewFileAfterWork.Checked;
            
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
            using (var cursor = new CWaitCursor(this))
            {
                var newImageFile = ImageUtility.SaveImageFromClipboard(ImageFormat.Png, _appOptions.WorkFolder);
                if (newImageFile != null)
                {
                    this.UserMessage($"Saved image to {newImageFile}");
                    this.UserMessage(ImageUtility.GetImageInfo(newImageFile, _appOptions.WorkFolder));
                    ViewFile(newImageFile); _lastImageFile = newImageFile;
                    new ClipBoard().SetText(newImageFile);
                }
                else MessageBox.Show("No image found in clipboard.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _appOptions = AppOptions.FromFile($@".\{System.Windows.Forms.Application.ProductName}.config.json");
            this.WorkFolder.Text = _appOptions.WorkFolder;
            this.rdoGifFade1.Checked = _appOptions.GifFade1;
            this.rdoGifFade6.Checked = _appOptions.GifFade6;
            this.txtGifDelay.Text = _appOptions.GifDelay;
            this.txtGifRepeat.Text = _appOptions.GifRepeat;
            this.rdoZoomIn.Checked = _appOptions.ZoomIn;
            this.txtZoomImageCount.Text = _appOptions.ZoomInImageCount; 
            this.chkViewFileAfterWork.Checked = _appOptions.ViewImageAfterWork;

            this.UserMessage("Ready...");
        }

        private void sliceBy4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var cursor = new CWaitCursor(this))
            {
                var newImages = ImageUtility.SliceImageBy4(_lastImageFile, _appOptions.WorkFolder);
                newImages.ForEach(newImage =>
                {
                    this.UserMessage(ImageUtility.GetImageInfo(newImage, _appOptions.WorkFolder));
                    ViewFile(newImage);
                });
            }
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
            createGifAnimation(true);
        }

        private void createGifAnimation(bool viewFile)
        {
            using (var cursor = new CWaitCursor(this))
            {
                var transition = ImageUtility.GifTransitionMode.None;
                if (rdoGifFade1.Checked)
                    transition = ImageUtility.GifTransitionMode.Fade1;
                else if (rdoGifFade6.Checked)
                    transition = ImageUtility.GifTransitionMode.Fade6;
                else if (rdoZoomIn.Checked)
                    transition = ImageUtility.GifTransitionMode.ZoomIn;

                this.UserMessage($"Creating Gif, imageCount:{_dragAndDropFileSelection.Count}, transition:{transition}, output:{GifName}");
                var fileNames = _dragAndDropFileSelection.Select(file => Path.GetFileName(file)).ToList();
                fileNames = null;
                var r = ImageUtility.GenerateGif(
                        this.GifName,
                        int.Parse(this.txtGifDelay.Text),
                        int.Parse(this.txtGifRepeat.Text),
                        transition,
                        this._dragAndDropFileSelection,
                        messages: fileNames,
                        messageX: -1,
                        messageY: -1,
                        zoomImageCount: int.Parse(this.txtZoomImageCount.Text)
                );

                this.UserMessage($"Gif created: {r}");
                if(r.Success && viewFile)
                    this.ViewFile(this.GifName, displayImageInfo: false);
                ImageUtility.CleanUpTempFiles();
            }
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

        private void ViewFile(string fileName, bool displayImageInfo = true)
        {
            if (this.chkViewFileAfterWork.Checked)
            {
                if (displayImageInfo)
                    DisplayImageInfo(fileName);
                ImageUtility.ViewFile(fileName);
            }
        }

        public static void RenameFile(string file, string newName)
        {
            if (File.Exists(file))
                File.Move(file, newName);
        }

        public static void DeleteFile(string file)
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

        

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.txtUserOutput.Text = "";
            _dragAndDropFileSelection.Clear();
        }

        private void createGifAnimationZoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fileToZooms = _dragAndDropFileSelection.Clone();
            _dragAndDropFileSelection.Clear();

            foreach (var file in fileToZooms)
            {
                _dragAndDropFileSelection.Clear();
                _dragAndDropFileSelection.Add(file);
                createGifAnimation(false);
                var newGifName = Path.Combine(Path.GetDirectoryName(this.GifName), Path.GetFileNameWithoutExtension(file) + "." + Path.GetFileName(this.GifName));
                ImageUtility.RenameFile(this.GifName, newGifName);
                ImageUtility.ViewFile(newGifName);
            }
        }

        private void butOpenWorkFolder_Click(object sender, EventArgs e)
        {
            ImageUtility.OpenFolderInExplorer(_appOptions.WorkFolder);
        }
    }
}
