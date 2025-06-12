using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pdf2eink
{
    public partial class mdi : Form
    {
        public mdi()
        {
            InitializeComponent();
            Shown += Mdi_Shown;
        }

        private void Mdi_Shown(object? sender, EventArgs e)
        {
            string[] args = Environment.GetCommandLineArgs();
            if (!args.Any(z => z.ToLower().EndsWith(".cb")))
                return;

            Viewer v = new Viewer();
            v.MdiParent = this;
            v.Init(args.First(z => z.ToLower().EndsWith(".cb")));
            v.Show();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.MdiParent = this;
            f.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Viewer v = new Viewer();
            v.MdiParent = this;
            v.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Editor v = new Editor();
            v.MdiParent = this;
            v.Show();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Settings s = new Settings();
            s.ShowDialog();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All supported files (*.pdf,*.djvu)|*.djvu;*.pdf|Pdf files (*.pdf)|*.pdf|Djvu files (*.djvu)|*.djvu";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            IPagesProvider p1 = null;
            if (ofd.FileName.ToLower().EndsWith("pdf"))
            {

                p1 = new PdfPagesProvider(ofd.FileName);
            }
            else
            if (ofd.FileName.ToLower().EndsWith("djvu") || ofd.FileName.ToLower().EndsWith("djv"))
            {
                //var fsi = File.CreateSymbolicLink("link1.temp", ofd.FileName);
                //fsi.Delete();
                p1 = new DjvuPagesProvider(ofd.FileName);
            }

            p1.Dpi = 300;
            SourceBookViewer s = new SourceBookViewer();
            p1.SourcePath = ofd.FileName;
            s.Open(p1);
            s.MdiParent = this;
            s.Show();
        }

        private void toolStripButton4_Click_1(object sender, EventArgs e)
        {

        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {

        }

        private void tileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void tileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void cascadeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.AddExtension = true;
            ofd.DefaultExt = "cb";
            ofd.Filter = "CB files (*.cb)|*.cb";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

        }
    }
}
