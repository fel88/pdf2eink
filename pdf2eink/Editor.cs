using System.Text;

namespace pdf2eink
{
    public partial class Editor : Form
    {
        public Editor()
        {
            InitializeComponent();
        }
        byte[] bts;
        int pageNo;
        private void toolStripButton1_Click(object sender, EventArgs e)
        {


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
            if (pageNo == pages - 1)
                return;

            pageNo++;
            showPage();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            pageNo = trackBar1.Value;
            showPage();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("page", "Page", max: pages, min: 1, decimalPlaces: 0);
            d.ShowDialog();

            var page = d.GetIntegerNumericField("page") - 1;
            trackBar1.Value = page;
            pageNo = page;
            showPage();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            var size = 76 * 448;

            List<byte> part0 = new List<byte>();
            part0.AddRange(Encoding.UTF8.GetBytes("CB"));
            part0.AddRange(BitConverter.GetBytes((byte)0));//format . 0 -simple without meta info
            part0.AddRange(BitConverter.GetBytes(pages - 1));

            var part1 = bts.Skip(8).Take(4 + pageNo * size).ToArray();
            var part2 = bts.Skip(12 + pageNo * size).Skip(size).ToArray();

            bts = part0.Concat(part1).Concat(part2).ToArray();
            pages = BitConverter.ToInt32(bts, 4);
            trackBar1.Maximum = pages - 1;
            showPage();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            pageNo--;
            showPage();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            bts = File.ReadAllBytes(ofd.FileName);
            pages = BitConverter.ToInt32(bts, 4);
            trackBar1.Maximum = pages - 1;
            showPage();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            File.WriteAllBytes(sfd.FileName, bts);
        }

        private void almostWhiteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}