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

        public static string GetNewImageFileName(ImageFormat imageFormat, string tmpFolder)
        {
            var folder = (string.IsNullOrEmpty(tmpFolder) ? Path.GetTempPath() : tmpFolder);
            var f = Path.Combine(folder, $"{Environment.TickCount}.{imageFormat}");
            _fileToCleanUp.Add(f);
            return f;
        }

        public static string SaveImageFromClipboard(ImageFormat imageFormat, string tmpFolder)
        {
            if (Clipboard.ContainsImage())
            {
                string filePath = GetNewImageFileName(imageFormat, tmpFolder);
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

        public static string BlendBitmaps(string bitmap1, string bitmap2, float blendLevel)
        {
            var b = BlendBitmaps(new Bitmap(bitmap1), new Bitmap(bitmap2), blendLevel);
            var fileName = GetNewImageFileName(ImageFormat.Png, Path.GetTempPath());
            b.Save(fileName);
            return fileName;
        }

        public static Bitmap BlendBitmaps(Bitmap bitmap1, Bitmap bitmap2, float blendLevel)
        {
            Bitmap blendedBitmap = new Bitmap(bitmap1.Width, bitmap1.Height);

            using (Graphics g = Graphics.FromImage(blendedBitmap))
            {
                g.DrawImage(bitmap1, 0, 0);

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
                g.DrawImage(bitmap2, new Rectangle(0, 0, bitmap2.Width, bitmap2.Height), 0, 0, bitmap2.Width, bitmap2.Height, GraphicsUnit.Pixel, imageAttributes);
            }

            return blendedBitmap;
        }

        public enum GifTransitionMode
        {
            None,
            Fade1,
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

        /// <summary>
        /// https://ezgif.com/maker/ezgif-4-5112dde2-gif
        /// </summary>
        /// <returns></returns>
        public static GeneratedGif GenerateGif(
            string gifName, 
            int delay, 
            int repeat,
            GifTransitionMode gifTransitionMode, 
            List<string> imageFileNames, 
            bool _256ColorOptimization = false, 
            List<string> messages = null, 
            int messageX = -1, 
            int messageY = -1, 
            int fontSize = 64, 
            string fontName = "Consola", 
            int zoomImageCount = 32,
            int zoomImagePixelStep = 1,
            bool generateMP4 = false) // Pixels
        {
            var (refWidth, refHeight) = GetImageWidthAndHeight(imageFileNames[0]);
            var rr = new GeneratedGif { GifName = gifName, Width = refWidth, Height = refHeight };
            int duration = 0;
            DeleteFile(gifName);
            using (var collection = new MagickImageCollection())
            {
                var imageIndex = 0;
                var imagesCount = imageFileNames.Count;
                foreach (var fileName in imageFileNames)
                {
                    if (gifTransitionMode == GifTransitionMode.ZoomIn)
                        delay = delay / 4;

                    var message = messages == null ? null : messages[imageIndex];

                    // Generate the main image
                    GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, fileName, delay, refWidth, refHeight);
                 

                    if (gifTransitionMode == GifTransitionMode.ZoomIn)
                    {
                        var zoomPixelStep = 3; // Pixels
                        var zoomPixel = zoomPixelStep;
                        for (var pZoom = 1; pZoom <= zoomImageCount; pZoom++)
                        {
                            var newZoomedImage = ZoomIntoBitmap(fileName, new Rectangle(zoomPixel, zoomPixel, refWidth - zoomPixel*2, refHeight - zoomPixel * 2));
                            GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, newZoomedImage, delay, refWidth, refHeight);
                            zoomPixel += zoomPixelStep;
                        }
                    }
                    else if (gifTransitionMode == GifTransitionMode.Fade1 || gifTransitionMode == GifTransitionMode.Fade6)
                    {
                        var fadingSteps = 6;
                        var fadingValue = 0.17f;
                        var imageIndexStartFading = imageIndex;
                        var fadeDelay = 16;
                        var imageIndexEndFading = imageIndex + 1;
                        if (imageIndex == imagesCount - 1)
                            imageIndexEndFading = 0; // Fade from the last one to the first one

                        if(gifTransitionMode == GifTransitionMode.Fade1)
                        {
                            fadingSteps = 1;
                            fadingValue = 0.6f;
                            fadeDelay = delay/4;
                        }

                        for (int j = 0; j < fadingSteps; j++)
                        {
                            var transitionImageFileName = BlendBitmaps(imageFileNames[imageIndexStartFading], imageFileNames[imageIndexEndFading], (j + 1) * fadingValue);
                            GenerateGifOneImage(message, messageX, messageY, fontSize, fontName, collection, imageIndex, transitionImageFileName, fadeDelay, refWidth, refHeight);
                        }
                    }
                  
                    imageIndex += 1;
                }

                var settings = new QuantizeSettings();
                settings.DitherMethod = DitherMethod.FloydSteinberg;
                if (_256ColorOptimization)
                {
                    settings.Colors = 256;
                }
                collection.Quantize(settings);
                collection.Optimize();

                rr.ImageCount = collection.Count;
                rr.Duration = collection.Sum(i => i.AnimationDelay);

                if (repeat == 1)
                {
                    collection[0].AnimationIterations = 1;
                    //collection.ToList().ForEach(action: i => i.AnimationIterations = 1);
                }

                collection.Write(gifName);

                //using (var animatedGif = new MagickImageCollection(gifName))
                //{
                //    animatedGif.Write($"{gifName}.webp", MagickFormat.WebP);
                //}
            }

            return rr;
        }

        private static void GenerateGifOneImage(string message, int messageX, int messageY, int fontSize, string fontName, MagickImageCollection collection, int i, string fileName, int animationDelay, int refWidth, int refHeight)
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
                image = new MagickImage(File.ReadAllBytes(fileName), settings);
            else
                image = new MagickImage(File.ReadAllBytes(fileName));

            //image.HasAlpha = true;

            if (message != null)
            {
                if (messageX == -1 && messageY == -1)
                    image.Annotate(message, Gravity.South);
                else
                    image.Draw(new Drawables().FillColor(MagickColors.White).Font(fontName).FontPointSize(fontSize).StrokeColor(MagickColors.Black).StrokeWidth(1).Text(messageX, messageY, message));
            }
            image.Draw(new Drawables().FillColor(MagickColors.White).Font(fontName).FontPointSize(22).StrokeColor(MagickColors.Black).StrokeWidth(1).Text(8, height - 16, imageNumber.ToString()));
            collection.Add(image);
            collection[collection.Count-1].AnimationDelay = animationDelay;
            collection[collection.Count - 1].GifDisposeMethod = GifDisposeMethod.Previous; // Prevents frames with transparent backgrounds from overlapping each other
        }
    }
}
