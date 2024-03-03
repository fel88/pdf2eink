using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace pdf2eink
{
    public partial class Editor : Form, ICbViewer
    {
        public Editor()
        {
            InitializeComponent();
        }

        int pageNo;

        public void showPage()
        {
            toolStripStatusLabel3.Text = $"{pageNo + 1} / {book.pages}";
            var bmp = book.GetPage(pageNo);
            pictureBox1.Image = bmp;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (pageNo == book.pages - 1)
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
            d.AddNumericField("page", "Page", max: book.pages, min: 1, decimalPlaces: 0);
            d.ShowDialog();

            var page = d.GetIntegerNumericField("page") - 1;
            trackBar1.Value = page;
            pageNo = page;
            showPage();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            DeletePage();
        }

        private void DeletePage()
        {
            book.DeletePage(pageNo);
            trackBar1.Maximum = book.pages - 1;
            showPage();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            pageNo--;
            showPage();
        }

        CbBook book;
        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            book = new CbBook(ofd.FileName);

            trackBar1.Maximum = book.pages - 1;
            showPage();
        }

        public void ShowPage(int page)
        {
            pageNo = page;
            trackBar1.Value = page;
            showPage();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            book.SaveAs(sfd.FileName);
        }

        private void almostWhiteToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void flyReadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ep = book.GetPage(pageNo);
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

            //attach source here
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TOCViewer t = new TOCViewer();
            t.Init(book.toc, this);
            t.Show();
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = Clipboard.GetText();

            var t = new TOC();
            t.Parse(text);
            book.toc = t;
        }
    }
}