using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using UglyToad.PdfPig.Content;


namespace pdf2eink
{
    public partial class Editor : Form, ICbViewer
    {
        public Editor()
        {
            InitializeComponent();
            // In your application's startup or a relevant initialization point
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
            undos.Clear();
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
            d.AddInt("page", "Page");
            if (!d.ShowDialog())
                return;

            var pageNo = d.GetInt("page");
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
        string lastText = "Hello world!";
        public void RenderText(int pageNo)
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
            int x;
            int y;


            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddStringField("text", "Text", lastText);
            d.AddStringField("fontName", "Font name", "Verdana");
            Font targetFont = null;
            d.AddCustomDialogField("customFont", "Font", () =>
            {
                FontDialog fd = new FontDialog();
                if (fd.ShowDialog() == DialogResult.OK)
                    targetFont = fd.Font;
            });
            d.AddOptionsField("fontNameOpt", "Font name", ["Courier New", "Consolas", "Verdana", "Bookerly", "Literata", "Lora", "PT Serif", "Rambla", "Sens"], 0);
            d.AddBoolField("fontFromList", "Use font list", true);
            d.AddBoolField("fitSizeToLine", "fitSizeToLine", true);
            d.AddBoolField("bold", "Bold", false);
            d.AddBoolField("italic", "Italic", false);
            d.AddBoolField("underline", "Underline", false);
            d.AddDouble("fontSize", "Font size", 16);
            d.AddInt("x", "X", 0);
            d.AddInt("y", "Y", 0);

            if (!d.ShowDialog())
                return;

            var fontName = d.GetStringField("fontName");
            if (d.GetBoolField("fontFromList"))
                fontName = d.GetOptionsField("fontNameOpt");

            var fontSize = (float)d.GetDouble("fontSize");
            x = d.GetInt("x");
            y = d.GetInt("y");
            lastText = d.GetStringField("text");
            var style = FontStyle.Regular;
            if (d.GetBoolField("bold"))
                style |= FontStyle.Bold;
            if (d.GetBoolField("italic"))
                style |= FontStyle.Italic;
            if (d.GetBoolField("underline"))
                style |= FontStyle.Underline;

            var bmp = book.GetPage(pageNo);
            var gr = Graphics.FromImage(bmp);
            gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            //gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //gr.PixelOffsetMode=System.Drawing.Drawing2D.PixelOffsetMode.
            var font = new Font(fontName, fontSize, style);
            if (targetFont != null)
                font = targetFont;
            float fontStep = 0.5f;
            if (d.GetBoolField("fitSizeToLine"))
            {
                var ms = gr.MeasureString(lastText, font);
                //binary search here
                while (ms.Width < bmp.Width)
                {
                    fontSize += fontStep;
                    var font2 = new Font(fontName, fontSize, style);
                    ms = gr.MeasureString(lastText, font2);
                }
                fontSize -= fontStep;


                font = new Font(fontName, fontSize, style);
            }
            gr.DrawString(lastText, font, Brushes.Black, x, y);


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
            SaveState();
            RenderText(pageNo);
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

        public void SaveState()
        {
            undos.Push(book.GetBytes());
        }

        private void fillRectangleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddIntegerNumericField("x", "X", max: book.Width);
            d.AddIntegerNumericField("y", "Y", max: book.Height);
            d.AddIntegerNumericField("w", "Width", max: book.Width);
            d.AddIntegerNumericField("h", "Height", max: book.Height);
            if (!d.ShowDialog())
                return;
            var x = d.GetIntegerNumericField("x");
            var y = d.GetIntegerNumericField("y");
            var w = d.GetIntegerNumericField("w");
            var h = d.GetIntegerNumericField("h");
            SaveState();
            fillRectangle(x, y, w, h);
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

        public static int GetCharactersThatFitLine(Graphics g, string text, Font font, float maxWidth,
            bool backTrackToLastSpacePositon = true, bool autoSplitNewLine = true)
        {
            int charactersFitted = 0;
            int lastSpacePosition = 0;
            for (int i = 1; i <= text.Length; i++)
            {
                if (text[i - 1] == ' ')
                {
                    lastSpacePosition = i;
                }
                string subString = text.Substring(0, i);
                SizeF size = g.MeasureString(subString, font);

                if (size.Width > maxWidth)
                {
                    // The current substring is too wide, so the previous one was the longest that fit.

                    charactersFitted = backTrackToLastSpacePositon ? lastSpacePosition : i - 1;
                    break;
                }
                else
                {
                    charactersFitted = i;
                }

                if (autoSplitNewLine && text[i - 1] == '\r' || text[i - 1] == '\n')
                    break;
            }
            return charactersFitted;
        }

        public static string ReverseString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            char[] charArray = input.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public class RenderTextBookContext
        {
            public string text;
        }

        public void RenderBookFromText(CbBook book, string _text, Font font,
            int? maxPages,
            bool bustrophedon,
            bool customLineSpacing = false,
            float lineSpacing = 1f)
        {
            toolStripProgressBar1.Maximum = _text.Length;
            toolStripProgressBar1.Visible = true;
            if (maxPages != null)
            {
                toolStripProgressBar1.Maximum = maxPages.Value;
                toolStripProgressBar1.Visible = true;
            }
            Task.Run(() =>
            {
                RenderTextBookContext renderCtx = new RenderTextBookContext();
                renderCtx.text = _text;

                renderCtx.text = renderCtx.text.Replace("\r\n", "\n");



                int originalLength = _text.Length;
                while (renderCtx.text.Length > 0)
                {
                    if (maxPages != null && book.pages > maxPages)
                        break;

                    statusStrip1.Invoke(() =>
                    {
                        if (maxPages == null)
                        {
                            toolStripProgressBar1.Value = originalLength - renderCtx.text.Length;
                            toolStripStatusLabel3.Text = $"pages: {book.pages}  {(int)(100f * toolStripProgressBar1.Value / (float)toolStripProgressBar1.Maximum)}%";
                        }
                        else
                        {
                            toolStripProgressBar1.Value = book.pages;
                            toolStripStatusLabel3.Text = $"pages: {book.pages}  {(int)(100f * toolStripProgressBar1.Value / (float)toolStripProgressBar1.Maximum)}%";
                        }
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

                    // Option 2: To get the number of characters and lines that actually fit

                    StringFormat sf = new StringFormat(StringFormatFlags.LineLimit); // Prevent wrapping



                    // 'charactersFitted' will contain the count of characters that fit within the layoutRectangle.
                    // 'linesFilled' will contain the count of lines that fit within the layoutRectangle.

                    // You can then use these values to decide how to draw the text,
                    // e.g., truncate the string or adjust the font size.
                    //StringFormat sf = new StringFormat();
                    //      sf.Trimming = StringTrimming.EllipsisWord;
                    //if (charactersFitted < text.Length)
                    { // Text does not fully fit, you might want to truncate it or add "..."
                        //string truncatedText = text.Substring(0, charactersFitted);// + "...";

                        //charactersLeft -= charactersFitted;
                        //gr.Clip = new Region(layoutRectangle);

                        if (bustrophedon || customLineSpacing)
                        {
                            //var yGap = fittedSize.Height / linesFilled;
                            var yGap = font.GetHeight(gr);

                            if (customLineSpacing)
                                yGap *= lineSpacing;

                            RenderTextLineByLine(book, font, layoutRectangle, bustrophedon, yGap, renderCtx, gr, sf);
                        }
                        else
                            DrawTextToRectangle(book, renderCtx, font, layoutRectangle, gr, sf);

                    }
                    /* else
                     {
                         // Text fits, draw it normally
                         gr.DrawString(text, font, Brushes.Black, layoutRectangle, sf);
                         charactersLeft = 0;

                     }*/
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
                    pageNo = 0;
                    showPage();
                });

            });


        }

        private static void RenderTextLineByLine(CbBook book, Font font, RectangleF layoutRectangle,
            bool bustrophedon, float yGap, RenderTextBookContext renderCtx, Graphics gr, StringFormat sf)
        {
            StringFormat stringFormat = new StringFormat(StringFormatFlags.LineLimit);
            stringFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

            int lineIndex = 0;

            while (true)
            {
                var maxChars = GetCharactersThatFitLine(gr, renderCtx.text, font, book.Width);
                if (renderCtx.text.Length > 0 && maxChars == 0)
                {
                    maxChars = GetCharactersThatFitLine(gr, renderCtx.text, font, book.Width, false);
                }

                var textToDraw = renderCtx.text.Substring(0, maxChars);
                SizeF fittedSize2 = gr.MeasureString(textToDraw, font);

                renderCtx.text = renderCtx.text.Substring(maxChars);

                if (bustrophedon && lineIndex % 2 != 0)
                {
                    // Save the current graphics state
                    GraphicsState state = gr.Save();
                    var normalLocation = new PointF(0, yGap * lineIndex);
                    // Measure the string to calculate translation
                    SizeF textSize2 = gr.MeasureString(textToDraw, font);

                    // Apply a horizontal flip (ScaleTransform with -1f for X)
                    // Then translate the origin to compensate for the flip
                    gr.TranslateTransform(normalLocation.X + textSize2.Width, normalLocation.Y);
                    gr.ScaleTransform(-1f, 1f);
                    gr.TranslateTransform(-normalLocation.X, -normalLocation.Y); // Translate back to the original Y position

                    // Draw the mirrored string
                    gr.DrawString(textToDraw, font, Brushes.Black, normalLocation, sf);

                    // Restore the original graphics state
                    gr.Restore(state);

                    //gr.DrawString(ReverseString(textToDraw), font, Brushes.Black, /*book.Width - fittedSize2.Width*/book.Width, yGap * lineIndex, stringFormat);

                }
                else
                {
                    gr.DrawString(textToDraw, font, Brushes.Black, 0, yGap * lineIndex, sf);
                }


                lineIndex++;

                var yOffset = yGap * lineIndex;
                if ((yOffset + font.Height) > layoutRectangle.Height)
                    break;

                if (string.IsNullOrEmpty(renderCtx.text))
                    break;
            }


        }

        private static void DrawTextToRectangle(CbBook book, RenderTextBookContext ctx, Font font,
            RectangleF layoutRectangle, Graphics gr, StringFormat sf)
        {
            StringBuilder sub = new StringBuilder();
            int charactersFitted;
            int linesFilled;
            SizeF fittedSize = new SizeF();
            const int AppendBlockSize = 512;
            do
            {
                if (ctx.text.Length < AppendBlockSize)
                {
                    sub.Append(ctx.text);
                }
                else
                {
                    sub.Append(ctx.text.Substring(sub.Length, AppendBlockSize));

                }
                fittedSize = gr.MeasureString(sub.ToString(), font, layoutRectangle.Size, sf, out charactersFitted, out linesFilled);
            } while (charactersFitted >= sub.Length && sub.Length < ctx.text.Length);

            string truncatedText = ctx.text.Substring(0, charactersFitted);// + "...";
            ctx.text = ctx.text.Substring(charactersFitted);


            gr.DrawString(truncatedText, font, Brushes.Black, new RectangleF(0, 0, book.Width, fittedSize.Height), sf);
        }

        public class FormattedString
        {
            public string Text;
            public bool IsBold;
            public bool IsItalic;
        }

        public async Task RenderBookFromFB2(CbBook book,
            XDocument doc,
            Font pFont,
            Font boldFont,
            Font italicFont,
            int? maxPages,
            bool onlySpacesBreak = true)
        {
            List<FormattedString> strings = new List<FormattedString>();
            var body = doc.Descendants().First(z => z.Name.LocalName == "body");
            foreach (var item in body.Descendants().Where(z => z.Name.LocalName == "section"))
            {
                //todo use sections to make CB chapters
                foreach (var eitem in item.Elements())
                {
                    if (eitem.Name.LocalName == "p")
                    {
                        foreach (var node in eitem.Nodes())
                        {
                            if (node is XElement element)
                            {
                                strings.Add(new FormattedString()
                                {
                                    Text = element.Value,
                                    IsBold = element.Name.LocalName == "strong",
                                    IsItalic = element.Name.LocalName == "emphasis"
                                });
                            }
                            else if (node is XText textNode)
                            {
                                strings.Add(new FormattedString() { Text = textNode.Value });
                            }
                        }

                    }
                    if (eitem.Name.LocalName == "title")
                    {
                        foreach (var ee in eitem.Elements())
                        {
                            if (ee.Name.LocalName == "p")
                            {
                                strings.Add(new FormattedString() { Text = ee.Value, IsBold = true });
                            }
                        }

                    }
                }

            }
            toolStripProgressBar1.Maximum = strings.Count();

            toolStripProgressBar1.Visible = true;
            await Task.Run(() =>
            {
                Queue<FormattedString> q = new Queue<FormattedString>(strings);
                Graphics gr = null;
                BookExportParams bep = new BookExportParams();

                RectangleF layoutRectangle = new RectangleF(0, 0, book.Width, book.Height - bep.PageInfoHeight);
                Bitmap bmp = null;
                var insertPage = () =>
                {
                    book.InsertPage(book.pages);
                    bmp = book.GetPage(pageNo);

                    gr = Graphics.FromImage(bmp);
                    //gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
                    gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                    //fillRectangle(0, 0, book.Width, book.Height);
                    gr.FillRectangle(Brushes.White, 0, 0, book.Width, book.Height);
                };
                var finalizePage = () =>
                {
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

                };

                insertPage();

                bool lastPageFinalized = false;
                int lineIndex = 0;
                while (q.Any())
                {
                    statusStrip1.Invoke(() =>
                    {
                        toolStripProgressBar1.Value = strings.Count - q.Count;
                    });
                    var deq = q.Dequeue();
                    var font = deq.IsBold ? boldFont : pFont;
                    font = deq.IsItalic ? italicFont : font;

                    var truncatedText = deq.Text;

                    // Option 1: To get the size of the entire string if it were drawn within the layout area
                    SizeF textSize = gr.MeasureString(truncatedText, font, layoutRectangle.Size);

                    // Option 2: To get the number of characters and lines that actually fit
                    int charactersFitted;
                    int linesFilled;
                    StringFormat sf = new StringFormat(StringFormatFlags.LineLimit); // Prevent wrapping

                    SizeF fittedSize = gr.MeasureString(truncatedText, font, layoutRectangle.Size, sf, out charactersFitted, out linesFilled);

                    StringFormat stringFormat = new StringFormat(StringFormatFlags.LineLimit);
                    stringFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                    string currentSplit = deq.Text;
                    var yGap = fittedSize.Height / linesFilled;
                    yGap = font.Height;

                    var maxLines = book.Height / yGap;

                    while (true)
                    {
                        var maxChars = GetCharactersThatFitLine(gr, currentSplit, font, book.Width, onlySpacesBreak);
                        var textToDraw = currentSplit.Substring(0, maxChars);
                        SizeF fittedSize2 = gr.MeasureString(textToDraw, font);

                        //todo track last X in order to continue string later
                        gr.DrawString(textToDraw, font, Brushes.Black, 0, yGap * lineIndex, sf);

                        currentSplit = currentSplit.Substring(maxChars);

                        lineIndex++;
                        if (lineIndex >= maxLines - 1)
                        {
                            if (!lastPageFinalized)
                                finalizePage();

                            lastPageFinalized = true;
                            if (maxPages != null && book.pages > maxPages)
                                break;

                            insertPage();
                            lastPageFinalized = false;

                            lineIndex = 0;
                        }

                        if (string.IsNullOrEmpty(currentSplit))
                            break;
                    }
                    if (maxPages != null && book.pages > maxPages)
                        break;
                    //if (lineIndex >= maxLines) {
                    //  break;
                }

                if (!lastPageFinalized)
                    finalizePage();

                statusStrip1.Invoke(() =>
                {
                    toolStripProgressBar1.Visible = false;
                });
            });


        }
        private void createFromTextToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public void FromText(string text)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();


            d.AddStringField("fontName", "Font name", "Verdana");
            d.AddOptionsField("fontNameOpt", "Font name", ["Courier New", "Consolas", "Verdana", "Bookerly", "Literata", "Lora", "PT Serif", "Rambla", "Sens"], 0);
            d.AddBoolField("fontFromList", "Use font list", true);
            d.AddBoolField("pagesLimit", "pagesLimit", true);
            d.AddBoolField("useBPHD", "Bustrophedon", false);
            d.AddBoolField("customLineSpacing", "Line spacing", false);
            d.AddDouble("lineSpacing", "Line spacing", 1.4);

            d.AddDouble("fontSize", "Font size", 16);
            d.AddBoolField("concatStringsUntilDot", "Concat until dot", false);

            Font selectedFont = null;
            d.AddCustomDialogField("fontDialog", "select font", () =>
            {
                FontDialog fd = new FontDialog();
                fd.ShowDialog();
                selectedFont = fd.Font;
            });

            d.AddInt("maxPages", "Max pages", 20);


            if (!d.ShowDialog())
                return;

            var fontName = d.GetStringField("fontName");
            var bphd = d.GetBoolField("useBPHD");
            var customLineSpacing = d.GetBoolField("customLineSpacing");
            var concatStringsUntilDot = d.GetBoolField("concatStringsUntilDot");
            var lineSpacing = (float)d.GetDouble("lineSpacing");

            if (d.GetBoolField("fontFromList"))
                fontName = d.GetOptionsField("fontNameOpt");

            var fontSize = (float)d.GetDouble("fontSize");

            int? pagesLimit = null;
            if (d.GetBoolField("pagesLimit"))
            {
                pagesLimit = d.GetInt("maxPages");
            }
            MemoryStream ms = new MemoryStream();
            CreateEmptyBook(ms);
            InitFromStream(ms);
            var font = new Font(fontName, fontSize);
            if (selectedFont != null)
                font = selectedFont;

            if (concatStringsUntilDot)
            {
                StringBuilder sb = new StringBuilder();
                var splitted = text.Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < splitted.Length-1; i++)
                {
                    var next = splitted[i + 1];
                    if (!next.Trim().Any() || !(char.IsUpper(next.Trim()[0])||'"'==(next.Trim()[0])))
                    {
                        sb.Append(splitted[i]);
                    }
                    else
                    {
                        sb.AppendLine(splitted[i]);
                    }
                }
                sb.Append(splitted[^1]);
                text = sb.ToString();
            }
            RenderBookFromText(book, text, font, pagesLimit, bphd, customLineSpacing, lineSpacing);
            trackBar1.Maximum = book.pages - 1;
        }

        public static void CreateEmptyBook(MemoryStream ms)
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

            Dictionary<int, (FontInfo, Font)> _fonts = new Dictionary<int, (FontInfo, Font)>();
            var doc = XDocument.Load(ofd.FileName);
            foreach (var font in doc.Descendants("font"))
            {
                var fontId = int.Parse(font.Attribute("id").Value);
                var bold = bool.Parse(font.Attribute("bold").Value);
                var italic = bool.Parse(font.Attribute("italic").Value);
                var family = font.Attribute("family").Value;
                var size = font.Attribute("size").Value.ToFloat();
                Font font1 = null;
                try
                {
                    var split = family.Split(['+', '-']);
                    var ss = split[1];
                    var cap = SplitByCapitalLetters(ss);
                    var cands = FontFamily.Families.Where(z => cap.All(u => z.ToString().Contains(u))).ToArray();

                    font1 = new Font(cands[0], size, bold ? FontStyle.Bold : FontStyle.Regular);

                }
                catch (Exception ex)
                {

                }

                _fonts.Add(fontId, (new FontInfo()
                {
                    Family = family,
                    IsBold = bold,
                    IsItalic = italic,
                    Size = size
                }, font1));
            }

            foreach (var pageItem in doc.Descendants("page"))
            {

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

                foreach (var wordInfo in pageItem.Elements("word"))
                    foreach (var letterInfo in wordInfo.Elements("letter"))
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

                foreach (var wordInfo in pageItem.Elements("word"))
                {
                    var x = wordInfo.Attribute("x").Value.ToDouble();
                    var y = wordInfo.Attribute("y").Value.ToDouble();
                    var w = wordInfo.Attribute("w").Value.ToDouble();
                    var h = wordInfo.Attribute("h").Value.ToDouble();
                    // var locX = wordInfo.Attribute("locationX").Value.ToDouble();
                    // var locY = wordInfo.Attribute("locationY").Value.ToDouble();
                    //var fontId = int.Parse(wordInfo.Attribute("fontId").Value);

                    // x = x;
                    // y = y;

                    Font font = null;
                    // if (_fonts[fontId].Item2 != null)
                    //    font = _fonts[fontId].Item2;
                    // else
                    font = new Font("Courier New", (float)h / 2);

                    // Text fits, draw it normally
                    var kx = book.Width / realPageW;
                    var ky = (book.Height - bep.PageInfoHeight) / realPageH;
                    x -= minX;
                    y -= minY;
                    x *= kx;
                    y *= ky;
                    // Define the text format flags for centering
                    TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
                    // Draw the letter within the rectangle, centered
                    //TextRenderer.DrawText(gr, letterInfo.Attribute("letter").Value, font, new Rectangle((int)x, (int)y, (int)w, (int)h), Color.Blue, flags);

                    gr.DrawString(wordInfo.Attribute("text").Value, font, Brushes.Black, (float)x, (float)y);
                    //gr.DrawRectangle(Pens.Black, (float)x, (float)y, (float)w, (float)h);

                    //foreach (var letterInfo in wordInfo.Elements("letter"))
                    //{
                    //    var x = letterInfo.Attribute("x").Value.ToDouble();
                    //    var y = letterInfo.Attribute("y").Value.ToDouble();
                    //    var w = letterInfo.Attribute("w").Value.ToDouble();
                    //    var h = letterInfo.Attribute("h").Value.ToDouble();
                    //    var locX = letterInfo.Attribute("locationX").Value.ToDouble();
                    //    var locY = letterInfo.Attribute("locationY").Value.ToDouble();
                    //    var fontId = int.Parse(letterInfo.Attribute("fontId").Value);

                    //    x = locX;
                    //    y = locY;

                    //    Font font = null;
                    //    if (_fonts[fontId].Item2 != null)
                    //        font = _fonts[fontId].Item2;
                    //    else
                    //        font = new Font("Courier New", (float)h / 2);

                    //    // Text fits, draw it normally
                    //    var kx = book.Width / realPageW;
                    //    var ky = (book.Height - bep.PageInfoHeight) / realPageH;
                    //    x -= minX;
                    //    y -= minY;
                    //    x *= kx;
                    //    y *= ky;
                    //    // Define the text format flags for centering
                    //    TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
                    //    // Draw the letter within the rectangle, centered
                    //    //TextRenderer.DrawText(gr, letterInfo.Attribute("letter").Value, font, new Rectangle((int)x, (int)y, (int)w, (int)h), Color.Blue, flags);

                    //    gr.DrawString(letterInfo.Attribute("letter").Value, font, Brushes.Black, (float)x, (float)y);
                    //    //gr.DrawRectangle(Pens.Black, (float)x, (float)y, (float)w, (float)h);
                    //}
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

        private async void createFromFB2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "FB2 (.fb2)|*.fb2";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var d = AutoDialog.DialogHelpers.StartDialog();
            MemoryStream ms = new MemoryStream();
            CreateEmptyBook(ms);
            InitFromStream(ms);

            d.AddStringField("fontName", "Font name", "Verdana");
            d.AddOptionsField("fontNameOpt", "Font name", ["Courier New", "Verdana", "Bookerly", "Literata", "Lora", "PT Serif", "Rambla", "Sens"], 0);
            d.AddBoolField("fontFromList", "Use font list", true);
            d.AddBoolField("pagesLimit", "pagesLimit", true);
            d.AddBoolField("onlySpacesBreak", "Only spaces break", true);

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

            await RenderBookFromFB2(book,
                XDocument.Load(ofd.FileName),
                new Font(fontName, fontSize),
                new Font(fontName, fontSize, FontStyle.Bold),
                new Font(fontName, fontSize, FontStyle.Italic),
                pagesLimit,
                d.GetBoolField("onlySpacesBreak")
                );

            trackBar1.Maximum = book.pages - 1;
        }

        private void fromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FromText(Clipboard.GetText());
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var text = File.ReadAllText(ofd.FileName);
            FromText(text);
        }

        Stack<byte[]> undos = new Stack<byte[]>();
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            if (undos.Count == 0)
                return;

            var pop = undos.Pop();

            MemoryStream ms = new MemoryStream(pop);
            book = new CbBook(ms);
            trackBar1.Maximum = book.pages - 1;
            showPage();
        }

        private void toolStripDropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void mergeBooksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var book1 = new CbBook(ofd.FileName);
            for (int i = 0; i < book1.pages; i++)
            {
                var page = book1.GetPage(i);
                book.InsertPage(book.pages);
                var bmp = page;
                using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                {
                    var buf = BookExportContext.GetBuffer(clone, true);
                    book.UpdatePage(buf, book.pages - 1);
                }
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MemoryStream ms = new MemoryStream();
            CreateEmptyBook(ms);
            InitFromStream(ms);

            book.InsertPage(0);
            showPage();
        }

        private void insert1bppImgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Bmp|*.bmp";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            var bmp = Bitmap.FromFile(ofd.FileName) as Bitmap;

            using var mat = bmp.ToMat();

            using var bmp1 = mat.ToBitmap();

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
    }
}