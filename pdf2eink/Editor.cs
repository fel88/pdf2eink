using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Net.Http.Headers;
using System.Text;

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
            if (MessageBox.Show("Are you sure to delete current page?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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
            Mat result = null;
            using (var mat2 = mat.CvtColor(ColorConversionCodes.BGR2GRAY))
            using (var threshold = BookExporter.Threshold(mat2, new BookExportParams() { }))
            {
                result = new Mat(threshold.Size(), threshold.Type());
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
                        //Directory.CreateDirectory("temp");
                        //sub.SaveImage(Path.Combine("temp", $"line{odd}.png"));
                        if (odd % 2 == 0)
                        {
                            //reverse
                            var vcuts = bex.GetVerticalCuts(sub, 5).ToList();
                            vcuts.Add(sub.Width - 1);
                            List<Mat> clones = new List<Mat>();
                            for (int j = 0; j < vcuts.Count; j++)
                            {
                                int x0 = 0;
                                if (j > 0)
                                    x0 = vcuts[j - 1];

                                using (var sub1 = sub.SubMat(0, sub.Rows, x0, vcuts[j]))
                                {
                                    clones.Add(sub1.Clone());
                                    //  clones.Last().SaveImage(Path.Combine("temp", $"clone{odd}_{j}.png"));
                                }
                            }
                            Mat mat3 = new Mat(sub.Size(), sub.Type());
                            mat3.SetTo(Scalar.White);
                            int xx = 0;
                            clones.Reverse();
                            xx = 0;
                            foreach (var item in clones)
                            {
                                if (xx < 0)
                                    break;

                                var roi = new Mat(mat3, new Rect(xx, 0, item.Width, item.Height));
                                xx += item.Width;
                                item.CopyTo(roi);
                            }
                            var roi2 = new Mat(result, new Rect(0, lastY, mat3.Width, mat3.Height));
                            mat3.CopyTo(roi2);
                            //mat3.SaveImage(Path.Combine("temp", "combo.png"));
                        }
                        else
                        {
                            var mat3 = new Mat(threshold, new Rect(0, lastY, threshold.Width, cuts[i] - lastY));
                            var roi2 = new Mat(result, new Rect(0, lastY, mat3.Width, mat3.Height));
                            mat3.CopyTo(roi2);
                        }
                    }
                    lastY = cuts[i];
                }
            }
            var bmp = result.ToBitmap();
            Form ff = new Form();
            ff.MdiParent = MdiParent;
            PictureBox pb = new PictureBox();
            pb.Dock = DockStyle.Fill;
            pb.Image = bmp;
            pb.SizeMode = PictureBoxSizeMode.Zoom;
            ff.Controls.Add(pb);
            ff.Show();
            //result.SaveImage(Path.Combine("temp", "result.png"));
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
            if (book.Toc == null)
                return;

            TOCViewer t = new TOCViewer();
            t.Init(book.Toc, this);
            t.Show();
        }

        private void parseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var text = Clipboard.GetText();

            var t = new TOC();
            t.Parse(text);
            book.AppendTOC(t);
        }

        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (book.Toc == null)
            {
                if (MessageBox.Show("Book doesn't have a TOC. Create one?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;

                book.AppendTOC(new TOC());
            }

            TOCViewer tocv = new TOCViewer();
            tocv.Init(book.Toc, this, true);
            tocv.Show();
        }

        private void bustofedonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ep = book.GetPage(pageNo);
            BookExporter bex = new BookExporter();
            var mat = ep.ToMat();
            Mat result = null;
            using (var mat2 = mat.CvtColor(ColorConversionCodes.BGR2GRAY))
            using (var threshold = BookExporter.Threshold(mat2, new BookExportParams() { }))
            {
                result = new Mat(threshold.Size(), threshold.Type());
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
                        //Directory.CreateDirectory("temp");
                        //sub.SaveImage(Path.Combine("temp", $"line{odd}.png"));
                        if (odd % 2 == 0 && i != cuts.Length - 1)
                        {
                            //reverse
                            using var mat3 = sub.Flip(FlipMode.Y);
                            var roi2 = new Mat(result, new Rect(0, lastY, mat3.Width, mat3.Height));

                            mat3.CopyTo(roi2);
                            //mat3.SaveImage(Path.Combine("temp", "combo.png"));
                        }
                        else
                        {
                            var mat3 = new Mat(threshold, new Rect(0, lastY, threshold.Width, cuts[i] - lastY));
                            var roi2 = new Mat(result, new Rect(0, lastY, mat3.Width, mat3.Height));
                            mat3.CopyTo(roi2);
                        }
                    }
                    lastY = cuts[i];
                }
            }

            var bmp = result.ToBitmap();
            ImageViewer ff = new ImageViewer();
            ff.Tag = pageNo;
            ff.MdiParent = MdiParent;
            ff.Init(bmp);
            var m = new ToolStripMenuItem() { Text = "apply to book", Tag = ff };
            m.Click += M_Click;
            ff.ContextMenu.Items.Add(m);
            ff.Show();
            //result.SaveImage(Path.Combine("temp", "result.png"));
        }

        private void M_Click(object? sender, EventArgs e)
        {
            var tsmi = (sender as ToolStripMenuItem);
            var imv = tsmi.Tag as ImageViewer;
            var pageNo = (int)imv.Tag;
            var bmp = (imv.PictureBox.Image as Bitmap);
            using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
            {
                var buf = BookExportContext.GetBuffer(clone);
                book.UpdatePage(buf, pageNo);
            }
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

        private void showImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp1.png");
            ProcessStartInfo startInfo = new ProcessStartInfo("temp1.png");
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddIntegerNumericField("page", "Page");
            if (!d.ShowDialog())
                return;

            var pageNo = d.GetIntegerNumericField("page");
            book.InsertPage(pageNo);
        }

        private void printToPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddNumericField("fontSize", "Font size", 12, 60, 6);
            d.AddNumericField("hGap", "X gap", 20, 600, 0);
            d.AddNumericField("vGap", "Y gap", 15, 600, 0);

            if (!d.ShowDialog())
                return;

            var fs = d.GetNumericField("fontSize");
            var hGap = (float)d.GetNumericField("hGap");
            var vGap = (float)d.GetNumericField("vGap");

            using var bmp = new Bitmap(book.Width, book.Height);
            using var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            for (int i1 = 0; i1 < book.Toc.Items.Count; i1++)
            {
                var page = book.Toc.Items[i1];

                gr.DrawString($"{page.Page}. {page.Text}", new Font("Arial", (float)fs), Brushes.Black, page.Ident * hGap, i1 * vGap);
            }

            using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
            {
                var buf = BookExportContext.GetBuffer(clone);
                book.UpdatePage(buf, pageNo);
            }

            showPage();
        }

        public void UpdateFooter(int pageNo, int minGray = 180)
        {
            var bmp = book.GetPage(pageNo);

            BookExportParams bep = new BookExportParams();
            BookExportContext.PrintFooter(pageNo + 1, book.pages, bmp, bep.PageInfoHeight);

            using var mat = bmp.ToMat();

            using var mat2 = mat.Threshold(minGray, 255, ThresholdTypes.Binary);
            using var bmp1 = mat2.ToBitmap();

            using (var clone = bmp1.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
            {
                var buf = BookExportContext.GetBuffer(clone);
                book.UpdatePage(buf, pageNo);
            }
        }

        public void InverseColors(int pageNo)
        {
            var bmp = book.GetPage(pageNo);
                      

            using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
            {
                var buf = BookExportContext.GetBuffer(clone);
                for (int i = 0; i < buf.Length; i++)
                {
                    //buf[i] = (byte)~buf[i];
                }    
                book.UpdatePage(buf, pageNo);
            }
        }

        private void singlePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateFooter(pageNo);
            showPage();
        }

        private void allPagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Maximum = book.pages;
            toolStripProgressBar1.Visible = true;
            Thread th = new Thread(() =>
            {
                for (int i = 0; i < book.pages; i++)
                {
                    statusStrip1.Invoke(() =>
                    {
                        toolStripProgressBar1.Value = i;
                    });
                    UpdateFooter(i);
                }
                statusStrip1.Invoke(() =>
                {
                    toolStripProgressBar1.Visible = false;
                });

            });
            th.Start();
        }

        private void inverseColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void allPagesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            toolStripProgressBar1.Maximum = book.pages;
            toolStripProgressBar1.Visible = true;
            Thread th = new Thread(() =>
            {
                for (int i = 0; i < book.pages; i++)
                {
                    statusStrip1.Invoke(() =>
                    {
                        toolStripProgressBar1.Value = i;
                    });
                    InverseColors(i);
                }
                statusStrip1.Invoke(() =>
                {
                    toolStripProgressBar1.Visible = false;
                });

            });
            th.Start();
        }

        private void thisPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            InverseColors(pageNo);
            showPage();
        }
    }
}