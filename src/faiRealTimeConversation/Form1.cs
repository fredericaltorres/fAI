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
        string _audioFileName;
        TestFileHelper _testFileHelper = new TestFileHelper();
        AudioHelper _audioHelper = null;

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

        bool closing = false;
        bool _recording = false;

        private async void butTalk_Click(object sender, EventArgs e)
        {
            if (_recording)
            {
                _audioHelper.StopRecording();
                this.UserMessage("Done recording");
                PlayAudio(_audioFileName);
                var r = await GetTextFromAudio();
                this.UserMessage(r.Text);
                _recording = false;
            }
            else
            {
                var deviceNumber = GetInputSelectedDeviceNumber();
                _audioHelper = new AudioHelper();
                _recording = true;
                _audioFileName = _testFileHelper.GetTempFileName(".faiRealTimeConversation.wav");
                _audioHelper.StartRecording(deviceNumber, _audioFileName);
                this.UserMessage("Recording");
            }
        }

        private async Task<DeepgramTranscriptionResult> GetTextFromAudio()
        {
            var client = new DeepgramAI();
            var r = await client.Audio.Transcriptions.CreateAsync(_audioFileName);
            return r;
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
