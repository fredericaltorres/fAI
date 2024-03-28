using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace faiRealTimeConversation
{
    public class AudioHelper
    {
        public class AudioDevice
        {
            public int DeviceNumber { get; set; }
            public string DeviceName { get; set; }
            public int Channels { get; set; }

            public override string ToString()
            {
                return $"{DeviceNumber}: {DeviceName}";
            }
        }

        public static List<AudioDevice> GetInputDevices()
        {
            var devices = new List<AudioDevice>();
            for (int i = -1; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                var caps = NAudio.Wave.WaveIn.GetCapabilities(i);
                devices.Add(new AudioDevice { DeviceNumber = i, DeviceName = caps.ProductName, Channels = caps.Channels });
            }
            return devices;
        }

        public static List<AudioDevice> GetOutputDevices()
        {
            var devices = new List<AudioDevice>();
            for (int i = -1; i < NAudio.Wave.WaveOut.DeviceCount; i++)
            {
                var caps = NAudio.Wave.WaveOut.GetCapabilities(i);
                devices.Add(new AudioDevice { DeviceNumber = i, DeviceName = caps.ProductName, Channels = caps.Channels });
            }
            return devices;
        }

        WaveFileWriter writer = null;
        WaveInEvent waveIn;


        public void StopRecording()
        {
            waveIn.StopRecording();
        }

        public void StartRecording(int deviceNumber, string audioFilename)
        {
            if (waveIn == null)
            {
                waveIn = new NAudio.Wave.WaveInEvent
                {
                    DeviceNumber = deviceNumber,
                    WaveFormat = new NAudio.Wave.WaveFormat(rate: 44100, bits: 16, channels: 1),
                    BufferMilliseconds = 40
                };
            }
            waveIn.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 16)
                {
                    waveIn.StopRecording();
                }
            };
            waveIn.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
                writer = null;
                waveIn.Dispose();
                //if (closing)
                //{

                //}
            };
            writer = new WaveFileWriter(audioFilename, waveIn.WaveFormat);
            waveIn.StartRecording();
        }
    }
}
