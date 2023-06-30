namespace pdf2eink
{
    public partial class Viewer : Form
    {
        public Viewer()
        {
            InitializeComponent();
        }
        byte[] bts;
        int pageNo;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            bts = File.ReadAllBytes(ofd.FileName);
            pages = BitConverter.ToInt32(bts, 4);
            trackBar1.Maximum = pages - 1;
            showPage();

        }
        int pages = 0;
        public void showPage()
        {
            toolStripStatusLabel3.Text = $"{pageNo + 1} / {pages}";
            var size = 76 * 448;
            var page1 = bts.Skip(12).Skip(pageNo * size).Take(size).ToArray();
            Bitmap bmp = new Bitmap(600, 448);

            for (int j = 0; j < bmp.Height; j++)
            {

                var line = page1.Skip(j * 76).Take(76).ToArray();
                int counter = 0;
                for (int i = 0; i < bmp.Width; i++)
                {
                    int byteNo = counter / 8;
                    int bitNo = counter % 8;
                    counter++;
                    var b = (byte)(line[byteNo] & (1 << (8 - 1 - bitNo))) > 0;
                    if (b)
                    {
                        bmp.SetPixel(i, j, Color.White);
                    }
                    else { bmp.SetPixel(i, j, Color.Black); }
                }
            }
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pageNo++;
            showPage();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            pageNo = trackBar1.Value;
            showPage();
        }
    }
}