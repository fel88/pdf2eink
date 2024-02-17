using PdfiumViewer;
using OpenCvSharp.Extensions;
using OpenCvSharp;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Drawing.Imaging;
using System.Drawing;
using System.Text;
using System.Data;
using static System.Net.Mime.MediaTypeNames;
using System.Diagnostics;
using DitheringLib;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace pdf2eink
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public class ExportResult
        {
            public bool Terminate;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All supported files (*.pdf,*.djvu)|*.djvu;*.pdf|Pdf files (*.pdf)|*.pdf|Djvu files (*.djvu)|*.djvu";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            if (internalFormat)
            {
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.AddExtension = true;
                sfd.DefaultExt = "cb";
                sfd.Filter = "CB files (*.cb)|*.cb";
                sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName);

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                Thread th = new Thread(() =>
                {
                    Func<PageInfo, ExportResult> action = null;

                    if (previewOnly)
                    {
                        action = (x) =>
                        {
                            var term = numericUpDown1.Value == x.Page;
                            if (term)
                                pictureBox1.Image = x.Bmp;

                            return new ExportResult() { Terminate = term };
                        };
                    }
                    IPagesProvider p1 = null;
                    if (ofd.FileName.ToLower().EndsWith("pdf"))
                    {
                        p1 = new PdfPagesProvider(ofd.FileName);
                    }
                    else
                    if (ofd.FileName.ToLower().EndsWith("djvu") || ofd.FileName.ToLower().EndsWith("djv"))
                    {
                        p1 = new DjvuPagesProvider(ofd.FileName);
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
                                MessageBox.Show("done: " + sfd.FileName);
                            });
                    };

                    bex.ExportToInternalFormat(eparams, p1, sfd.FileName, action);
                    p1.Dispose();
                });
                th.IsBackground = true;
                th.Start();
            }
            else
                ExportToBmpSeries(ofd.FileName);
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

                                    using (var rmat = mat3.Resize(new OpenCvSharp.Size(600, 448 * 2)))
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

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;

            using (var pdoc = PdfDocument.Load(ofd.FileName))
            {
                var bounds = pdoc.GetTextBounds(new PdfTextSpan(0, 0, 0));

                var fn = Path.GetFileNameWithoutExtension(ofd.FileName);

                {
                    using (var img = pdoc.Render((int)numericUpDown1.Value, dpi, dpi, PdfRenderFlags.CorrectFromDpi) as Bitmap)
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

                                    using (var rmat = mat3.Resize(new OpenCvSharp.Size(600, 448 * 2)))
                                    {
                                        if (checkBox2.Checked)
                                        {
                                            Dithering d = new Dithering();
                                            pictureBox1.Image = d.Process(rmat.ToBitmap());
                                        }
                                        else using (var top1 = BookExporter.Threshold(rmat, eparams))
                                            {
                                                pictureBox1.Image = top1.ToBitmap();
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }


        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image.Save("temp1.png");
            ProcessStartInfo startInfo = new ProcessStartInfo("temp1.png");
            //startInfo.Verb = "edit";
            startInfo.UseShellExecute = true;

            Process.Start(startInfo);

        }

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
    }

}