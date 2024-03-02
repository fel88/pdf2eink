using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static pdf2eink.Form1;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Windows.Forms;
using OpenCvSharp.Internal.Vectors;
using System.Windows.Forms.VisualStyles;

namespace pdf2eink
{
    public class BookExporter
    {

        private int CountNonZeroX(Mat mat, int y)
        {
            using (var sub = mat.SubMat(y, y + 1, 0, mat.Cols))
            {
                return mat.Cols - sub.CountNonZero();
            }
        }
        private int CountNonZeroY(Mat mat, int x)
        {
            using (var sub = mat.SubMat(0, mat.Rows, x, x + 1))
            {
                return mat.Rows - sub.CountNonZero();
            }
        }

        public static Mat Threshold(Mat top, BookExportParams eparams)
        {
            if (eparams.AdaptiveThreshold)
            {
                Mat ret = null;
                Mat gray = null;
                if (top.Channels() == 3)
                {
                    gray = top.CvtColor(ColorConversionCodes.RGB2GRAY);
                    top = gray;
                }
                ret = top.AdaptiveThreshold(255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 7, 11);
                if (gray != null)
                    gray.Dispose();

                return ret;
            }
            return top.Threshold(eparams.MinGray, 255, ThresholdTypes.Binary);
        }
        private int GetSafeY(Mat rmat, BookExportParams eparams)
        {
            using (var top1 = Threshold(rmat, eparams))
            {
                //top1.SaveImage("temp11.png");
                for (int i = 0; i < 20; i++)
                {
                    var c1 = CountNonZeroX(top1, top1.Height / 2 + i);
                    var c2 = CountNonZeroX(top1, top1.Height / 2 - i);
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

        public int[] GetHorizontalCuts(Mat top1)
        {
            List<int> hh = new List<int>();

            bool f = false;
            for (int i = 0; i < top1.Height; i++)
            {
                var c1 = CountNonZeroX(top1, i);
                if (c1 == 0 && f)
                {
                    hh.Add(i);
                    f = false;
                }
                else
                {
                    f = true;
                }
            }

            return hh.ToArray();
        }

        public int[] GetVerticalCuts(Mat top1, int gap)
        {
            List<int> hh = new List<int>();            
            int accum = 0;
            for (int i = 0; i < top1.Width; i++)
            {
                var c1 = CountNonZeroY(top1, i);
                if (c1 == 0 )
                {
                    accum++;                    
                }
                else
                {
                    accum = 0;
                }
                if (accum >= gap)
                {
                    accum = 0;
                    hh.Add(i);                    
                }                
            }

            return hh.ToArray();
        }

        public class BookExportContext
        {
            public int Pages;
            public FileStream Stream;
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

            public void AppendPage(Bitmap clone)
            {
                //clone.Save(i + "_page_0.bmp");
                var bts = GetBuffer(clone);
                Stream.Write(bts);
                Pages++;
            }

            internal void UpdatePages()
            {
                Stream.Seek(4, SeekOrigin.Begin);
                Stream.Write(BitConverter.GetBytes(Pages));
            }
        }

        public void ExportToInternalFormat(BookExportParams eparams,
            IPagesProvider pp, string outputFileName,
            Func<PageInfo, ExportResult> action = null)
        {

            using (var fs = new FileStream(outputFileName, FileMode.Create))
            {
                BookExportContext ctx = new BookExportContext();
                ctx.Stream = fs;
                {
                    fs.Write(Encoding.UTF8.GetBytes("CB"));
                    fs.Write(BitConverter.GetBytes((byte)0));//format . 0 -simple without meta info
                    fs.Write(BitConverter.GetBytes(pp.Pages));
                    fs.Write(BitConverter.GetBytes((ushort)eparams.Width));//width
                    fs.Write(BitConverter.GetBytes((ushort)eparams.Height));//heigth

                    //var bounds = pdoc.GetTextBounds(new PdfTextSpan(0, 0, 0));



                    int sp = 0;
                    int ep = pp.Pages;
                    if (eparams.UsePagesLimit)
                    {
                        ep = Math.Min(eparams.EndPage, pp.Pages);
                        sp = Math.Max(0, eparams.StartPage);
                    }
                    for (int i = sp; i < ep; i++)
                    {
                        if (eparams.Progress != null)
                            eparams.Progress(i - sp, ep - sp);
                        /*statusStrip1.Invoke(() =>
                        {
                            toolStripProgressBar1.Maximum = ep - sp;
                            toolStripProgressBar1.Value = i - sp;
                            toolStripProgressBar1.Visible = true;
                            double perc = 100.0 * toolStripProgressBar1.Value / (double)toolStripProgressBar1.Maximum;
                            toolStripStatusLabel1.Text = $"progress: {i - sp}/{ep - sp} {Math.Round(perc, 1)}%";
                        });*/

                        using var img = pp.GetPage(i);

                        if (img == null)
                            continue;

                        using var tmat = img.ToMat();
                        List<Mat> mats = new List<Mat>();
                        if (eparams.SplitWhenAspectGreater)
                        {
                            var aspect = ((decimal)tmat.Width / tmat.Height) * 100;
                            if (aspect > eparams.AspectSplitLimit)
                            {
                                Rect[] rects = new Rect[2]
                                      {
                                                new Rect (0, 0, tmat.Width/2,tmat.Height),
                                                new Rect (tmat.Width/2, 0, tmat.Width/2,tmat.Height ),
                                      };
                                foreach (var item in rects)
                                {
                                    var top = new Mat(tmat, item);
                                    mats.Add(top);
                                }
                            }
                            else
                            {
                                mats.Add(tmat.Clone());
                            }
                        }
                        else
                            mats.Add(tmat.Clone());

                        foreach (var mat in mats)
                        {
                            using var mat2 = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
                            using Mat inv = new Mat();
                            Cv2.BitwiseNot(mat2, inv);
                            using var thr = Threshold(inv, eparams);
                            using var coords = thr.FindNonZero();
                            var rect = Cv2.BoundingRect(coords);
                            if (rect.Width == 0 || rect.Height == 0)
                                continue;

                            using var mat3 = mat2.Clone(rect);

                            using var rmat = mat3.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height * 2));
                            //search safe cut line
                            var safeY = GetSafeY(rmat, eparams);
                            //rmat.Height / 2
                            Rect[] rects = new Rect[2]
                            {
                                                new Rect (0, 0, rmat.Width, safeY),
                                                new Rect (0, safeY, rmat.Width, rmat.Height - safeY),
                            };
                            foreach (var item in rects)
                            {
                                using var top = new Mat(rmat, item);
                                if (eparams.RenderPageInfo)
                                {
                                    using var topResized = top.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height - eparams.PageInfoHeight));
                                    using var total = new Mat(new OpenCvSharp.Size(eparams.Width, eparams.Height), MatType.CV_8UC1);
                                    topResized.CopyTo(total.RowRange(0, eparams.Height - eparams.PageInfoHeight).ColRange(0, eparams.Width));
                                    total.Rectangle(new Rect(0, eparams.Height - eparams.PageInfoHeight, eparams.Width, eparams.PageInfoHeight), Scalar.White, -1);
                                    // total.Line(590, 0, 590, 448, Scalar.Black);
                                    //total.PutText((pages + 1) + " / " + pdoc.PageCount * 2,new OpenCvSharp.Point(591,5),HersheyFonts.HersheySimplex,1,Scalar.Black)
                                    using var temp1 = total.CvtColor(ColorConversionCodes.GRAY2BGR);
                                    using var bmp1 = temp1.ToBitmap();
                                    using (var gr = Graphics.FromImage(bmp1))
                                    {
                                        var hh = eparams.Height - eparams.PageInfoHeight - 1;
                                        gr.DrawLine(Pens.Black, 0, hh, eparams.Width, hh);
                                        var str = (ctx.Pages + 1) + " / " + pp.Pages * 2;
                                        /*for (int z = 0; z < str.Length; z++)
                                        {
                                            gr.DrawString(str[z].ToString(), new Font("Courier New", 6),
                                         Brushes.Black, 0, 5 + z * 10);
                                        }*/
                                        var ms = gr.MeasureString("99999 / 99999", new Font("Consolas", 7));

                                        int xx = (ctx.Pages * 15) % (int)(eparams.Width - ms.Width - 1);
                                        gr.DrawString(str.ToString(), new Font("Consolas", 7),
                                         Brushes.Black, xx, hh - 1);
                                    }
                                    var matTotal = bmp1.ToMat();
                                    using (var top1 = Threshold(matTotal, eparams))
                                    {
                                        if (top1.Width > 0 && top1.Height > 0)
                                        {
                                            //top1.SaveImage(i + "_page_0.bmp");
                                            using (var bmp = top1.ToBitmap())
                                            {
                                                if (action != null)
                                                {
                                                    var res = action(new PageInfo()
                                                    {
                                                        Page = ctx.Pages,
                                                        Bmp = bmp.Clone() as Bitmap
                                                    });
                                                    if (res.Terminate)
                                                        return;
                                                }
                                                using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                                                {
                                                    ctx.AppendPage(clone);
                                                }
                                            }

                                        }
                                    }
                                }
                                else
                                    using (var topResized = top.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height)))
                                    using (var top1 = Threshold(topResized, eparams))
                                    {
                                        if (top1.Width > 0 && top1.Height > 0)
                                        {

                                            //top1.SaveImage(i + "_page_0.bmp");
                                            using (var bmp = top1.ToBitmap())
                                            {
                                                using (var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed))
                                                {
                                                    ctx.AppendPage(clone);
                                                }
                                            }
                                        }
                                    }
                            }
                            //using (var bottom = new Mat(rmat, new Rect(0, safeY, rmat.Width, rmat.Height - safeY)))


                        }
                        foreach (var item in mats)
                        {
                            item.Dispose();
                        }
                    }
                }

                ctx.UpdatePages();

            }
            if (eparams.Finish != null)
                eparams.Finish();
            /*statusStrip1.Invoke(() =>
            {
                toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
                toolStripStatusLabel1.Text = "done";
                toolStripProgressBar1.Visible = false;
                MessageBox.Show("done: " + outputFileName);
            });*/
        }


    }
}
