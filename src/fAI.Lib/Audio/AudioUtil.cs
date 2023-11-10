using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fAI
{
    public class AudioUtil
    {
        public class MP3Info
        {
            public int SampleRate { get; set; }
            public int BitsPerSample { get; set; }

            public int Duration { get; set; }
            public double DurationAsDouble { get; set; }
        }

        private static bool IsMp3File(string filename)
        {
            var ext = Path.GetExtension(filename).ToLower();
            return ext == ".mp3";
        }

        public static string GetTempFileName(string extension)
        {
            return Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + extension);
        }

        public static void DeleteFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }

        public static MP3Info GetWavInfo(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            if (ext != ".wav")
                throw new ArgumentException("File must be an MP3 file");

            using (var mp3 = new WaveFileReader(fileName))
            {
                return new MP3Info
                {
                    Duration = (int)mp3.TotalTime.TotalSeconds,
                    DurationAsDouble = mp3.TotalTime.TotalSeconds,
                    SampleRate = mp3.WaveFormat.SampleRate,
                    BitsPerSample = mp3.WaveFormat.BitsPerSample
                };
            }
        }


        public static MP3Info GetMp3Info(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLower();
            if(ext != ".mp3")
                throw new ArgumentException("File must be an MP3 file");

            using (var mp3 = new Mp3FileReader(fileName))
            {
                return new MP3Info 
                {
                    Duration = (int)mp3.TotalTime.TotalSeconds,
                    DurationAsDouble = mp3.TotalTime.TotalSeconds,
                    SampleRate = mp3.WaveFormat.SampleRate,
                    BitsPerSample = mp3.WaveFormat.BitsPerSample
                };
            }
        }
    }
}
