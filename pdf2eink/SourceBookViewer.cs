using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pdf2eink
{
    public partial class SourceBookViewer : Form
    {
        public SourceBookViewer()
        {
            InitializeComponent();

        }

        public void Open(IPagesProvider pp)
        {
            book = pp;
            pictureBox1.Image = pp.GetPage(0);
            Text = pp.SourcePath;
            trackBar1.Maximum = pp.Pages;
            toolStripStatusLabel1.Text = $"{pp.Pages} total pages";
        }

        IPagesProvider book;
        int page = 0;
        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                page++;
                pictureBox1.Image = book.GetPage(page);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp1.png");
            ProcessStartInfo startInfo = new ProcessStartInfo("temp1.png");
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            page = trackBar1.Value;
            pictureBox1.Image = book.GetPage(page);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("page", "Page", 0, book.Pages, decimalPlaces: 0);
            if (!d.ShowDialog())
                return;

            page = d.GetIntegerNumericField("page");
            pictureBox1.Image = book.GetPage(page);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}
