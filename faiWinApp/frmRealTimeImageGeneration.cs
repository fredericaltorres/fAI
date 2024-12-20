using DynamicSugar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static fAI.LeonardoImage;

namespace faiWinApp
{
    public partial class frmRealTimeImageGeneration : Form
    {
        string _workFolder = @"c:\temp\fai.RealTimeImageGeneration";
        FileSequenceManager _finalOutputFiles;
        fAI.Leonardo _leoClient = new fAI.Leonardo();

        List<CustomModel> _customModels;

        public frmRealTimeImageGeneration()
        {
            InitializeComponent();
        }

        private void UserMessage(string m)
        {
            this.txtUserOutput.Text += $"{m}{(m.EndsWith(Environment.NewLine) ? "" : Environment.NewLine)}";
            this.txtUserOutput.SelectionStart = this.txtUserOutput.TextLength;
            this.txtUserOutput.ScrollToCaret();
            System.Windows.Forms.Application.DoEvents();
        }

        private void frmRealTimeImageGeneration_Load(object sender, EventArgs e)
        {
            _finalOutputFiles = new FileSequenceManager(_workFolder, reCreateIfExists: false);
            _finalOutputFiles.CreateDirectory(_workFolder);
            _finalOutputFiles = new FileSequenceManager(_workFolder, reCreateIfExists: false, sequence: _finalOutputFiles.FileNames.Count);

            _customModels = _leoClient.Image.GetModels();

            foreach (var m in _customModels)
                this.cboModels.Items.Add(m);

            this.cboModels.SelectedIndex = 0;

            this.UserMessage("Ready...");
        }

        private CustomModel GetSelectedModel()
        {
            return (CustomModel)this.cboModels.SelectedItem;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }




        private void renderImage_Click(object sender, EventArgs e)
        {
            this.UserMessage($"Rendering image... Model:{GetSelectedModel().name}, prompt: {this.txtPrompt.Text}");
            
            var prompt = $"{this.txtPrompt.Text}";
            var startSeed = 123456789;
            var modelName = GetSelectedModel().name;

            try
            {
                var fileName = _leoClient.Image.GenerateSync(prompt,
                    modelName: modelName,
                    size: fAI.OpenAIImage.ImageSize._512x512,
                    seed: startSeed,
                    photoReal: true,
                    stableDiffusionVersion: StableDiffusionVersion.v2_1,
                    presetStylePhotoRealOn: PresetStylePhotoRealOn.CINEMATIC,
                    timeoutManagerSleepTime: 3);

                _finalOutputFiles.AddFile(fileName);
                this.pictureBox1.Image = Image.FromFile(_finalOutputFiles.FileNames.Last());

                this.UserMessage("Image rendered.");
            }
            catch (Exception ex)
            {
                this.UserMessage($"Error: {ex.Message}");
            }
        }

        private void cboModels_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.lblModelDescription.Text = GetSelectedModel().description;
        }
    }
}
