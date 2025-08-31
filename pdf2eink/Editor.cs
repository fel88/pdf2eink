using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;


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
            if (pageNo < 0)
                pageNo = 0;

            showPage();
        }

        CbBook book;
        string lastPath;

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "CB/TCB/ZCB files (*.cb, *.tcb, *.zcb)|*.cb;*.tcb;*.zcb|CB files (*.cb)|*.cb|Tiled book (*.tcb)|*.tcb";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            string pathToOpen = ofd.FileName;
            if (ofd.FileName.EndsWith(".zcb"))
            {
                if (MessageBox.Show("Decompress book?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CB files (*.cb)|*.cb";
                sfd.FileName = $"{pathToOpen.Replace(".zcb", string.Empty).Replace(".cb", string.Empty)}_decoded.cb";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                ZCBProcessor zcb = new ZCBProcessor();
                File.WriteAllBytes(sfd.FileName, zcb.Decompress(File.ReadAllBytes(pathToOpen)));

                pathToOpen = sfd.FileName;
            }

            Init(pathToOpen);
        }

        public void Init(string path)
        {
            Text = $"Editor: {path}";
            lastPath = path;
            book = new CbBook(path);
            trackBar1.Maximum = book.pages - 1;
            showPage();
        }

        public void InitFromStream(Stream stream)
        {
            Text = $"Editor";

            book = new CbBook(stream);
            trackBar1.Maximum = book.pages - 1;

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
            sfd.Filter = "CB/TCB (*.cb, *.tcb)|*.cb;*.tcb|CB files (*.cb)|*.cb|Tiled book (*.tcb)|*.tcb";

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
            //todo make settings to choose
            return;
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
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = (byte)~buf[i];
                }
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

        public void RenderText(int pageNo, string text, int x, int y)
        {
            //Bookerly  Webdings Bookerly, Literata, Lora, and PT Serif. For sans serif, I recommend Fira Sans, Noto Sans, Rambla, and Sen. 
            //Linux Biolinum
            // Bitter Pro at
            //Avenir Next. 
            /*

    Bookerly

    Amazon Ember

    Literata

    Alegreya

    Atkinson Hyperlegible

    Droid Sans

    Bitter Pro

    EBGaramond

    IBM Plex Sans

    Lora

    Halant

    Linux Libertine

    Ubuntu

    Sanchez

    Vollkorn *Gentium Book Plus

             */
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddStringField("text", "Text", text);
            d.AddStringField("fontName", "Font name", "Verdana");
            d.AddOptionsField("fontNameOpt", "Font name", ["Verdana", "Courier New", "Bookerly", "Literata", "Lora", "PT Serif", "Rambla", "Sens"], 0);
            d.AddBoolField("fontFromList", "Use font list", true);
            d.AddBoolField("fitSizeToLine", "fitSizeToLine", true);
            d.AddNumericField("fontSize", "Font size", 16);
            d.AddIntegerNumericField("x", "X", 0);
            d.AddIntegerNumericField("y", "Y", 0);

            if (!d.ShowDialog())
                return;

            var fontName = d.GetStringField("fontName");
            if (d.GetBoolField("fontFromList"))
                fontName = d.GetOptionsField("fontNameOpt");

            var fontSize = (float)d.GetNumericField("fontSize");
            x = d.GetIntegerNumericField("x");
            y = d.GetIntegerNumericField("y");
            text = d.GetStringField("text");

            var bmp = book.GetPage(pageNo);
            var gr = Graphics.FromImage(bmp);
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            //gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //gr.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.
            var font = new Font(fontName, fontSize);
            float fontStep = 0.5f;
            if (d.GetBoolField("fitSizeToLine"))
            {
                var ms = gr.MeasureString(text, font);
                //binary search here
                while (ms.Width < bmp.Width)
                {
                    fontSize += fontStep;
                    var font2 = new Font(fontName, fontSize);
                    ms = gr.MeasureString(text, font2);
                }
                fontSize -= fontStep;
                font = new Font(fontName, fontSize);
            }
            gr.DrawString(text, font, Brushes.Black, x, y);


            using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
            {
                var buf = BookExportContext.GetBuffer(clone);
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = (byte)~buf[i];
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


        private void extractTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (book == null)
            {
                TilesViewer tvv = new TilesViewer();
                tvv.MdiParent = MdiParent;
                tvv.Init([new TiledPageInfo() { Infos = Array.Empty<TileInfo>() }]);
                tvv.Show();
                return;
            }

            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddIntegerNumericField("startPage", "Start page", pageNo);
            d.AddIntegerNumericField("endPage", "End page", pageNo);

            if (!d.ShowDialog())
                return;
            var startPage = d.GetIntegerNumericField("startPage");
            var endPage = d.GetIntegerNumericField("endPage");
            Thread th = new Thread(() =>
            {
                statusStrip1.Invoke(() =>
                {
                    toolStripProgressBar1.Visible = true;
                    toolStripProgressBar1.Value = 0;
                    toolStripProgressBar1.Maximum = endPage - startPage + 1;
                });
                List<TiledPageInfo> pages = new List<TiledPageInfo>();
                for (int i = startPage; i <= endPage; i++)
                {
                    var bmp = book.GetPage(i);
                    TileProcessor tp = new TileProcessor();
                    tp.Init(bmp);
                    //tp.GetDebugBitmap().Save("debug1.jpg");
                    tp.MakeGroups();
                    //  tp.GetDebugBitmap().Save("debug2.jpg");
                    tp.SimplifyMarks();
                    // tp.GetDebugBitmap().Save("debug3.jpg");

                    pages.Add(tp.ExtractTiles());
                    statusStrip1.Invoke(() =>
                    {
                        toolStripProgressBar1.Value = i - startPage;
                        toolStripStatusLabel3.Text = (int)(100f * toolStripProgressBar1.Value / (float)toolStripProgressBar1.Maximum) + "%";
                    });
                }


                var allTilesInfos = pages.SelectMany(z => z.Infos).ToArray();
                var allTiles = pages.SelectMany(z => z.Infos.Select(u => u.Tile)).ToArray();
                var tiles = TileProcessor.DistinctTiles(allTiles.ToArray()).ToList();

                foreach (var item in allTilesInfos)
                {
                    item.Tile = tiles.First(z => z.ImageHash == item.Tile.ImageHash);
                }

                Invoke(() =>
                {
                    toolStripProgressBar1.Visible = false;

                    TilesViewer tv = new TilesViewer();
                    tv.MdiParent = MdiParent;
                    tv.Init(pages.ToArray());
                    tv.Show();
                });


            });
            th.IsBackground = true;
            th.Start();

        }

        private void compressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ZCB files (*.zcb)|*.zcb";
            sfd.FileName = $"{lastPath}.zcb";

            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            var bts = book.GetBytes();
            ZCBProcessor zcb = new ZCBProcessor();
            File.WriteAllBytes(sfd.FileName, zcb.Compress(bts));
        }

        private void renderTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RenderText(pageNo, "Hello world", 10, 10);
            showPage();
        }
        void fillRectangle(int x, int y, int width, int height)
        {
            var bmp = book.GetPage(pageNo);
            var gr = Graphics.FromImage(bmp);

            gr.FillRectangle(Brushes.White, x, y, width, height);

            using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
            {
                var buf = BookExportContext.GetBuffer(clone);
                for (int i = 0; i < buf.Length; i++)
                {
                    buf[i] = (byte)~buf[i];
                }
                book.UpdatePage(buf, pageNo);
            }
        }
        private void fillRectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {

            fillRectangle(0, 0, book.Width, book.Height / 3);
            showPage();
        }

        private void defaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            pictureBox1.Invalidate();
        }

        private void nearestToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            pictureBox1.Invalidate();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            if (pageNo == book.pages - 1)
                return;

            pageNo++;
            showPage();
        }

        public void DrawTextInRectangle(CbBook book, string text, Font font, int? maxPages)
        {
            toolStripProgressBar1.Maximum = text.Length;
            toolStripProgressBar1.Visible = true;
            Task.Run(() =>
            {
                int charactersLeft = text.Length;
                int originalLength = text.Length;
                while (charactersLeft > 0)
                {
                    if (maxPages != null && book.pages > maxPages)
                        break;
                    statusStrip1.Invoke(() =>
                    {
                        toolStripProgressBar1.Value = originalLength - charactersLeft;
                    });
                    BookExportParams bep = new BookExportParams();
                    RectangleF layoutRectangle = new RectangleF(0, 0, book.Width, book.Height - bep.PageInfoHeight);
                    book.InsertPage(book.pages);
                    var bmp = book.GetPage(pageNo);

                    var gr = Graphics.FromImage(bmp);
                    //gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                    //fillRectangle(0, 0, book.Width, book.Height);
                    gr.FillRectangle(Brushes.White, 0, 0, book.Width, book.Height);

                    // Option 1: To get the size of the entire string if it were drawn within the layout area
                    SizeF textSize = gr.MeasureString(text, font, layoutRectangle.Size);

                    // Option 2: To get the number of characters and lines that actually fit
                    int charactersFitted;
                    int linesFilled;
                    StringFormat sf = new StringFormat(StringFormatFlags.LineLimit); // Prevent wrapping

                    SizeF fittedSize = gr.MeasureString(text, font, layoutRectangle.Size, sf, out charactersFitted, out linesFilled);

                    // 'charactersFitted' will contain the count of characters that fit within the layoutRectangle.
                    // 'linesFilled' will contain the count of lines that fit within the layoutRectangle.
                    if (linesFilled > 19)
                    {

                    }
                    // You can then use these values to decide how to draw the text,
                    // e.g., truncate the string or adjust the font size.
                    //StringFormat sf = new StringFormat();
                    //      sf.Trimming = StringTrimming.EllipsisWord;
                    if (charactersFitted < text.Length)
                    {
                        // Text does not fully fit, you might want to truncate it or add "..."
                        string truncatedText = text.Substring(0, charactersFitted);// + "...";
                        text = text.Substring(charactersFitted);
                        charactersLeft -= charactersFitted;
                        //gr.Clip = new Region(layoutRectangle);
                        gr.DrawString(truncatedText, font, Brushes.Black, new RectangleF(0, 0, book.Width, fittedSize.Height), sf);
                    }
                    else
                    {
                        // Text fits, draw it normally
                        gr.DrawString(text, font, Brushes.Black, layoutRectangle, sf);
                        charactersLeft = 0;

                    }
                    //gr.Clip = new Region(new RectangleF (0,0,book.Width,book.Height));
                    //footer
                    var hh = book.Height - bep.PageInfoHeight - 1;

                    gr.FillRectangle(Brushes.White, 0, hh, book.Width, bep.PageInfoHeight + 1);

                    gr.DrawLine(Pens.Black, 0, hh, book.Width, hh);

                    var str = $"{pageNo} / {book.pages}";
                    /*for (int z = 0; z < str.Length; z++)
                    {
                        gr.DrawString(str[z].ToString(), new Font("Courier New", 6),
                     Brushes.Black, 0, 5 + z * 10);
                    }*/
                    string fontName = "Consolas";
                    fontName = "Courier New";
                    var ms = gr.MeasureString("99999 / 99999", new Font(fontName, 7));

                    int xx = (pageNo * 15) % (int)(book.Width - ms.Width - 1);
                    gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                    gr.DrawString(str.ToString(), new Font(fontName, 7), Brushes.Black, xx, hh - 1);
                    using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                    {
                        var buf = BookExportContext.GetBuffer(clone);
                        for (int i = 0; i < buf.Length; i++)
                        {
                            buf[i] = (byte)~buf[i];
                        }
                        book.UpdatePage(buf, pageNo);
                    }

                    pageNo++;
                }

                statusStrip1.Invoke(() =>
                {
                    toolStripProgressBar1.Visible = false;
                });
            });


        }
        private void createFromTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var text = File.ReadAllText(ofd.FileName);
            var d = AutoDialog.DialogHelpers.StartDialog();
            MemoryStream ms = new MemoryStream();
            CreateEmptyBook(ms);
            InitFromStream(ms);



            d.AddStringField("fontName", "Font name", "Verdana");
            d.AddOptionsField("fontNameOpt", "Font name", ["Verdana", "Courier New", "Bookerly", "Literata", "Lora", "PT Serif", "Rambla", "Sens"], 0);
            d.AddBoolField("fontFromList", "Use font list", true);
            d.AddBoolField("pagesLimit", "pagesLimit", true);

            d.AddNumericField("fontSize", "Font size", 16);
            d.AddIntegerNumericField("maxPages", "Max pages", 20);



            if (!d.ShowDialog())
                return;

            var fontName = d.GetStringField("fontName");
            if (d.GetBoolField("fontFromList"))
                fontName = d.GetOptionsField("fontNameOpt");

            var fontSize = (float)d.GetNumericField("fontSize");

            int? pagesLimit = null;
            if (d.GetBoolField("pagesLimit"))
            {
                pagesLimit = d.GetIntegerNumericField("maxPages");
            }

            DrawTextInRectangle(book, text, new Font(fontName, fontSize), pagesLimit);
        }

        private void CreateEmptyBook(MemoryStream ms)
        {
            var fs = ms;
            BookExportContext ctx = new BookExportContext();
            ctx.Stream = ms;
            BookExportParams eparams = new BookExportParams();

            //fs.Write(Encoding.UTF8.GetBytes("CB" + '\0'));
            fs.Write(Encoding.UTF8.GetBytes("CB"));
            //if (eparams.TiledMode)
            {
                //  fs.WriteByte(0x2); //version CB format v2 : rectified and tiled

            }
            //  else
            {
                fs.WriteByte(0x1); //version CB format: raw pages only
            }
            if (eparams.TOC != null && eparams.TOC.Items.Count > 0)
                fs.WriteByte(0x1);//format . 1 -with TOC
                                  //wite TOC here                  
            else
                fs.WriteByte(0x0);//format . 0 -simple without meta info

            fs.Write(BitConverter.GetBytes(0));
            fs.Write(BitConverter.GetBytes((ushort)eparams.Width));//width
            fs.Write(BitConverter.GetBytes((ushort)eparams.Height));//heigth

            fs.Seek(0, SeekOrigin.Begin);

        }
      

        public static string[] SplitByCapitalLetters(string input)
        {
            // Splits the string at every point where a lowercase letter is followed by an uppercase letter
            // or where an uppercase letter is followed by another uppercase letter and then a lowercase letter
            // (to handle acronyms like "USA Today").
            return Regex.Split(input, @"(?<=[a-z])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])");
        }
        private void createFromLettersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            var text = File.ReadAllText(ofd.FileName);
            //var d = AutoDialog.DialogHelpers.StartDialog();
            MemoryStream ms = new MemoryStream();
            CreateEmptyBook(ms);
            InitFromStream(ms);

            Dictionary<int, Font> _fonts = new Dictionary<int, Font>();
            var doc = XDocument.Load(ofd.FileName);
            foreach (var font in doc.Descendants("font"))
            {
                var fontId = int.Parse(font.Attribute("id").Value);
                var bold = bool.Parse(font.Attribute("bold").Value);
                var italic = bool.Parse(font.Attribute("italic").Value);
                var family = font.Attribute("family").Value;
                var size = font.Attribute("size").Value.ToFloat();

                var split = family.Split(['+', '-']);
                var ss = split[1];
                var cap = SplitByCapitalLetters(ss);
                var cands = FontFamily.Families.Where(z => cap.All(u => z.ToString().Contains(u))).ToArray();

                var font1 = new Font(cands[0], size, bold ? FontStyle.Bold : FontStyle.Regular);
                _fonts.Add(fontId, font1);

            }

            foreach (var pageItem in doc.Descendants("page"))
            {
                if (book.pages > 3)
                    break;
                book.InsertPage(book.pages);
                var pageW = pageItem.Attribute("w").Value.ToFloat();
                var pageH = pageItem.Attribute("h").Value.ToFloat();


                statusStrip1.Invoke(() =>
                {
                    //toolStripProgressBar1.Value = originalLength - charactersLeft;
                });
                BookExportParams bep = new BookExportParams();
                RectangleF layoutRectangle = new RectangleF(0, 0, book.Width, book.Height - bep.PageInfoHeight);
                var bmp = book.GetPage(pageNo);

                var gr = Graphics.FromImage(bmp);
                //gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                //fillRectangle(0, 0, book.Width, book.Height);
                gr.FillRectangle(Brushes.White, 0, 0, book.Width, book.Height);
                var minX = (double)pageW;
                var minY = (double)pageH;

                var maxX = 0.0;
                var maxY = 0.0;
                foreach (var letterInfo in pageItem.Elements("letter"))
                {
                    var x = letterInfo.Attribute("x").Value.ToDouble();
                    var y = letterInfo.Attribute("y").Value.ToDouble();
                    var w = letterInfo.Attribute("w").Value.ToDouble();
                    var h = letterInfo.Attribute("h").Value.ToDouble();
                    var locX = letterInfo.Attribute("locationX").Value.ToDouble();
                    var locY = letterInfo.Attribute("locationY").Value.ToDouble();
                    x = locX;
                    y = locY;

                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x + w);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y + h);
                }

                var realPageW = maxX - minX + 1;
                var realPageH = maxY - minY;

                foreach (var letterInfo in pageItem.Elements("letter"))
                {
                    var x = letterInfo.Attribute("x").Value.ToDouble();
                    var y = letterInfo.Attribute("y").Value.ToDouble();
                    var w = letterInfo.Attribute("w").Value.ToDouble();
                    var h = letterInfo.Attribute("h").Value.ToDouble();
                    var locX = letterInfo.Attribute("locationX").Value.ToDouble();
                    var locY = letterInfo.Attribute("locationY").Value.ToDouble();
                    var fontId = int.Parse(letterInfo.Attribute("fontId").Value);

                    x = locX;
                    y = locY;

                    var font = _fonts[fontId];
                    // Text fits, draw it normally
                    var kx = book.Width / realPageW;
                    var ky = (book.Height - bep.PageInfoHeight) / realPageH;
                    x -= minX;
                    y -= minY;
                    x *= kx;
                    y *= ky;
                    gr.DrawString(letterInfo.Attribute("letter").Value, font, Brushes.Black, (float)x, (float)y);
                }

                var hh = book.Height - bep.PageInfoHeight - 1;

                gr.FillRectangle(Brushes.White, 0, hh, book.Width, bep.PageInfoHeight + 1);

                gr.DrawLine(Pens.Black, 0, hh, book.Width, hh);

                var str = $"{pageNo} / {book.pages}";
                /*for (int z = 0; z < str.Length; z++)
                {
                    gr.DrawString(str[z].ToString(), new Font("Courier New", 6),
                 Brushes.Black, 0, 5 + z * 10);
                }*/
                string fontName = "Consolas";
                fontName = "Courier New";
                var mss = gr.MeasureString("99999 / 99999", new Font(fontName, 7));

                int xx = (pageNo * 15) % (int)(book.Width - mss.Width - 1);
                gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                gr.DrawString(str.ToString(), new Font(fontName, 7), Brushes.Black, xx, hh - 1);
                using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                {
                    var buf = BookExportContext.GetBuffer(clone);
                    for (int i = 0; i < buf.Length; i++)
                    {
                        buf[i] = (byte)~buf[i];
                    }
                    book.UpdatePage(buf, pageNo);
                }



                statusStrip1.Invoke(() =>
                {
                    toolStripProgressBar1.Visible = false;
                });
                pageNo++;
            }

        }
    }


}
