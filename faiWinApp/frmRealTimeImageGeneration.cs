using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace faiWinApp
{
    public partial class frmRealTimeImageGeneration : Form
    {
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
            this.UserMessage("Ready...");
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
