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

namespace pdf2eink
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private byte[] GetBuffer(Bitmap bmp)
        {

            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
             bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
             bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes); bmp.UnlockBits(bmpData);

            return rgbValues;
        }

        bool renderPageInfo = false;
        public class ExportResult
        {
            public bool Terminate;
        }
        public class PageInfo
        {
            public Bitmap Bmp;
            public int Page;
        }
        void ExportToInternalFormat(string fileName, string outputFileName, Func<PageInfo, ExportResult> action = null)
        {

            using (var fs = new FileStream(outputFileName, FileMode.Create))
            {
                int pages = 0;
                using (var pdoc = PdfDocument.Load(fileName))
                {
                    fs.Write(Encoding.UTF8.GetBytes("CB"));
                    fs.Write(BitConverter.GetBytes((byte)0));//format . 0 -simple without meta info
                    fs.Write(BitConverter.GetBytes(pdoc.PageCount));
                    fs.Write(BitConverter.GetBytes((ushort)600));//width
                    fs.Write(BitConverter.GetBytes((ushort)448));//heigth

                    //var bounds = pdoc.GetTextBounds(new PdfTextSpan(0, 0, 0));

                    var fn = Path.GetFileNameWithoutExtension(fileName);


                    for (int i = 0; i < pdoc.PageCount; i++)
                    {
                        progressBar1.Invoke(() =>
                        {
                            progressBar1.Maximum = pdoc.PageCount;
                            progressBar1.Value = i;
                        });

                        using (var img = pdoc.Render(i, 300, 300, PdfRenderFlags.CorrectFromDpi) as Bitmap)
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
                                        if (rect.Width == 0 || rect.Height == 0)
                                            continue;

                                        var mat3 = mat2.Clone(rect);

                                        using (var rmat = mat3.Resize(new OpenCvSharp.Size(600, 448 * 2)))
                                        {
                                            //search safe cut line
                                            var safeY = GetSafeY(rmat);
                                            //rmat.Height / 2
                                            Rect[] rects = new Rect[2]
                                            {
                                                new Rect (0, 0, rmat.Width, safeY),
                                                new Rect (0, safeY, rmat.Width, rmat.Height - safeY),
                                            };
                                            foreach (var item in rects)
                                            {
                                                using (var top = new Mat(rmat, item))
                                                {
                                                    if (renderPageInfo)
                                                    {
                                                        using (var topResized = top.Resize(new OpenCvSharp.Size(600, 440)))
                                                        {
                                                            using (var total = new Mat(new OpenCvSharp.Size(600, 448), MatType.CV_8UC1))
                                                            {
                                                                topResized.CopyTo(total.RowRange(0, 440).ColRange(0, 600));
                                                                total.Rectangle(new Rect(0, 440, 600, 8), Scalar.White, -1);
                                                                // total.Line(590, 0, 590, 448, Scalar.Black);
                                                                //total.PutText((pages + 1) + " / " + pdoc.PageCount * 2,new OpenCvSharp.Point(591,5),HersheyFonts.HersheySimplex,1,Scalar.Black)
                                                                using (var temp1 = total.CvtColor(ColorConversionCodes.GRAY2BGR))
                                                                {
                                                                    using (var bmp1 = temp1.ToBitmap())
                                                                    {
                                                                        using (var gr = Graphics.FromImage(bmp1))
                                                                        {
                                                                            gr.DrawLine(Pens.Black, 0, 439, 600, 439);
                                                                            var str = (pages + 1) + " / " + pdoc.PageCount * 2;
                                                                            /*for (int z = 0; z < str.Length; z++)
                                                                            {
                                                                                gr.DrawString(str[z].ToString(), new Font("Courier New", 6),
                                                                             Brushes.Black, 0, 5 + z * 10);
                                                                            }*/
                                                                            var ms = gr.MeasureString("99999 / 99999", new Font("Consolas", 7));

                                                                            int xx = (pages * 15) % (int)(600 - ms.Width - 1);
                                                                            gr.DrawString(str.ToString(), new Font("Consolas", 7),
                                                                             Brushes.Black, xx, 438);
                                                                        }
                                                                        var matTotal = bmp1.ToMat();
                                                                        using (var top1 = matTotal.Threshold(200, 255, ThresholdTypes.Binary))
                                                                        {
                                                                            if (top1.Width > 0 && top1.Height > 0)
                                                                            {
                                                                                //top1.SaveImage(i + "_page_0.bmp");
                                                                                using (var bmp = top1.ToBitmap())
                                                                                {
                                                                                    if (action != null)
                                                                                    {
                                                                                        var res = action(new PageInfo() { Page = pages, Bmp = bmp.Clone() as Bitmap });
                                                                                        if (res.Terminate)
                                                                                            return;
                                                                                    }
                                                                                    using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                                                                                    {
                                                                                        //clone.Save(i + "_page_0.bmp");
                                                                                        var bts = GetBuffer(clone);
                                                                                        fs.Write(bts);
                                                                                        pages++;
                                                                                    }
                                                                                }

                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                        using (var topResized = top.Resize(new OpenCvSharp.Size(600, 448)))
                                                        using (var top1 = topResized.Threshold(200, 255, ThresholdTypes.Binary))
                                                        {
                                                            if (top1.Width > 0 && top1.Height > 0)
                                                            {

                                                                //top1.SaveImage(i + "_page_0.bmp");
                                                                using (var bmp = top1.ToBitmap())
                                                                {
                                                                    using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                                                                    {
                                                                        //clone.Save(i + "_page_0.bmp");
                                                                        var bts = GetBuffer(clone);
                                                                        fs.Write(bts);
                                                                        pages++;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                }
                                            }
                                            //using (var bottom = new Mat(rmat, new Rect(0, safeY, rmat.Width, rmat.Height - safeY)))


                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                fs.Seek(4, SeekOrigin.Begin);
                fs.Write(BitConverter.GetBytes(pages));
            }
            progressBar1.Invoke(() =>
            {
                progressBar1.Value = progressBar1.Maximum;
                MessageBox.Show("done: " + outputFileName);
            });
        }

        private int CountNonZero(Mat mat, int y)
        {
            using (var sub = mat.SubMat(y, y + 1, 0, mat.Cols))
            {
                return mat.Cols - sub.CountNonZero();
            }
        }
        private int GetSafeY(Mat rmat)
        {
            using (var top1 = rmat.Threshold(200, 255, ThresholdTypes.Binary))
            {
                //top1.SaveImage("temp11.png");
                for (int i = 0; i < 20; i++)
                {
                    var c1 = CountNonZero(top1, top1.Height / 2 + i);
                    var c2 = CountNonZero(top1, top1.Height / 2 - i);
                    if (c1 == 0)
                    {
                        return top1.Height / 2 + i;
                    }
                    if (c2 == 0)
                    {
                        return top1.Height / 2 - i;
                    }
                }
                return rmat.Height / 2;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            if (checkBox1.Checked)
            {
                SaveFileDialog sfd = new SaveFileDialog();

                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;

                }
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
                    ExportToInternalFormat(ofd.FileName, sfd.FileName, action);
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
                    using (var img = pdoc.Render(i, 300, 300, PdfRenderFlags.CorrectFromDpi) as Bitmap)
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

                                            using (var top1 = top.Threshold(200, 255, ThresholdTypes.Binary))
                                            {
                                                using (var bottom1 = bottom.Threshold(200, 255, ThresholdTypes.Binary))
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

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
                    using (var img = pdoc.Render((int)numericUpDown1.Value, 300, 300, PdfRenderFlags.CorrectFromDpi) as Bitmap)
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
                                        else using (var top1 = rmat.Threshold(200, 255, ThresholdTypes.Binary))
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

        private void cbRenderPageInfo_CheckedChanged(object sender, EventArgs e)
        {
            renderPageInfo = cbRenderPageInfo.Checked;
        }

        bool previewOnly;
        private void cbPreviewOnly_CheckedChanged(object sender, EventArgs e)
        {
            previewOnly = cbPreviewOnly.Checked;
        }

        bool wearLeveling;
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            wearLeveling = checkBox3.Checked;
        }
    }
}