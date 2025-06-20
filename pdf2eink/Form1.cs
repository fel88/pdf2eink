using PdfiumViewer;
using OpenCvSharp.Extensions;
using OpenCvSharp;
using System.Drawing.Imaging;
using DitheringLib;
using System.Collections.Immutable;

namespace pdf2eink
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 1;
        }



        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void ExportToBmpSeries(string fileName)
        {
            using (var pdoc = PdfDocument.Load(fileName))
            {
                var bounds = pdoc.GetTextBounds(new PdfTextSpan(0, 0, 0));

                var fn = Path.GetFileNameWithoutExtension(fileName);
                Directory.CreateDirectory(fn);
                var bb = System.Reflection.Assembly.GetExecutingAssembly().Location;
                Directory.SetCurrentDirectory(Path.Combine(bb, fn));
                for (int i = 0; i < pdoc.PageCount; i++)
                {
                    using (var img = pdoc.Render(i, dpi, dpi, PdfRenderFlags.CorrectFromDpi) as Bitmap)
                    using (var mat = img.ToMat())
                    {
                        using (var mat2 = mat.CvtColor(ColorConversionCodes.BGR2GRAY))
                        {
                            using (Mat inv = new Mat())
                            {
                                Cv2.BitwiseNot(mat2, inv);

                                using (var coords = inv.FindNonZero())
                                {
                                    var rect = Cv2.BoundingRect(coords);
                                    var mat3 = mat2.Clone(rect);

                                    using (var rmat = mat3.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height * 2)))
                                    using (var top = new Mat(rmat, new Rect(0, 0, rmat.Width, rmat.Height / 2)))
                                    {
                                        using (var bottom = new Mat(rmat, new Rect(0, rmat.Height / 2, rmat.Width, rmat.Height / 2)))
                                        {
                                            //var res = top.AdaptiveThreshold(255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 7, 11);
                                            using (var top1 = BookExporter.Threshold(top, eparams))
                                            {
                                                using (var bottom1 = BookExporter.Threshold(bottom, eparams))
                                                {
                                                    if (top1.Width > 0 && top1.Height > 0)
                                                    {
                                                        //top1.SaveImage(i + "_page_0.bmp");
                                                        using (var bmp = top1.ToBitmap())
                                                        {
                                                            using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                                                            {
                                                                clone.Save(i + "_page_0.bmp");
                                                            }
                                                        }
                                                    }

                                                    if (bottom1.Width > 0 && bottom1.Height > 0)
                                                    {
                                                        //bottom1.SaveImage(i + "_page_1.bmp");
                                                        using (var bmp = bottom1.ToBitmap())
                                                        {
                                                            using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                                                            {
                                                                clone.Save(i + "_page_1.bmp");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


            }
        }



        bool internalFormat = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            internalFormat = checkBox1.Checked;
        }

        //private void button2_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog ofd = new OpenFileDialog();
        //    if (ofd.ShowDialog() != DialogResult.OK) return;

        //    using (var pdoc = PdfDocument.Load(ofd.FileName))
        //    {
        //        var bounds = pdoc.GetTextBounds(new PdfTextSpan(0, 0, 0));

        //        var fn = Path.GetFileNameWithoutExtension(ofd.FileName);

        //        {
        //            using (var img = pdoc.Render((int)numericUpDown1.Value, dpi, dpi, PdfRenderFlags.CorrectFromDpi) as Bitmap)
        //            using (var mat = img.ToMat())
        //            {
        //                using (var mat2 = mat.CvtColor(ColorConversionCodes.BGR2GRAY))
        //                {
        //                    using (Mat inv = new Mat())
        //                    {
        //                        Cv2.BitwiseNot(mat2, inv);

        //                        using (var coords = inv.FindNonZero())
        //                        {
        //                            var rect = Cv2.BoundingRect(coords);
        //                            var mat3 = mat2.Clone(rect);

        //                            using (var rmat = mat3.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height * 2)))
        //                            {
        //                                if (checkBox2.Checked)
        //                                {
        //                                    Dithering d = new Dithering();
        //                                    pictureBox1.Image = d.Process(rmat.ToBitmap());
        //                                }
        //                                else using (var top1 = BookExporter.Threshold(rmat, eparams))
        //                                    {
        //                                        pictureBox1.Image = top1.ToBitmap();
        //                                    }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }


        //}

        BookExportParams eparams = new BookExportParams();

        private void cbRenderPageInfo_CheckedChanged(object sender, EventArgs e)
        {
            eparams.RenderPageInfo = cbRenderPageInfo.Checked;
        }

        bool previewOnly;
        private void cbPreviewOnly_CheckedChanged(object sender, EventArgs e)
        {
            previewOnly = cbPreviewOnly.Checked;
        }

        bool wearLeveling = true;
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            wearLeveling = checkBox3.Checked;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            eparams.MinGray = (int)numericUpDown2.Value;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            eparams.UsePagesLimit = checkBox4.Checked;
            numericUpDown4.Enabled = checkBox4.Checked;
            numericUpDown3.Enabled = checkBox4.Checked;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            eparams.StartPage = (int)numericUpDown3.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            eparams.EndPage = (int)numericUpDown4.Value;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            eparams.SplitWhenAspectGreater = checkBox5.Checked;
            numericUpDown5.Enabled = eparams.SplitWhenAspectGreater;
        }
        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            eparams.AspectSplitLimit = (int)numericUpDown5.Value;
        }

        int dpi = 300;

        private void numericUpDown6_ValueChanged(object sender, EventArgs e)
        {
            dpi = (int)numericUpDown6.Value;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            eparams.AdaptiveThreshold = checkBox6.Checked;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var spl = comboBox1.Text.Split('x');
            numericUpDown7.Value = int.Parse(spl[0]);
            numericUpDown8.Value = int.Parse(spl[1]);
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            eparams.Width = (int)numericUpDown7.Value;
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            eparams.Height = (int)numericUpDown8.Value;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            eparams.AutoDithering = checkBox2.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            eparams.Rotate90 = checkBox7.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            eparams.FlyRead = checkBox8.Checked;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "All supported files (*.pdf,*.djvu)|*.djvu;*.pdf|Pdf files (*.pdf)|*.pdf|Djvu files (*.djvu)|*.djvu";

            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            if (!internalFormat)
            {
                ExportToBmpSeries(ofd.FileName);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            MemoryStream ms = new MemoryStream();
            if (!previewOnly)
            {
                sfd.AddExtension = true;
                sfd.DefaultExt = "cb";
                sfd.Filter = "CB files (*.cb)|*.cb";
                sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName);

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;
            }

            Thread th = new Thread(() =>
            {
                Func<PageInfo, ExportResult> action = null;

                /*if (previewOnly)
                {
                    action = (x) =>
                    {
                        var term = numericUpDown1.Value == x.Page;
                        if (term)
                            pictureBox1.Image = x.Bmp;

                        return new ExportResult() { Terminate = term };
                    };
                }*/
                IPagesProvider p1 = null;
                if (ofd.FileNames.Count() > 1)
                {
                    List<IPagesProvider> combo = new List<IPagesProvider>();
                    foreach (var item in ofd.FileNames)
                    {
                        if (item.ToLower().EndsWith("pdf"))
                            combo.Add(new PdfPagesProvider(item));
                        else if (item.ToLower().EndsWith("djvu") || item.ToLower().EndsWith("djv"))
                            combo.Add(new DjvuPagesProvider(item));
                    }
                    p1 = new CombinedPagesProvider(combo.ToArray());
                }
                else
                {
                    if (ofd.FileName.ToLower().EndsWith("pdf"))
                    {
                        p1 = new PdfPagesProvider(ofd.FileName);
                    }
                    else
                    if (ofd.FileName.ToLower().EndsWith("djvu") || ofd.FileName.ToLower().EndsWith("djv"))
                    {
                        p1 = new DjvuPagesProvider(ofd.FileName);
                    }
                }
                p1.Dpi = dpi;

                //ExportToInternalFormat(ofd.FileName, sfd.FileName, action);
                var bex = new BookExporter();
                eparams.Progress = (now, max) =>
                {
                    statusStrip1.Invoke(() =>
                    {
                        toolStripProgressBar1.Maximum = max;
                        toolStripProgressBar1.Value = now;
                        toolStripProgressBar1.Visible = true;
                        double perc = 100.0 * toolStripProgressBar1.Value / (double)toolStripProgressBar1.Maximum;
                        toolStripStatusLabel1.Text = $"progress: {now}/{max} {Math.Round(perc, 1)}%";
                    });
                };

                eparams.Finish = () =>
                {
                    statusStrip1.Invoke(() =>
                    {
                        toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
                        toolStripStatusLabel1.Text = "done";
                        toolStripProgressBar1.Visible = false;
                        if (previewOnly)
                        {
                            if (MessageBox.Show($"Done. Open?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                Viewer v = new Viewer();
                                ms.Seek(0, SeekOrigin.Begin);
                                v.Init(ms, $"stream of {Path.GetFileName(ofd.FileName)}");
                                v.MdiParent = MdiParent;
                                v.Show();
                            }
                        }
                        else
                        {
                            if (MessageBox.Show($"Done: {sfd.FileName}. Open?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                Viewer v = new Viewer();
                                v.Init(sfd.FileName);
                                v.MdiParent = MdiParent;
                                v.Show();
                            }
                        }
                    });
                };

                if (previewOnly)
                    bex.ExportToInternalFormat(eparams, p1, ms, action);
                else
                    bex.ExportToInternalFormat(eparams, p1, sfd.FileName, action);

                if (eparams.Finish != null)
                    eparams.Finish();

                p1.Dispose();
            });
            th.IsBackground = true;
            th.Start();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void fromClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var s = Clipboard.GetText();

            eparams.TOC = new TOC();
            eparams.TOC.Parse(s);
            TOCViewer tocv = new TOCViewer();
            tocv.Init(eparams.TOC);
            tocv.ShowDialog();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            eparams.TOC = new TOC();
            TOCViewer tocv = new TOCViewer();
            tocv.Init(eparams.TOC);
            tocv.ShowDialog();
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            eparams.TiledMode = checkBox9.Checked;
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            eparams.RectifyLetters = checkBox10.Checked;

        }
    }
}