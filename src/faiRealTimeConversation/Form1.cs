using DynamicSugar;
using fAI;
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
using static DynamicSugar.DS;
using static faiRealTimeConversation.AudioHelper;

namespace faiRealTimeConversation
{
    // https://markheath.net/post/30-days-naudio-docs
    public partial class Form1 : Form
    {
        string _recordedAudio;
        TestFileHelper _testFileHelper = new TestFileHelper();

        public Form1()
        {
            _testFileHelper.Clean();
            InitializeComponent();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
             this.UserMessage("Welcome to the fAI Real Time Conversation App");
            LoadInputDevices();
            LoadOutputDevices();
        }

        WaveFileWriter writer = null;
        bool closing = false;
        WaveInEvent waveIn;
        bool _recording = false;

        private async void butTalk_Click(object sender, EventArgs e)
        {
            if (_recording)
            {
                waveIn.StopRecording();
                this.UserMessage("Done recording");
                PlayAudio(_recordedAudio);
                var client = new DeepgramAI();
                var r = await client.Audio.Transcriptions.CreateAsync(_recordedAudio);
                this.UserMessage(r.Text);
            }
            else
            {
                var deviceNumber = GetInputSelectedDeviceNumber();
                _recording = true;
                _recordedAudio = _testFileHelper.GetTempFileName(".faiRealTimeConversation.wav");

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
                    if (closing)
                    {
                        waveIn.Dispose();
                    }
                };
                writer = new WaveFileWriter(_recordedAudio, waveIn.WaveFormat);
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

        private void LoadOutputDevices()
        {
            var outputDevices = AudioHelper.GetOutputDevices();

            var selectedInputDevice = outputDevices.FirstOrDefault(id => id.DeviceName.Contains("Focus")); //"Captain"
            if (selectedInputDevice == null)
                selectedInputDevice = outputDevices.FirstOrDefault(id => id.DeviceName.Contains("PnP Audio"));

            foreach (var id in outputDevices)
            {
                this.cboOutputDevices.Items.Add(id);
                var o = this.cboOutputDevices.Items[this.cboOutputDevices.Items.Count - 1];
                if (id == selectedInputDevice)
                    this.cboOutputDevices.SelectedItem = o;
            }
        }

        int GetInputSelectedDeviceNumber()
        {
            var d = this.cboInputDevices.SelectedItem as AudioHelper.AudioDevice;
            return d.DeviceNumber;
        }

        private void LoadInputDevices()
        {
            var inputDevices = AudioHelper.GetInputDevices();

            var selectedInputDevice = inputDevices.FirstOrDefault(id => id.DeviceName.Contains("Captain=========="));
            if (selectedInputDevice == null)
                selectedInputDevice = inputDevices.FirstOrDefault(id => id.DeviceName.Contains("PnP Audio"));

            foreach (var id in inputDevices)
            {
                this.cboInputDevices.Items.Add(id);
                var o = this.cboInputDevices.Items[this.cboInputDevices.Items.Count - 1];
                if(id == selectedInputDevice)
                    this.cboInputDevices.SelectedItem = o;
            }
        }

        public static bool PlayAudio(string filePath)
        {
            if (File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(filePath);
                return true;
            }
            else return false;
        }

        private async void butTest_Click(object sender, EventArgs e)
        {
            var client = new DeepgramAI();
            var r = await client.Audio.Transcriptions.CreateAsync(@"C:\temp\faiRealTimeConversation\recorded.mp3");
            this.UserMessage($"Text: {r.Duration }");
            this.UserMessage($"Text: {r.Text}");
        }
    }
}
