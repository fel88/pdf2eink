using System.Diagnostics;

namespace pdf2eink
{
    public partial class Viewer : Form, ICbViewer
    {
        public Viewer()
        {
            InitializeComponent();
        }

        int pageNo;

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CB files (*.cb)|*.cb";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            Init(ofd.FileName);
        }

        string currentPath;
        public void Init(string path)
        {
            Text = $"Viewer: {path}";
            book = new CbBook(path);
            currentPath = path;

            Init();
        }

        void Init()
        {
            if (book.HasTOC)
                toolStripDropDownButton1.Enabled = true;

            toolStripStatusLabel1.Text = $"{book.Width} x {book.Height}";

            trackBar1.Maximum = book.pages - 1;
            showPage();
        }
        public void Init(Stream stream, string name)
        {
            Text = $"Viewer: {name}";
            book = new CbBook(stream);
            Init();
        }
        public int Pages => book.pages;

        public void ShowPage(int page)
        {
            pageNo = page;
            trackBar1.Value = page;
            showPage();
        }
        public void showPage()
        {
            toolStripStatusLabel3.Text = $"{pageNo + 1} / {book.pages}";
            var bmp = book.GetPage(pageNo);
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
            d.AddNumericField("page", "Page", max: book.pages, min: 1, decimalPlaces: 0);
            if (!d.ShowDialog())
                return;

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
                if (pageNo == book.pages - 1)
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
        CbBook book;
        private void showToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            TOCViewer t = new TOCViewer();
            t.Init(book.Toc, this);
            t.Show();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            Editor v = new Editor();
            v.Init(currentPath);
            v.MdiParent = MdiParent;
            v.Show();
        }
    }
}