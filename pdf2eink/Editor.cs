using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.ComponentModel.DataAnnotations;
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

        public Bitmap ExtractPage()
        {
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
            return bmp;
        }

        public void showPage()
        {
            toolStripStatusLabel3.Text = $"{pageNo + 1} / {pages}";
            var bmp = ExtractPage();
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

        public void DeletePage()
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

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            DeletePage();
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

        private void flyReadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ep = ExtractPage();
            BookExporter bex = new BookExporter();
            var mat = ep.ToMat();
            using (var mat2 = mat.CvtColor(ColorConversionCodes.BGR2GRAY))
            using (var threshold = BookExporter.Threshold(mat2, new BookExportParams() { }))
            {
                var cuts = bex.GetHorizontalCuts(threshold);
                int lastY = 0;
                int odd = 0;
                for (int i = 0; i < cuts.Length; i++)
                {
                    //get crop
                    if ((lastY - cuts[i]) == 0)
                    {
                        lastY = cuts[i];
                        continue;
                    }
                    using (var sub = threshold.SubMat(lastY, cuts[i], 0, mat.Cols))
                    {                       
                        odd++;
                        if (odd % 2 == 0)
                        {
                            //reverse
                            var vcuts = bex.GetVerticalCuts(sub, 5);
                            List<Mat> clones = new List<Mat>();
                            for (int j = 1; j < vcuts.Length; j++)
                            {
                                using (var sub1 = threshold.SubMat(0, sub.Rows, vcuts[j - 1], vcuts[j]))
                                {
                                    clones.Add(sub1.Clone());
                                }
                            }
                        }
                    }
                    lastY = cuts[i];
                }
            }
        }

        private void attachSourceBookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All supported files (*.pdf,*.djvu)|*.djvu;*.pdf|Pdf files (*.pdf)|*.pdf|Djvu files (*.djvu)|*.djvu";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
        }
    }
}