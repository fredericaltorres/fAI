using ImageMagick;
using System;
using DynamicSugar;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.CodeDom.Compiler;

namespace faiWinApp
{
    public class ImageUtility
    {
        public static List<string> SliceImageBy4(string originalImage, string tmpFolder)
        {
            var r = new List<string>();
            r.Add(SliceImageBy4_TopRight(originalImage, tmpFolder));
            r.Add(SliceImageBy4_BottomRight(originalImage, tmpFolder));
            r.Add(SliceImageBy4_TopLeft(originalImage, tmpFolder));
            r.Add(SliceImageBy4_BottomLeft(originalImage, tmpFolder));
            return r;
        }

        public static string SliceImageBy4_BottomRight(string filePath, string tmpFolder)
        {
            string originalImagePath = filePath;
            string newImagePath = GetNewImageFileName(ImageFormat.Png, tmpFolder);

            using (Bitmap originalImage = new Bitmap(originalImagePath))
            {
                int width = originalImage.Width / 2;
                int height = originalImage.Height / 2; // Quarter height
                int startY = originalImage.Height - height; // Starting Y-coordinate for the bottom quarter
                int startX = originalImage.Width - width;

                Rectangle cropArea = new Rectangle(startX, startY, width, height);

                using (Bitmap croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat))
                {
                    croppedImage.Save(newImagePath); // Save the bottom quarter of the image
                }
                return newImagePath;
            }
        }

        public static string SliceImageBy4_TopRight(string filePath, string tmpFolder)
        {
            string originalImagePath = filePath;
            string newImagePath = GetNewImageFileName(ImageFormat.Png, tmpFolder);

            using (Bitmap originalImage = new Bitmap(originalImagePath))
            {
                int width = originalImage.Width / 2;
                int height = originalImage.Height / 2;
                int startX = originalImage.Width - width;

                var cropArea = new Rectangle(startX, 0, width, height);

                using (Bitmap croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat))
                {
                    croppedImage.Save(newImagePath); // Save the top quarter of the image
                }
                return newImagePath;
            }
        }

        public static string SliceImageBy4_TopLeft(string filePath, string tmpFolder)
        {
            string originalImagePath = filePath;
            string newImagePath = GetNewImageFileName(ImageFormat.Png, tmpFolder);

            using (Bitmap originalImage = new Bitmap(originalImagePath))
            {
                int width = originalImage.Width / 2;
                int height = originalImage.Height / 2;

                var cropArea = new Rectangle(0, 0, width, height);

                using (Bitmap croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat))
                {
                    croppedImage.Save(newImagePath); // Save the top quarter of the image
                }
                return newImagePath;
            }
        }

        public static string SliceImageBy4_BottomLeft(string filePath, string tmpFolder)
        {
            string originalImagePath = filePath;
            string newImagePath = GetNewImageFileName(ImageFormat.Png, tmpFolder);

            using (Bitmap originalImage = new Bitmap(originalImagePath))
            {
                int width = originalImage.Width / 2;
                int height = originalImage.Height / 2; // Quarter height
                int startY = originalImage.Height - height; // Starting Y-coordinate for the bottom quarter

                Rectangle cropArea = new Rectangle(0, startY, width, height);

                using (Bitmap croppedImage = originalImage.Clone(cropArea, originalImage.PixelFormat))
                {
                    croppedImage.Save(newImagePath); // Save the bottom quarter of the image
                }
                return newImagePath;
            }
        }


        public static (int, int) GetImageWidthAndHeight(string filePath)
        {
            var sb = new StringBuilder();
            using (Image img = Image.FromFile(filePath))
            {
                return (img.Width, img.Height);
            }
        }

        public static string GetImageInfo(string filePath, string tmpFolder)
        {
            var sb = new StringBuilder();
            using (Image img = Image.FromFile(filePath))
            {
                int width = img.Width;
                int height = img.Height;
                
                sb.AppendLine($"File: {filePath}");
                sb.AppendLine($"Width: {width}");
                sb.AppendLine($"Height: {height}");
            }
            return sb.ToString();
        }

        static List<string> _fileToCleanUp = new List<string>();

        public static void CleanUpTempFiles()
        {
            foreach(var f in _fileToCleanUp)
                DeleteFile(f);
            _fileToCleanUp.Clear();
        }

        public static string GetNewImageFileName(ImageFormat imageFormat, string tmpFolder, string imageFileNamePrefix = "")
        {
            var folder = (string.IsNullOrEmpty(tmpFolder) ? Path.GetTempPath() : tmpFolder);
            var f = Path.Combine(folder, $"{imageFileNamePrefix}{(string.IsNullOrEmpty(imageFileNamePrefix) ? "" : "." )}{Environment.TickCount}.{imageFormat}");
            _fileToCleanUp.Add(f);
            return f;
        }

        public static string SaveImageFromClipboard(ImageFormat imageFormat, string tmpFolder, string imageFileNamePrefix)
        {
            if (Clipboard.ContainsImage())
            {
                string filePath = GetNewImageFileName(imageFormat, tmpFolder, imageFileNamePrefix);
                Image img = Clipboard.GetImage();
                img.Save(filePath, imageFormat);
                return filePath;
            }
            else return null;
        }

        public static bool OpenFolderInExplorer(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
                return true;
            }
            else return false;
        }

        public static bool ViewFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(filePath);
                return true;
            }
            else return false;
        }

        public static void RenameFile(string file, string newName)
        {
            DeleteFile(newName);
            if (File.Exists(file))
                File.Move(file, newName);
        }

        public static void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
        }

        public static Bitmap ZoomIntoBitmap_old(Bitmap originalBitmap, Rectangle zoomArea)
        {
            var zoomedBitmap = new Bitmap(zoomArea.Width, zoomArea.Height); // Create a new bitmap with the same size as the zoom area

            using (var graphics = Graphics.FromImage(zoomedBitmap))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic; // Set high-quality processing
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // Draw the zoomed area
                graphics.DrawImage(originalBitmap, new Rectangle(0, 0, zoomedBitmap.Width, zoomedBitmap.Height), zoomArea, GraphicsUnit.Pixel);
            }

            return zoomedBitmap;
        }
        public static Bitmap ZoomIntoBitmap(Bitmap originalBitmap, Rectangle zoomArea)
        {
            var zoomedBitmap = new Bitmap(originalBitmap.Width, originalBitmap.Height); // Create a new bitmap with the same size as the zoom area

            using (var graphics = Graphics.FromImage(zoomedBitmap))
            {
                graphics.InterpolationMode  = InterpolationMode.HighQualityBicubic; // Set high-quality processing
                graphics.SmoothingMode      = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode    = PixelOffsetMode.HighQuality;
                graphics.CompositingQuality = CompositingQuality.HighQuality;

                // Draw the zoomed area
                graphics.DrawImage(originalBitmap, new Rectangle(0, 0, originalBitmap.Width, originalBitmap.Height), zoomArea, GraphicsUnit.Pixel);
            }

            return zoomedBitmap;
        }

        public static string ZoomIntoBitmap(string fileName, Rectangle zoomArea)
        {
            Bitmap originalBitmap = new Bitmap(fileName);
            var b = ZoomIntoBitmap(originalBitmap, zoomArea);
            var newFileName = GetNewImageFileName(ImageFormat.Png, Path.GetTempPath());
            b.Save(newFileName);
            return newFileName;
        }

        public static string BlendBitmaps(string imageFileName1, string imageFileName2, float blendLevel)
        {
            var b1 = new Bitmap(imageFileName1);
            var b2 = new Bitmap(imageFileName2);
            var b = BlendBitmaps(b1, b2, blendLevel);
            var fileName = GetNewImageFileName(ImageFormat.Png, Path.GetTempPath());
            b.Save(fileName);
            return fileName;
        }

        public static Bitmap BlendBitmaps(Bitmap bitmap1, Bitmap bitmap2, float blendLevel)
        {
            var blendedBitmap = new Bitmap(bitmap1.Width, bitmap1.Height);

            using (var g = Graphics.FromImage(blendedBitmap))
            {
                g.DrawImage(bitmap1, 0, 0, bitmap1.Width, bitmap1.Height);

                ColorMatrix colorMatrix = null;

                colorMatrix = new ColorMatrix(new float[][] {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 0, 0},
                    new float[] {0, 0, 1, 0, 0},
                    new float[] {0, 0, 0, blendLevel, 0}, // Change alpha to blend
                    new float[] {0, 0, 0, 0, 1}
                });

                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;

                ImageAttributes imageAttributes = new ImageAttributes();
                imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                var rect = new Rectangle(0, 0, bitmap2.Width, bitmap2.Height);
                g.DrawImage(bitmap2, rect, 0, 0, bitmap2.Width, bitmap2.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            return blendedBitmap;
        }

        public enum GifTransitionMode
        {
            None,
            Fade2,
            Fade6,
            ZoomIn,
        }

        public class GeneratedGif
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public string GifName { get; set; }
            public int ImageCount { get; set; }
            public long ImageSizeMB { get; set; }
            public int Duration { get; set; }
            public Exception Exception { get; set; }
            public bool Success => Exception == null;

            public override string ToString()
            {
                this.ImageSizeMB = new FileInfo(this.GifName).Length / 1024 / 1024; 
                return DS.Dictionary(this).Format();
            }
        }

        private static void RemoveReadOnlyAttribute(string fileName)
        {
            if (File.Exists(fileName))
            {
                var f = new FileInfo(fileName);
                if (f.IsReadOnly)
                    f.IsReadOnly = false;
            }
        }

        static string ffmpeg = @"C:\Brainshark\scripts\ffmpeg\v4.3.1\bin\ffmpeg.exe";

        public static bool RunFFMPEG(List<string> pngFiles, string mp4OutputFile, int mp4FrameRate = 16)
        {
            var fileList = string.Join("\r\n", pngFiles.Select(f => $"file '{f}'"));
            var tfh = new TestFileHelper();
            var inputFileName = tfh.CreateFile(fileList, "txt");

            var tmpPath = "";
            var cmd = $@"-y -framerate {mp4FrameRate} -f concat -i ""{inputFileName}"" -c:v libx264 -r 30 -pix_fmt yuv420p ""{mp4OutputFile}""";
            var intExitCode = RunFFMPEG(cmd, mp4OutputFile);
            return intExitCode == 0;
        }

        public static int RunFFMPEG(string cmd, string mp4OutputFile)
        {
            DeleteFile(mp4OutputFile);
            var intExitCode = 0;
            var r = Executorr.ExecProgram(ffmpeg, cmd, true, ref intExitCode, true);
            return intExitCode;
        }

        public static int LoopMp4(string mp4IutputFile, string mp4OutputFile, int loopCount)
        {
            return RunFFMPEG($@"-stream_loop {loopCount-1} -i ""{mp4IutputFile}"" -c copy ""{mp4OutputFile}"" ", mp4OutputFile);
        }

        public static string ConcatMp4(List<string> mp4IutputFile, string mp4OutputFile)
        {
            var tfh = new TestFileHelper();
            var sb = new StringBuilder();
            mp4IutputFile.ForEach(f => sb.AppendLine($"file {f.Replace(@"\",@"/")}"));
            var mp4FileList = tfh.CreateTempFile(sb.ToString(),"txt");
            
            var newMp4 = tfh.GetTempFileName("mp4");
            var r = RunFFMPEG($@" -safe 0 -f concat -i ""{mp4FileList}"" -c copy ""{mp4OutputFile}"" ", mp4OutputFile);
            tfh.DeleteFile(mp4FileList);
            if (r == 0)
                return mp4OutputFile;
            else
                return null;
        }

        /// <summary>
        /// https://ezgif.com/maker/ezgif-4-5112dde2-gif
        /// </summary>
        /// <returns></returns>
        public static GeneratedGif GenerateGif(
            Action<string> notify,
            string gifName,
            int delay,
            bool repeat,
            GifTransitionMode gifTransitionMode,
            List<string> imageFileNames,
            bool _256ColorOptimization = false,
            List<string> messages = null,
            int messageX = -1,
            int messageY = -1,
            int fontSize = 64,
            string fontName = "Consola", 
            int zoomImageCount = 32,
            int zoomPixelStep = 1,
            int mp4FrameRate = 16,
            int mp4FirstFrameDurationSecond = 2,
            bool generateMP4 = false) // Pixels
        {
            var (refWidth, refHeight) = GetImageWidthAndHeight(imageFileNames[0]);
            var rr = new GeneratedGif { GifName = gifName, Width = refWidth, Height = refHeight };
            int duration = 0;
            DeleteFile(gifName);
            var imagesGenerated = new List<string>();

            Func<string, int, string> Img = (f, count) =>
            {
                for(var i = 0; i < count; i++)
                    imagesGenerated.Add(f);
                return f;
            };

            using (var collection = new MagickImageCollection())
            {
                var imageIndex = 0;
                var imagesCount = imageFileNames.Count;
                var zoomDelay = delay / 10;

                foreach (var fileName in imageFileNames)
                {
                    notify($"Processing {imageIndex} / {imageFileNames.Count}");
                    var message = messages == null ? null : messages[imageIndex];

                    if (gifTransitionMode == GifTransitionMode.ZoomIn)
                    {
                        // Generate the main image
                        GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, Img(fileName, 1), zoomDelay, refWidth, refHeight);

                        var zoomPixel = zoomPixelStep;
                        for (var pZoom = 1; pZoom <= zoomImageCount; pZoom++)
                        {
                            var newZoomedImage = Img(ZoomIntoBitmap(fileName, new Rectangle(zoomPixel, zoomPixel, refWidth - (zoomPixel * 2), refHeight - (zoomPixel * 2))),1);
                            GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, newZoomedImage, zoomDelay, refWidth, refHeight);
                            zoomPixel += zoomPixelStep;
                        }
                    }
                    else if (gifTransitionMode == GifTransitionMode.Fade2 || gifTransitionMode == GifTransitionMode.Fade6)
                    {
                        if(generateMP4) // Generate a wait of 2 second for the MP4
                        {
                            var duplicateCount = mp4FrameRate * mp4FirstFrameDurationSecond;
                            // Generate the main image for x second in mnp4 mode
                            GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, Img(fileName, duplicateCount), delay, refWidth, refHeight, 
                                duplicate: duplicateCount);
                        }
                        else
                        {
                            // Generate the main image
                            GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, Img(fileName, 1), delay, refWidth, refHeight);
                        }

                        // GifTransitionMode.Fade6
                        var fadingSteps = mp4FrameRate;
                        var fadingValue = 0.9f / fadingSteps;
                        var imageIndexStartFading = imageIndex;
                        var fadeDelay = 16;
                        var imageIndexEndFading = imageIndex + 1;
                        if (imageIndex == imagesCount - 1)
                            imageIndexEndFading = 0; // Fade from the last one to the first one

                        if (gifTransitionMode == GifTransitionMode.Fade2)
                        {
                            fadingSteps = 4;
                            fadingValue = 0.3f;
                            fadeDelay = 22;
                        }

                        for (int j = 0; j < fadingSteps; j++)
                        {
                            var transitionImageFileName = Img(BlendBitmaps(imageFileNames[imageIndexStartFading], imageFileNames[imageIndexEndFading], (j + 1) * fadingValue), 1);
                            GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, transitionImageFileName, fadeDelay, refWidth, refHeight, 
                                                duplicate: mp4FrameRate/2);
                        }
                    }

                    imageIndex += 1;
                }

                rr.ImageCount = collection.Count;
                rr.Duration = collection.Sum(i => i.AnimationDelay);

                if (generateMP4)
                {
                    notify($"Generating MP4");
                    var sequence = 0;
                    using (var tfh = new TestFileHelper())
                    {
                        var tmpPath = tfh.GetTempFolder();
                        var imageVdoFileNamesSequenced = new List<string>();
                        imagesGenerated.ForEach(f =>
                        {
                            var ff = Path.Combine(tmpPath, $"{sequence:00000}.png");
                            File.Copy(f, ff);
                            imageVdoFileNamesSequenced.Add(ff);
                            sequence++;
                        });

                        var vdoFileName = Path.ChangeExtension(gifName, "mp4");
                        var cmd = $@"-y -framerate {mp4FrameRate} -i ""{tmpPath}\%05d.png"" -start_number 0 -c:v libx264 -r 30 -pix_fmt yuv420p ""{vdoFileName}""";
                        var intExitCode = RunFFMPEG(cmd, vdoFileName);
                        if (intExitCode != 0)
                            rr.Exception = new Exception($"ffmpeg failed with exit code {intExitCode}");

                        imageVdoFileNamesSequenced.ForEach(f =>
                        {
                            RemoveReadOnlyAttribute(f);
                            tfh.FileNamesToDelete.Add(f);
                        });
                    }
                }
                else
                {
                    var settings = new QuantizeSettings();
                    settings.DitherMethod = DitherMethod.FloydSteinberg;
                    if (_256ColorOptimization)
                    {
                        settings.Colors = 256;
                    }
                    collection.Quantize(settings);
                    collection.Optimize();

                    if (!repeat)
                        collection[0].AnimationIterations = 1;

                    collection.Write(gifName);
                 }

                //using (var animatedGif = new MagickImageCollection(gifName))
                //{
                //    animatedGif.Write($"{gifName}.webp", MagickFormat.WebP);
                //}
            }

            return rr;
        }

        private static void GenerateGifOneImage(string message, int messageX, int messageY, int fontSize, string fontName, MagickImageCollection collection, int i, string fileName, int animationDelay, int refWidth, int refHeight, int duplicate = 1)
        {
            int imageNumber = collection.Count;
            var (width, height) = GetImageWidthAndHeight(fileName);
            if(width != refWidth || height != refHeight)
            {
                // Resize
                var newFileName = GetNewImageFileName(ImageFormat.Png, Path.GetTempPath());
                var nImage = new MagickImage(File.ReadAllBytes(fileName));
                nImage.Resize(refWidth, refHeight);
                nImage.Write(newFileName);
                fileName = newFileName;
            }

            var settings = new MagickReadSettings()
            {
                FillColor = MagickColors.White, // Text in white with a black stroke
                StrokeColor = MagickColors.Black,
                Font = fontName,
                FontStyle = FontStyleType.Normal,
                FontPointsize = fontSize,
                Width = width,
                Height = height,
            };


            MagickImage image = null;
            if (messageX == -1 && messageY == -1)
                //image = new MagickImage(File.ReadAllBytes(fileName), settings);
                image = new MagickImage(fileName, settings);
            else
                //image = new MagickImage(File.ReadAllBytes(fileName));
                image = new MagickImage(fileName);

            //image.HasAlpha = true;

            if (message != null)
            {
                if (messageX == -1 && messageY == -1)
                    image.Annotate(message, Gravity.South);
                else
                    image.Draw(new Drawables().FillColor(MagickColors.White).Font(fontName).FontPointSize(fontSize).StrokeColor(MagickColors.Black).StrokeWidth(1).Text(messageX, messageY, message));
            }
            image.Draw(new Drawables().FillColor(MagickColors.White).Font(fontName).FontPointSize(26).StrokeColor(MagickColors.Black).StrokeWidth(1).Text(8, height - 16, imageNumber.ToString()));

            for (var j = 0; j < duplicate; j++)
            {
                var image2 = new MagickImage(image);
                collection.Add(image2);
                collection[collection.Count - 1].AnimationDelay = animationDelay;
                collection[collection.Count - 1].GifDisposeMethod = GifDisposeMethod.Previous; // Prevents frames with transparent backgrounds from overlapping each other
            }
        }
    }
}
