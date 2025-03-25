using System.Diagnostics;

namespace pdf2eink
{
    public partial class ImageViewer : Form
    {
        public ImageViewer()
        {
            InitializeComponent();
        }

        public void Init(Bitmap bmp)
        {
            pictureBox1.Image = bmp;
            toolStripStatusLabel1.Text = $"{bmp.Width}x{bmp.Height}";
        }

        public ContextMenuStrip ContextMenu => contextMenuStrip1;
        public PictureBox PictureBox => pictureBox1;

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp1.png");
            ProcessStartInfo startInfo = new ProcessStartInfo("temp1.png");
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }
    }
}
