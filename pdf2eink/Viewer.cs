using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

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
            ofd.Filter = "CB files (*.cb)|*.cb";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            Init(ofd.FileName);
        }

        public void ParseTOC()
        {
            toolStripDropDownButton1.Enabled = true;
            //parse toc here
            var tocItems = BitConverter.ToUInt32(bts, 4);
            int accum = 8;
            for (int i = 0; i < tocItems; i++)
            {
                var page = BitConverter.ToInt32(bts, (int)accum);
                accum += 4;
                var ident = BitConverter.ToUInt16(bts, (int)accum);
                accum += 2;
                var len = BitConverter.ToInt32(bts, (int)accum);
                accum += 4;
                var str = Encoding.UTF8.GetString(bts, accum, len);
                accum += len;
                toc.Items.Add(new TOCItem() { Header = str, Page = page, Ident = ident });
            }
        }

        TOC toc = new TOC();

        public void Init(string path)
        {
            Text = $"Viewer: {path}";
            bts = File.ReadAllBytes(path);
            var format = bts[4];
            if (format == 1)
            {
                ParseTOC();
            }
            pages = BitConverter.ToInt32(bts, 4);
            trackBar1.Maximum = pages - 1;
            showPage();
        }

        public int Pages => pages;
        int pages = 0;
        public void ShowPage(int page)
        {
            pageNo = page;
            trackBar1.Value = page;
            showPage();
        }
        public void showPage()
        {
            toolStripStatusLabel3.Text = $"{pageNo + 1} / {pages}";
            var width = BitConverter.ToUInt16(bts, 8);
            var height = BitConverter.ToUInt16(bts, 10);
            int stride = 4 * (int)Math.Ceiling(width / 8 / 4f);//aligned 4
            var size = stride * height;
            var page1 = bts.Skip(12).Skip(pageNo * size).Take(size).ToArray();
            Bitmap bmp = new Bitmap(width, height);

            for (int j = 0; j < bmp.Height; j++)
            {

                var line = page1.Skip(j * stride).Take(stride).ToArray();
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
            pageNo--;
            showPage();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (pageNo == pages - 1)
                    return;

                pageNo++;
                showPage();
            }
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp1.png");
            ProcessStartInfo startInfo = new ProcessStartInfo("temp1.png");
            //startInfo.Verb = "edit";
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }

        private void showToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TOCViewer t = new TOCViewer();
            t.Init(toc,this);
            t.Show();                
        }
    }
}