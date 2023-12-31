﻿
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

        };

        public Form1()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _appOptions.WorkFolder = this.WorkFolder.Text;
            _appOptions.Mp4FirstFrameDuration = this.mp4FirstFrameDurationSecond.Text;
            _appOptions.GifFade1 = this.rdoGifFade1.Checked;
            _appOptions.GifFade6 = this.rdoGifFade6.Checked;
            _appOptions.GifDelay = this.txtGifDelay.Text;
            _appOptions.GifRepeat = this.cbGifRepeat.Checked;
            _appOptions.GenerateMP4 = this.ckGenerateMP4.Checked;
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
            PasteImage(true);
        }

        private void PasteImage(bool viewfile)
        {
            using (var cursor = new CWaitCursor(this))
            {
                var newImageFile = ImageUtility.SaveImageFromClipboard(ImageFormat.Png, _appOptions.WorkFolder);
                if (newImageFile != null)
                {
                    this.UserMessage($"Saved image to {newImageFile}");
                    this.UserMessage(ImageUtility.GetImageInfo(newImageFile, _appOptions.WorkFolder));
                    _lastImageFile = newImageFile;
                    if (this.chkViewFileAfterWork.Checked && viewfile)
                    {
                        ViewFile(newImageFile); 
                    }
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
            this.mp4FirstFrameDurationSecond.Text = _appOptions.Mp4FirstFrameDuration;
            this.txtGifDelay.Text = _appOptions.GifDelay;
            this.cbGifRepeat.Checked = _appOptions.GifRepeat;
            this.ckGenerateMP4.Checked = _appOptions.GenerateMP4;
            this.rdoZoomIn.Checked = _appOptions.ZoomIn;
            this.txtZoomImageCount.Text = _appOptions.ZoomInImageCount; 
            this.chkViewFileAfterWork.Checked = _appOptions.ViewImageAfterWork;

            this.UserMessage("Ready...");
        }

        private void sliceBy4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteImage(false);
            using (var cursor = new CWaitCursor(this))
            {
                var newImages = ImageUtility.SliceImageBy4(_lastImageFile, _appOptions.WorkFolder);
                new TestFileHelper().DeleteFile(_lastImageFile);
                newImages.ForEach(newImage =>
                {
                    this.UserMessage(ImageUtility.GetImageInfo(newImage, _appOptions.WorkFolder));
                    ViewFile(newImage);
                });
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
            // e.AllowedEffect = DragDropEffects.Copy | DragDropEffects.Move;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            // Can only drop files, so check
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            if(_isCtrlDown)
            {
                _dragAndDropFileSelection.Clear();
            }

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var file in files)
            {
                _dragAndDropFileSelection.Add(file);
                
            }

            this.UserMessage(Environment.NewLine);
            this.UserMessage($"File Selection: {_dragAndDropFileSelection.Count}");
            _dragAndDropFileSelection.ForEach(fileSelected =>
            {
                DisplayImageInfo(fileSelected);
                // this.UserMessage("   "+fileSelected);
            });
        }

        string FinalOutputFileName => Path.Combine(_appOptions.WorkFolder, "Animated." + (this._appOptions.GenerateMP4 ? "mp4" : "gif"));
        //string PngName => Path.Combine(_appOptions.WorkFolder, "Output.png");

        private void createGifAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
          

            createGifAnimation(true);
        }

        private void createGifAnimation(bool viewFile)
        {
            var generateMP4 = this.ckGenerateMP4.Checked;
            Action<string> notify = (m) => this.UserMessage(m);
            using (var cursor = new CWaitCursor(this))
            {
                var transition = ImageUtility.GifTransitionMode.None;
                if (rdoGifFade1.Checked)
                    transition = ImageUtility.GifTransitionMode.Fade2;
                else if (rdoGifFade6.Checked)
                    transition = ImageUtility.GifTransitionMode.Fade6;
                else if (rdoZoomIn.Checked)
                    transition = ImageUtility.GifTransitionMode.ZoomIn;

                this.UserMessage($"Creating {(generateMP4 ? "MP4":"GIF")}, imageCount:{_dragAndDropFileSelection.Count}, transition:{transition}, output:{FinalOutputFileName}");
                var fileNames = _dragAndDropFileSelection.Select(file => Path.GetFileName(file)).ToList();
                fileNames = null;
                var r = ImageUtility.GenerateGif(
                        notify,
                        this.FinalOutputFileName,
                        int.Parse(this.txtGifDelay.Text),
                        this.cbGifRepeat.Checked,
                        transition,
                        this._dragAndDropFileSelection,
                        messages: fileNames,
                        messageX: -1,
                        messageY: -1,
                        zoomImageCount: this.GetTxtZoomImageCount(),
                        generateMP4: generateMP4,
                        mp4FirstFrameDurationSecond: GetMp4FirstFrameDurationSecond()
                );

                if (r.Success)
                {
                    this.UserMessage($"Gif created: {r}");
                    if (r.Success && viewFile)
                        this.ViewFile(this.FinalOutputFileName, displayImageInfo: false);
                }
                else
                {
                    ShowError(r.Exception.Message);
                }
                ImageUtility.CleanUpTempFiles();
            }
        }
        
        private void ShowError(string message)
        {
            MessageBox.Show(message);
        }

        private int GetMp4FirstFrameDurationSecond()
        {
            if (string.IsNullOrEmpty(this.mp4FirstFrameDurationSecond.Text))
                return 1;

            return int.Parse(this.mp4FirstFrameDurationSecond.Text);
        }

        private int GetTxtZoomImageCount ()
        {
            if(string.IsNullOrEmpty(this.txtZoomImageCount.Text))
                return 0;

            return int.Parse(this.txtZoomImageCount.Text);
        }

        private void DisplayImageInfo(string fileName)
        {
            var len = new FileInfo(fileName).Length / 1024.0 / 1024.0;
            Bitmap image = null;
            try
            {
                if (Path.GetExtension(fileName).ToLower() == ".mp4")
                {
                    this.UserMessage($"Image:{Path.GetFileName(fileName)}, size:{len:0.0} Mb");
                }
                else
                {
                    image = new Bitmap(fileName);
                    int width = image.Width;
                    int height = image.Height;
                    PixelFormat pixelFormat = image.PixelFormat;
                    this.UserMessage($"Image:{Path.GetFileName(fileName)}, size:{len:0.0} Mb, width: {width}, height: {height}, pixelFormat: {pixelFormat}");
                }
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
            DeleteFile(FinalOutputFileName);
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
                collection.Write(FinalOutputFileName);
                this.UserMessage($"Created {FinalOutputFileName}");
                ViewFile(FinalOutputFileName);
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
                var newGifName = Path.Combine(Path.GetDirectoryName(this.FinalOutputFileName), Path.GetFileNameWithoutExtension(file) + "." + Path.GetFileName(this.FinalOutputFileName));
                ImageUtility.RenameFile(this.FinalOutputFileName, newGifName);
                ImageUtility.ViewFile(newGifName);
            }
        }

        private void butOpenWorkFolder_Click(object sender, EventArgs e)
        {
            ImageUtility.OpenFolderInExplorer(_appOptions.WorkFolder);
        }

        bool _isCtrlDown = false;
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            var show = _isCtrlDown != e.Control;
            _isCtrlDown = e.Control;
            if(show)
                this.UserMessage($"Ctrl:{_isCtrlDown}"); 
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            var show = _isCtrlDown != e.Control;
            _isCtrlDown = e.Control;
            if (show)
                this.UserMessage($"Ctrl:{_isCtrlDown}");
        }

        private void createGifAnimationToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            createGifAnimation(true);
        }

        private void loopMp4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsSelectionAllMp4)
            {
                var newFiles = new List<string>();
                var tfh = new TestFileHelper();
                var tmpPath = tfh.GetTempFolder(); // Let;s not delete this folder for now
                var sequence = 0;
                foreach (var mp4 in _dragAndDropFileSelection)
                {
                    var ff = Path.Combine(tmpPath, $"{sequence:00000}.mp4");
                    sequence++;
                    this.UserMessage($"Converting {mp4} to {ff}");
                    var r = ImageUtility.LoopMp4(mp4, ff, 2);
                    newFiles.Add(ff);
                }
                _dragAndDropFileSelection = newFiles;
            }
        }

        private void concatMP4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsSelectionAllMp4)
            {
                this.UserMessage($"Concatening {_dragAndDropFileSelection.Count} mp4 files");
                var outputFileName = ImageUtility.ConcatMp4(_dragAndDropFileSelection, FinalOutputFileName);
                if (File.Exists(outputFileName))
                {
                    ViewFile(outputFileName);
                }
            }
        }
        private bool IsSelectionAllMp4 => _dragAndDropFileSelection.Count == _dragAndDropFileSelection.Where(f => Path.GetExtension(f).ToLower() == ".mp4").ToList().Count;
    }
}
