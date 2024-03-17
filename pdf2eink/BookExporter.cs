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
                else if (c1 != 0)
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
            bool was = false;
            for (int i = 0; i < top1.Width; i++)
            {
                var c1 = CountNonZeroY(top1, i);
                if (c1 == 0)
                {
                    if (was)
                        accum++;
                }
                else
                {
                    was = true;
                    accum = 0;
                }
                if (accum >= gap)
                {
                    accum = 0;
                    if (was)
                        hh.Add(i);
                    was = false;
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

                //fs.Write(Encoding.UTF8.GetBytes("CB" + '\0'));
                fs.Write(Encoding.UTF8.GetBytes("CB"));
                fs.WriteByte(0x1);//version CB format
                if (eparams.TOC != null && eparams.TOC.Items.Count > 0)
                    fs.WriteByte(0x1);//format . 1 -with TOC
                                      //wite TOC here                  
                else
                    fs.WriteByte(0x0);//format . 0 -simple without meta info

                fs.Write(BitConverter.GetBytes(pp.Pages));
                fs.Write(BitConverter.GetBytes((ushort)eparams.Width));//width
                fs.Write(BitConverter.GetBytes((ushort)eparams.Height));//heigth

                if (eparams.TOC != null && eparams.TOC.Items.Count > 0)
                {
                    fs.Write(BitConverter.GetBytes((int)eparams.TOC.Items.Count));
                    var offset = fs.Position;
                    fs.Write(BitConverter.GetBytes(0));//total section len in bytes
                    foreach (var item in eparams.TOC.Items)
                    {
                        var h1 = Encoding.UTF8.GetBytes(item.Header).ToList();
                        //while (h1.Count % 4 != 0)
                        //     h1.Add(0);
                        //     
                        fs.Write(BitConverter.GetBytes(item.Page));
                        fs.Write(BitConverter.GetBytes((ushort)item.Ident));
                        fs.Write(BitConverter.GetBytes((ushort)h1.Count));
                        fs.Write(h1.ToArray());
                    }
                    var totalLen = fs.Position - offset + 4;
                    var offset2 = fs.Position;
                    fs.Seek(offset, SeekOrigin.Begin);
                    fs.Write(BitConverter.GetBytes((int)totalLen));
                    fs.Seek(offset2, SeekOrigin.Begin);
                }

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

                                var rtotal = total;
                                if (eparams.FlyRead)
                                {
                                    rtotal = FlyRead(total);
                                }

                                // total.Line(590, 0, 590, 448, Scalar.Black);
                                //total.PutText((pages + 1) + " / " + pdoc.PageCount * 2,new OpenCvSharp.Point(591,5),HersheyFonts.HersheySimplex,1,Scalar.Black)
                                using var temp1 = rtotal.CvtColor(ColorConversionCodes.GRAY2BGR);
                                using var bmp1 = temp1.ToBitmap();
                                if (rtotal != total)
                                {
                                    rtotal.Dispose();
                                }
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

                ctx.UpdatePages();
            }
            if (eparams.Finish != null)
                eparams.Finish();
        }

        private Mat FlyRead(Mat mat2)
        {

            BookExporter bex = new BookExporter();

            Mat result = null;
            //using (var mat2 = mat.CvtColor(ColorConversionCodes.BGR2GRAY))
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
                    using (var sub = threshold.SubMat(lastY, cuts[i], 0, mat2.Cols))
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
                            mat3.Line(mat3.Width - 1, 0, mat3.Width - 1, mat3.Height, Scalar.Black, 2);
                            var roi2 = new Mat(result, new Rect(0, lastY, mat3.Width, mat3.Height));
                            mat3.CopyTo(roi2);
                            //mat3.SaveImage(Path.Combine("temp", "combo.png"));
                        }
                        else
                        {
                            var mat3 = new Mat(threshold, new Rect(0, lastY, threshold.Width, cuts[i] - lastY));

                            var roi2 = new Mat(result, new Rect(0, lastY, mat3.Width, mat3.Height));
                            mat3.CopyTo(roi2);
                            result.Line(0, lastY, 0, lastY + mat3.Height, Scalar.Black, 2);

                        }
                    }
                    lastY = cuts[i];
                }
            }
            return result;
        }
    }
}
