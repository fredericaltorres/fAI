using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace faiRealTimeConversation
{
    // https://markheath.net/post/30-days-naudio-docs
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
             this.UserMessage("Welcome to the fAI Real Time Conversation App");
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            var outputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            var inputDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            for (int i = -1; i < NAudio.Wave.WaveIn.DeviceCount; i++)
            {
                var caps = NAudio.Wave.WaveIn.GetCapabilities(i);
                Console.WriteLine($"{i}: {caps.ProductName}");
            }
        }

        WaveFileWriter writer = null;
        bool closing = false;
        WaveInEvent waveIn;
        bool _recording = false;

        private void butTalk_Click(object sender, EventArgs e)
        {
            if (_recording)
            {
                waveIn.StopRecording();
                this.UserMessage("Done recording");
            }
            else
            {
                _recording = true;
                var outputFolder = Path.Combine(@"c:\temp", "faiRealTimeConversation");
                Directory.CreateDirectory(outputFolder);
                var outputFilePath = Path.Combine(outputFolder, "recorded.wav");
                if(waveIn == null)
                {
                    //var cap = NAudio.Wave.WaveIn.GetCapabilities(1);
                    //waveIn = new WaveInEvent();

                    waveIn = new NAudio.Wave.WaveInEvent
                    {
                        DeviceNumber = -1,
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
                    if (closing)
                    {
                        waveIn.Dispose();
                    }
                };
                writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
                waveIn.StartRecording();
                this.UserMessage("Recording");
            }
        }


        private void UserMessage(string m)
        {
            this.txtUserOutput.Text += $"{m}{(m.EndsWith(Environment.NewLine) ? "" : Environment.NewLine)}";
            this.txtUserOutput.SelectionStart = this.txtUserOutput.TextLength;
            this.txtUserOutput.ScrollToCaret();
            System.Windows.Forms.Application.DoEvents();
        }
    }
}
