using System;
using System.IO;

namespace fAI.Util.Strings
{
    public static class FileUtil
    {

        public static string ImageToBase64Html(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException($"Image file not found: {imagePath}", imagePath);

            string extension = Path.GetExtension(imagePath).ToLowerInvariant();

            string mimeType;
            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    mimeType = "image/jpeg";
                    break;
                case ".png":
                    mimeType = "image/png";
                    break;
                default:
                    throw new NotSupportedException($"Unsupported image format '{extension}'. Only .jpg, .jpeg, and .png are supported.");
            }

            byte[] imageBytes = File.ReadAllBytes(imagePath);
            string base64Data = Convert.ToBase64String(imageBytes);

            return $"data:{mimeType};base64,{base64Data}";
        }

        public static string FileToBase64(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));

            if (!File.Exists(fileName))
                throw new FileNotFoundException("The specified file was not found.", fileName);

            byte[] fileBytes = File.ReadAllBytes(fileName);
            return Convert.ToBase64String(fileBytes);
        }
    }
}
