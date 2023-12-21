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

        public static bool GenerateGif(string gifName, List<string> imageFileNames, bool _256ColorOptimization = false, 
            List<string> messages = null, int messageX = -1, int messageY = -1, int fontSize = 64, string fontName = "Consola")
        {
            DeleteFile(gifName);
            using (var collection = new MagickImageCollection())
            {
                var i = 0;
                foreach (var fileName in imageFileNames)
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

                    if (messages != null)
                    {
                        // https://legacy.imagemagick.org/discourse-server/viewtopic.php?t=36435
                        var message = messages[i];
                        //image.Draw(new Drawables().Color( ).Font(fontName).FontPointSize(72).Text(messageX, messageY, message));

                        if(messageX ==-1 && messageY == -1)
                        {
                            image.Annotate(message, Gravity.South);
                        }
                        else
                        {
                            //IMagickGeometry boundingArea = new MagickGeometry(messageX, messageY, messageX+100, messageY+100);
                            //image.Annotate(message, boundingArea);
                            image.Draw(new Drawables().FillColor(MagickColors.White)
                                                      .Font(fontName)
                                                      .FontPointSize(fontSize)
                                                      .StrokeColor(MagickColors.Black)
                                                      .StrokeWidth(1)
                                                      .Text(messageX, messageY, message));
                        }
                    }
                    collection.Add(image);
                    collection[i].AnimationDelay = 100;
                    collection[i].GifDisposeMethod = GifDisposeMethod.Previous; // Prevents frames with transparent backgrounds from overlapping each other

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
    }
}
