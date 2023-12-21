using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public static string GetNewImageFileName(ImageFormat imageFormat, string tmpFolder)
        {
            var folder = (string.IsNullOrEmpty(tmpFolder) ? Path.GetTempPath() : tmpFolder);
            return Path.Combine(folder, $"{Environment.TickCount}.{imageFormat}");
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
        public static bool ViewFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(filePath);
                return true;
            }
            else return false;
        }

        private static void DeleteFile(string file)
        {
            if (File.Exists(file))
                File.Delete(file);
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
            Fade6
        }

        public static bool GenerateGif(
            string gifName, 
            int delay, 
            GifTransitionMode gifTransitionMode, 
            List<string> imageFileNames, 
            bool _256ColorOptimization = false, 
            List<string> messages = null, 
            int messageX = -1, 
            int messageY = -1, 
            int fontSize = 64, 
            string fontName = "Consola")
        {
            DeleteFile(gifName);
            using (var collection = new MagickImageCollection())
            {
                var i = 0;
                var imagesCount = imageFileNames.Count;
                foreach (var fileName in imageFileNames)
                {
                    GenerateGifOneImage(messages[i], messageX, messageY, fontSize, fontName, collection, i, fileName, delay);

                    if (gifTransitionMode == GifTransitionMode.Fade1 || gifTransitionMode == GifTransitionMode.Fade6)
                    {
                        var fadingSteps = 6;
                        var fadingValue = 0.17f;
                        var imageIndexStartFading = i;
                        var fadeDelay = 16;
                        var imageIndexEndFading = i + 1;
                        if (i == imagesCount - 1)
                            imageIndexEndFading = 0; // Fade from the last one to the first one

                        if(gifTransitionMode == GifTransitionMode.Fade1)
                        {
                            fadingSteps = 1;
                            fadingValue = 0.6f;
                            fadeDelay = 40;
                        }

                        for (int j = 0; j < fadingSteps; j++)
                        {
                            var transitionImageFileName = BlendBitmaps(imageFileNames[imageIndexStartFading], imageFileNames[imageIndexEndFading], (j + 1) * fadingValue);
                            GenerateGifOneImage(messages[i], messageX, messageY, fontSize, fontName, collection, i, transitionImageFileName, fadeDelay);
                            DeleteFile(transitionImageFileName);
                        }
                    }
                  
                    i += 1;
                }

                if (_256ColorOptimization)
                {
                    // Lower quality and file size
                    var settings = new QuantizeSettings();
                    settings.Colors = 256;
                    collection.Quantize(settings);
                    collection.Optimize();
                }

                collection.Write(gifName);
            }

            return true;
        }

        private static void GenerateGifOneImage(string message, int messageX, int messageY, int fontSize, string fontName, MagickImageCollection collection, int i, string fileName, int animationDelay)
        {
            var (width, height) = GetImageWidthAndHeight(fileName);
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

            if (messageX == -1 && messageY == -1)
            {
                settings.Width = width;
                settings.Height = height;
            }

            MagickImage image = null;
            if (messageX == -1 && messageY == -1)
                image = new MagickImage(File.ReadAllBytes(fileName), settings);
            else
                image = new MagickImage(File.ReadAllBytes(fileName));

            if (message != null)
            {
                if (messageX == -1 && messageY == -1)
                    image.Annotate(message, Gravity.South);
                else
                    image.Draw(new Drawables().FillColor(MagickColors.White).Font(fontName).FontPointSize(fontSize).StrokeColor(MagickColors.Black).StrokeWidth(1).Text(messageX, messageY, message));
            }
            collection.Add(image);
            collection[collection.Count-1].AnimationDelay = animationDelay;
            collection[collection.Count - 1].GifDisposeMethod = GifDisposeMethod.Previous; // Prevents frames with transparent backgrounds from overlapping each other
        }
    }
}
