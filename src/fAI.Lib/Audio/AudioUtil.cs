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
            public string FileName { get; set; }

            public override string ToString()
            {
                return $"FileName:{this.FileName}, SampleRate:{this.SampleRate}, BitsPerSample:{this.BitsPerSample}, Duration(s):{this.Duration}, DurationAsDouble(s):{this.DurationAsDouble:0.00}";
            }
        }

        public static string ConcatenateWavFiles(string inputFilePath1, string inputFilePath2, string outputFilePath, bool asMp3 = true)
        {
            var wavFilePart1 = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".wav");
            try
            {
                using (var reader1 = new WaveFileReader(inputFilePath1))
                {
                    using (var reader2 = new WaveFileReader(inputFilePath2))
                    {
                        // Create a new WaveFormat with the same sample rate and channel count as the input files
                        var waveFormat = new WaveFormat(reader1.WaveFormat.SampleRate, reader1.WaveFormat.BitsPerSample, reader1.WaveFormat.Channels);

                        // Create a new WaveFileWriter to write the output file
                        using (var writer = new WaveFileWriter(wavFilePart1, waveFormat))
                        {
                            // Read and write data from the first input file
                            byte[] buffer1 = new byte[reader1.Length];
                            reader1.Read(buffer1, 0, buffer1.Length);
                            writer.Write(buffer1, 0, buffer1.Length);

                            // Read and write data from the second input file
                            byte[] buffer2 = new byte[reader2.Length];
                            reader2.Read(buffer2, 0, buffer2.Length);
                            writer.Write(buffer2, 0, buffer2.Length);
                        }
                    }
                }
                if (asMp3)
                {
                    var mp3Out = ConvertWavToMP3(wavFilePart1, outputFilePath); // Create MP3 file with the same name
                    return mp3Out;
                }
                else
                {
                    File.Copy(wavFilePart1, outputFilePath);
                    return outputFilePath;
                }
            }
            finally
            {
                DeleteFile(wavFilePart1);
            }
        }

        public static bool PlayMp3WithWindowsPlayer(string mp3FileName)
        {
            try
            {
                var proc = new System.Diagnostics.Process();
                proc.EnableRaisingEvents = false;
                proc.StartInfo.FileName = mp3FileName;
                proc.StartInfo.Arguments = "";
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
                return true;
            }
            catch
            {
                return false;
            }
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

        public static string ConvertMp3ToWav(string mp3FileName, string wavFileName = null)
        {
            if (wavFileName == null)
                wavFileName = Path.Combine(Path.GetTempPath(), Path.GetTempFileName() + ".wav");

            DeleteFile(wavFileName);

            using (var reader = new Mp3FileReader(mp3FileName))
            {
                WaveFileWriter.CreateWaveFile(wavFileName, reader);
            }
            return wavFileName;
        }

        public static string ConvertWavToMP3(string wavFileName, string mp3FileName = null)
        {
            if (mp3FileName == null)
                mp3FileName = wavFileName.Replace(".wav", ".mp3");

            DeleteFile(mp3FileName);

            using (var reader = new WaveFileReader(wavFileName))
            {
                MediaFoundationEncoder.EncodeToMp3(reader, mp3FileName, 44000);
            }
            return mp3FileName;
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
                    BitsPerSample = mp3.WaveFormat.BitsPerSample,
                    FileName = fileName
                };
            }
        }
    }
}
