using System;
using System.IO;

namespace fAI.Util.Strings
{
    public static class FileUtil
    {
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
