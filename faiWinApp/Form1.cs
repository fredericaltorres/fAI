﻿using DynamicSugar;
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
using static fAI.LeonardoImage;
using static System.Net.Mime.MediaTypeNames;

namespace faiWinApp
{

    public partial class Form1 : Form
    {
        private string _lastImageFile = null;
        AppOptions _appOptions = new AppOptions();
        List<string> _dragAndDropFileSelection = new List<string>()
        {

        };

        public Form1()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _appOptions.PasteFileName = this.PasteFileName.Text;
            _appOptions.WorkFolder = this.WorkFolder.Text;
            _appOptions.Mp4FirstFrameDuration = this.mp4FirstFrameDurationSecond.Text;
            _appOptions.mp4FrameRate = this.mp4FrameRate.Text;
            _appOptions.mp4ZoomPercent = this.mp4ZoomPercent.Text;
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
                var newImageFile = ImageUtility.SaveImageFromClipboard(ImageFormat.Png, _appOptions.WorkFolder, _appOptions.PasteFileName);
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
            this.PasteFileName.Text = _appOptions.PasteFileName;
            this.rdoGifFade1.Checked = _appOptions.GifFade1;
            this.rdoGifFade6.Checked = _appOptions.GifFade6;
            this.mp4FirstFrameDurationSecond.Text = _appOptions.Mp4FirstFrameDuration;
            this.mp4FrameRate.Text = _appOptions.mp4FrameRate;
            this.mp4ZoomPercent.Text = _appOptions.mp4ZoomPercent;
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

            if (_isCtrlDown)
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
            });
        }

        string FinalOutputFileName => Path.Combine(_appOptions.WorkFolder, "Animated." + (this._appOptions.GenerateMP4 ? "mp4" : "gif"));
        //string PngName => Path.Combine(_appOptions.WorkFolder, "Output.png");

        private void createGifAnimationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createGifAnimation(true);
        }

        private int GetGifDelayInRigthUnit() => int.Parse(this.txtGifDelay.Text) * 100;
        private int GetMp4FrameRate() => int.Parse(this.mp4FrameRate.Text);

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

                this.UserMessage($"Creating {(generateMP4 ? "MP4" : "GIF")}, imageCount:{_dragAndDropFileSelection.Count}, transition:{transition}, output:{FinalOutputFileName}");
                var fileNames = _dragAndDropFileSelection.Select(file => Path.GetFileName(file)).ToList();
                fileNames = null;
                var r = ImageUtility.GenerateGif(
                        notify,
                        this.FinalOutputFileName,
                        this.GetGifDelayInRigthUnit(),
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

        private int GetMp4ZoomPercent()
        {
            if (string.IsNullOrEmpty(this.mp4ZoomPercent.Text))
                return -1;

            return int.Parse(this.mp4ZoomPercent.Text);
        }

        private int GetTxtZoomImageCount()
        {
            if (string.IsNullOrEmpty(this.txtZoomImageCount.Text))
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
            catch (Exception ex)
            {
                this.UserMessage($"Error: {ex.Message}");
            }
            finally
            {
                if (image != null)
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
                    image.Draw(new Drawables().FontPointSize(72).Text(128, 128 * 2, sub));
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
            if (show)
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

        private void gIFToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void womanOverTimeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.UserMessage("[Leonardo]Generating images, Woman over time...");
            var prompt = @"
Close-up portrait BLONDE WOMAN {age} years old, nice body shape,|(STYLED HAIR:1.7), color portrait, Linkedin profile picture, professional portrait photography by Martin Schoeller, by Mark Mann, by Steve McCurry, bokeh, studio lighting, canonical lens, shot on dslr, 64 megapixels, sharp focus.
";
            var workFolder = @"C:\temp\@fAiImages\Leonardo.Woman.Life";
            var ages = new List<int>() { 10, 20, 30, 40, 50, 60, 70, 80 };
            var finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: true);
            var client = new fAI.Leonardo();

            foreach (var age in ages)
            {
                this.UserMessage("[Leonardo]Age:{age}".Template(new { age }));
                var fileName = client.Image.GenerateSync(prompt.Template(new { age }),
                                                         size: fAI.OpenAIImage.ImageSize._768x1360, seed: 689242880);
                finalOutputFiles.AddFile(fileName, move: true);
            }
        }

        private void PasteFileName_TextChanged(object sender, EventArgs e)
        {
            _appOptions.PasteFileName = this.PasteFileName.Text;
        }

        private void butSortFileNames_Click(object sender, EventArgs e)
        {
            _dragAndDropFileSelection.Sort();
            _dragAndDropFileSelection.ForEach(fileSelected =>
            {
                DisplayImageInfo(fileSelected);
            });
        }

        private void DarkwaveSoundscape_CreateImages()
        {
            this.UserMessage("[Leonardo]Generating images, Woman over time...");
            var prompt = @"
In the depths of a darkwave soundscape, a celestial being emerges, embodying the essence of Mars while emanating an ethereal aura. Captured in an oil painting, this image encapsulates the mysterious and otherworldly nature of the heavenly martian. Every brushstroke reveals a macabre beauty, with their midnight black wings gently unfurled against the backdrop of a crimson sky. Gleaming silver veins intricately trace over their translucent skin, contrasting with their phosphorescent green eyes, which seem to hold the secrets of ancient galaxies. This meticulously crafted masterpiece, rendered with impeccable detail, conveys a sense of enigmatic allure and invites the viewers to contemplate the enigmatic allure of the celestial realm.
";
            var workFolder = @"C:\temp\@fAiImages\Darkwave.Soundscape";
            var startSeed = 935939584;
            var imageCount = 50;
            var finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false);
            finalOutputFiles.CreateDirectory(workFolder);
            finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false, sequence: finalOutputFiles.FileNames.Count);


            var client = new fAI.Leonardo();

            for (var i = 30; i < imageCount; i++)
            {
                var seed = startSeed + i;
                this.UserMessage("[Leonardo]Age:{seed}".Template(new { seed }));
                var fileName = client.Image.GenerateSync(prompt,
                                                         size: fAI.OpenAIImage.ImageSize._768x1360,
                                                         seed: seed, photoReal: true, stableDiffusionVersion: StableDiffusionVersion.v2_1,
                                                         presetStylePhotoRealOn: PresetStylePhotoRealOn.CINEMATIC);
                finalOutputFiles.AddFile(fileName, move: true);
            }
        }

        private void createImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DarkwaveSoundscape_CreateImages();
        }

        private void buildVideoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\Darkwave.Soundscape\sequence.md";
            BuildVideo(sequenceFileName);
        }

        private void BuildVideo(string sequenceFileName)
        {
            Action<string> notify = (m) => this.UserMessage(m);
            var finalOutputFiles = new FileSequenceManager();
            var error = finalOutputFiles.LoadSequenceFile(sequenceFileName, true);
            ImageUtility.GenerateMP4Animation(notify,
                finalOutputFiles.FileNames,
                this.FinalOutputFileName,
                mp4FrameRate: GetMp4FrameRate(),
                imageDurationSecond: GetMp4FirstFrameDurationSecond(),
                zoomInPercent: GetMp4ZoomPercent());

            ViewFile(this.FinalOutputFileName);
        }

        private void createImageToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Decadence_CreateImages();
        }

        private void Decadence_CreateImages()
        {
            this.UserMessage("[Leonardo]Generating images, Decadence...");
            var prompt = @"
a manifestation of the decadence of the human being, nature losing against humans, tragedy, pain
";
            var workFolder = @"C:\temp\@fAiImages\Decadence";
            var startSeed = 318635520;
            var imageCount = 32;
            var finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false);
            finalOutputFiles.CreateDirectory(workFolder);
            finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false, sequence: finalOutputFiles.FileNames.Count);
            var startImageIndex = finalOutputFiles.FileNames.Count;
            var client = new fAI.Leonardo();

            for (var i = startImageIndex; i < imageCount; i++)
            {
                var seed = startSeed + i;
                this.UserMessage($"[Leonardo]Age:{seed}");
                var fileName = client.Image.GenerateSync(prompt,
                                                         size: fAI.OpenAIImage.ImageSize._1024x576,
                                                         seed: seed, 
                                                         photoReal: true, 
                                                         stableDiffusionVersion: StableDiffusionVersion.v2_1,
                                                         presetStylePhotoRealOn: PresetStylePhotoRealOn.CINEMATIC);
                finalOutputFiles.AddFile(fileName, move: true);
            }
        }

        private void buildVideoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\Decadence.Manual\sequence.md";
            BuildVideo(sequenceFileName);
        }

        private void createImageToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.UserMessage("[Leonardo]Generating images, Boss Room...");
            var prompt = @"final boss room entrance from dark fantasy";
            var workFolder = @"C:\temp\@fAiImages\BossRoom";
            var startSeed = 344178432;//756848896;
            var imageCount = 32;
            var finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false);
            finalOutputFiles.CreateDirectory(workFolder);
            finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false, sequence: finalOutputFiles.FileNames.Count);
            var startImageIndex = finalOutputFiles.FileNames.Count;
            var client = new fAI.Leonardo();

            for (var i = startImageIndex; i < imageCount; i++)
            {
                var seed = startSeed + i;
                this.UserMessage($"[Leonardo]Seed:{seed}");
                var fileName = client.Image.GenerateSync(prompt,
                                                         size: fAI.OpenAIImage.ImageSize._1024x576,
                                                         seed: seed,
                                                         photoReal: true,
                                                         promptMagic: false,
                                                         isPublic: false,
                                                         stableDiffusionVersion: StableDiffusionVersion.v0_9,
                                                         scheduler: Scheduler.LEONARDO,
                                                         presetStylePhotoRealOn: PresetStylePhotoRealOn.CINEMATIC);
                finalOutputFiles.AddFile(fileName, move: true);
            }
        }

        private void buildVideoBossRoom_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\BossRoom\sequence.md";
            Action<string> notify = (m) => this.UserMessage(m);
            var finalOutputFiles = new FileSequenceManager();
            var error = finalOutputFiles.LoadSequenceFile(sequenceFileName, true);
            ImageUtility.GenerateMP4Animation(notify,
                finalOutputFiles.FileNames,
                this.FinalOutputFileName,
                transistionDurationSecond: 2,
                mp4FrameRate: GetMp4FrameRate(),
                imageDurationSecond: GetMp4FirstFrameDurationSecond(),
                zoomInPercent: GetMp4ZoomPercent());

            ViewFile(this.FinalOutputFileName);
        }

        private void getLeonardoImageGenerationInfoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var client = new fAI.Leonardo();

            // var generation1 = client.Image.GetGenerationsById("cd31b33b-4d52-448d-abf9-1598c55415b2");
            var models = client.Image.GetModels();
            models.ForEach(m => this.UserMessage(m.ToString()));

            var elements = client.Image.GetElements();
            var generation = client.Image.GetGenerationsByUserId(null);
            var tfh = new TestFileHelper();
            var jsonFile = tfh.CreateTempFile(generation.Json, ".json");
            this.UserMessage($"Json file: {jsonFile}");
        }

        private void celestialShimmeringCreateImage_Click(object sender, EventArgs e)
        {
            this.UserMessage("[Leonardo]Generating images, Celestial shimmering...");
            var prompt = @"
A delicately shimmering celestial artifact captured in a surreal pinhole photograph, the image main subject is a bright red translucent, ethereal figure with intricately flowing lines reminiscent of a ghostly apparition. This stunning photograph, taken with exceptional precision, showcases the artifact's enchanting radiance, refracting soft hues of opalescent light. The image possesses an otherworldly allure, inviting viewers to ponder the mystical origins of this captivating digital masterpiece, 4K.
";
            var workFolder = @"C:\temp\@fAiImages\CelestialShimmering";
            var startSeed = 351786496;
            var imageCount = 32;
            var finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false);
            finalOutputFiles.CreateDirectory(workFolder);
            finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false, sequence: finalOutputFiles.FileNames.Count);
            var startImageIndex = finalOutputFiles.FileNames.Count;
            var client = new fAI.Leonardo();
            var modelName = "Leonardo Diffusion XL";

            var elements = client.Image.GetElements("Glasscore");

            for (var i = startImageIndex; i < imageCount; i++)
            {
                var seed = startSeed + i-1;
                this.UserMessage($"[Leonardo]Age:{seed}");
                var fileName = client.Image.GenerateSync(prompt,
                                                         size: fAI.OpenAIImage.ImageSize._768x1360,
                                                         modelName: modelName,
                                                         seed: seed,
                                                         promptMagic: false,
                                                         photoReal: !true, // photoReal v1
                                                         promptMagicVersion:  null,
                                                         promptMagicStrength: null,
                                                         stableDiffusionVersion: StableDiffusionVersion.v0_9,
                                                         presetStyleAlchemyOn: PresetStyleAlchemyOn.CINEMATIC,
                                                         presetStylePhotoRealOn: PresetStylePhotoRealOn.CINEMATIC,
                                                         elements: elements,
                                                         elementWeight: -0.6
                                                         );
                finalOutputFiles.AddFile(fileName, move: true);
            }
        }

        private void buildVideoCS_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\CelestialShimmering\sequence.md";
            Action<string> notify = (m) => this.UserMessage(m);
            var finalOutputFiles = new FileSequenceManager();
            var error = finalOutputFiles.LoadSequenceFile(sequenceFileName, true);
            ImageUtility.GenerateMP4Animation(notify,
                finalOutputFiles.FileNames,
                this.FinalOutputFileName,
                transistionDurationSecond: 2,
                mp4FrameRate: GetMp4FrameRate(),
                imageDurationSecond: GetMp4FirstFrameDurationSecond(),
                zoomInPercent: GetMp4ZoomPercent());

            ViewFile(this.FinalOutputFileName);
        }

        private void mysticalCreatureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\mystical creature\sequence.md";
            Action<string> notify = (m) => this.UserMessage(m);
            var finalOutputFiles = new FileSequenceManager();
            var error = finalOutputFiles.LoadSequenceFile(sequenceFileName, true);
            ImageUtility.GenerateMP4Animation(notify,
                finalOutputFiles.FileNames,
                this.FinalOutputFileName,
                transistionDurationSecond: 2,
                mp4FrameRate: GetMp4FrameRate(),
                imageDurationSecond: GetMp4FirstFrameDurationSecond(),
                zoomInPercent: GetMp4ZoomPercent());

            ViewFile(this.FinalOutputFileName);
        }

        private void darkAndEerieWorldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\dark and eerie world\sequence.md";
            Action<string> notify = (m) => this.UserMessage(m);
            var finalOutputFiles = new FileSequenceManager();
            var error = finalOutputFiles.LoadSequenceFile(sequenceFileName, true);
            ImageUtility.GenerateMP4Animation(notify,
                finalOutputFiles.FileNames,
                this.FinalOutputFileName,
                transistionDurationSecond: 2,
                mp4FrameRate: GetMp4FrameRate(),
                imageDurationSecond: GetMp4FirstFrameDurationSecond(),
                zoomInPercent: GetMp4ZoomPercent());

            ViewFile(this.FinalOutputFileName);
        }

        private void womanCucumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\Woman-Cucumber\sequence.md";
            Action<string> notify = (m) => this.UserMessage(m);
            var finalOutputFiles = new FileSequenceManager();
            var error = finalOutputFiles.LoadSequenceFile(sequenceFileName, true);
            ImageUtility.GenerateMP4Animation(notify,
                finalOutputFiles.FileNames,
                this.FinalOutputFileName,
                transistionDurationSecond: 2,
                mp4FrameRate: GetMp4FrameRate(),
                imageDurationSecond: GetMp4FirstFrameDurationSecond(),
                zoomInPercent: GetMp4ZoomPercent());

            ViewFile(this.FinalOutputFileName);
        }

        private void createImageToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            this.UserMessage("[Leonardo]Generating Rail way track");
            var prompt = @"
 realistic, Image: Creepy old railway tracks leading into dense, misty woods--q 2 --v 5.2 --ar 16:9
";
            var workFolder = @"C:\temp\@fAiImages\RailWayTrack";
            int startSeed = 629455447;
            var imageCount = 64;
            var finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false);
            finalOutputFiles.CreateDirectory(workFolder);
            finalOutputFiles = new FileSequenceManager(workFolder, reCreateIfExists: false, sequence: finalOutputFiles.FileNames.Count);
            var startImageIndex = finalOutputFiles.FileNames.Count;
            var client = new fAI.Leonardo();
            var modelName = "AlbedoBase XL";
            //var elements = client.Image.GetElements("Glasscore");

            for (var i = startImageIndex; i < imageCount; i++)
            {
                var seed = startSeed + i - 1;
                this.UserMessage($"[Leonardo]Age:{seed}");
                var fileName = client.Image.GenerateSync(prompt,
                                                         size: fAI.OpenAIImage.ImageSize._768x1360,
                                                         modelName: modelName,
                                                         seed: seed,
                                                         promptMagic: false,
                                                         photoReal: false, // photoReal v1
                                                         promptMagicVersion: null,
                                                         promptMagicStrength: null,
                                                         stableDiffusionVersion: StableDiffusionVersion.v0_9,
                                                         presetStyleAlchemyOn: PresetStyleAlchemyOn.CINEMATIC,
                                                         presetStylePhotoRealOn: PresetStylePhotoRealOn.LEONARDO //,
                                                         //elements: elements,
                                                         //elementWeight: -0.6
                                                         );
                finalOutputFiles.AddFile(fileName, move: true);
            }
        }

        private void buildVideoToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var sequenceFileName = @"C:\temp\@fAiImages\RailWayTrack\sequence.md";
            Action<string> notify = (m) => this.UserMessage(m);
            var finalOutputFiles = new FileSequenceManager();
            var error = finalOutputFiles.LoadSequenceFile(sequenceFileName, true);
            ImageUtility.GenerateMP4Animation(notify,
                finalOutputFiles.FileNames,
                this.FinalOutputFileName,
                transistionDurationSecond: 2,
                mp4FrameRate: GetMp4FrameRate(),
                imageDurationSecond: GetMp4FirstFrameDurationSecond(),
                zoomInPercent: GetMp4ZoomPercent());

            ViewFile(this.FinalOutputFileName);
        }
    }
}
/*
         {
            "generated_images": [
                {
                    "url": "https://cdn.leonardo.ai/users/bda69047-2232-48b9-b4ac-2c43394e3f2e/generations/022b9d37-a150-4008-a20c-8615b7f38458/Default_A_delicately_shimmering_celestial_artifact_captured_in_0.jpg",
                    "nsfw": false,
                    "id": "c4d6b633-a0bd-498a-a53d-e6c3c106276e",
                    "likeCount": 0,
                    "motionMP4URL": null,
                    "generated_image_variation_generics": []
                }
            ],
            "modelId": "1e60896f-3c26-4296-8ecc-53e2afecc132",
            "motion": null,
            "motionModel": null,
            "motionStrength": null,
            "prompt": "A delicately shimmering celestial artifact captured in a surreal pinhole photograph, the image main subject is a bright red translucent, ethereal figure with intricately flowing lines reminiscent of a ghostly apparition. This stunning photograph, taken with exceptional precision, showcases the artifact's enchanting radiance, refracting soft hues of opalescent light. The image possesses an otherworldly allure, inviting viewers to ponder the mystical origins of this captivating digital masterpiece, 4K.",
            "negativePrompt": "",
            "imageHeight": 1360,
            "imageToVideo": null,
            "imageWidth": 768,
            "inferenceSteps": null,
            "seed": 351786496,
            "public": true,
            "scheduler": "LEONARDO",
            "sdVersion": "SDXL_0_9",
            "status": "COMPLETE",
            "presetStyle": "CINEMATIC",
            "initStrength": null,
            "guidanceScale": null,
            "id": "022b9d37-a150-4008-a20c-8615b7f38458",
            "createdAt": "2024-03-12T03:55:35.235",
            "promptMagic": false,
            "promptMagicVersion": null,
            "promptMagicStrength": null,
            "photoReal": true,
            "photoRealStrength": null,
            "fantasyAvatar": null,
            "generation_elements": [
                {
                    "id": 18280693,
                    "weightApplied": -0.6,
                    "lora": {
                        "akUUID": "a699f5da-f7f5-4afe-8473-c426b245c145",
                        "baseModel": "SDXL_1_0",
                        "description": "Craft realistic glass objects. Use \"glass\" in the prompt with Leonardo Vision XL for best results.",
                        "name": "Glasscore",
                        "urlImage": "https://cdn.leonardo.ai/element_thumbnails/a699f5da-f7f5-4afe-8473-c426b245c145/thumbnail.png",
                        "weightDefault": 1.0,
                        "weightMax": 2.0,
                        "weightMin": -2.0
                    }
                }
            ]
        },
 */