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
    }
}
