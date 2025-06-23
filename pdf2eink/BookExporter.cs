using DitheringLib;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing.Imaging;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Windows.Forms;

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


        private int GetSafeY(Mat rmat, BookExportParams eparams, int maxDeltaY = 20)
        {
            using var top1 = Threshold(rmat, eparams);
            //top1.SaveImage("temp11.png");
            for (int i = 0; i < maxDeltaY; i++)
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
            hh.Add(top1.Height);
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


        public void ExportToInternalFormat(BookExportParams eparams,
         IPagesProvider pp, string outputFileName,
         Func<PageInfo, ExportResult> action = null)
        {
            using (var fs = new FileStream(outputFileName, FileMode.Create))
            {
                ExportToInternalFormat(eparams, pp, fs, action);
            }
        }

        public void ExportToInternalFormatWithSections(
            BookExportParams eparams,
            IPagesProvider pp, Stream stream,
            Func<PageInfo, ExportResult> action = null
           )
        {
            var fs = stream;
            BookExportContext ctx = new BookExportContext();
            ctx.Stream = stream;

            //fs.Write(Encoding.UTF8.GetBytes("CB" + '\0'));
            fs.Write(Encoding.UTF8.GetBytes("CB"));
            fs.WriteByte(0x2);//version CB format (1 - old, 2 - with sections)
            /*
             * flags
             */
            if (eparams.TOC != null && eparams.TOC.Items.Count > 0)
                fs.WriteByte(0x1);//format . 1 -with TOC
                                  //wite TOC here                  
            else
                fs.WriteByte(0x0);//format . 0 -simple without meta info

            //sections here

            // <len><section ID> 0x10 - book info, 0xA0 - TOC, 0xB0 - pages table, 0xC0 - pages array
            //book info header            
            fs.Write(BitConverter.GetBytes((int)(4 + 1 + 4 + 2 + 2)));
            fs.WriteByte(0x10);

            fs.Write(BitConverter.GetBytes(pp.Pages));
            fs.Write(BitConverter.GetBytes((ushort)eparams.Width));//width
            fs.Write(BitConverter.GetBytes((ushort)eparams.Height));//heigth

            //parse section
            if (eparams.TOC != null)
                AppendTOC(eparams.TOC, fs);

            //var bounds = pdoc.GetTextBounds(new PdfTextSpan(0, 0, 0));
            int sp = 0;
            int ep = pp.Pages;
            if (eparams.UsePagesLimit)
            {
                ep = Math.Min(eparams.EndPage, pp.Pages);
                sp = Math.Max(0, eparams.StartPage);
            }

            var roughPagesCount = ep - sp;
            if (eparams.SplitWhenAspectGreater)
                roughPagesCount *= 2;

            roughPagesCount *= 2;

            long pagesTableHeaderOffset = fs.Position;

            //pages table header            
            fs.Write(BitConverter.GetBytes((int)(9 + roughPagesCount * 4)));
            fs.WriteByte(0xB0);

            //total pages                       
            fs.Write(BitConverter.GetBytes(pp.Pages));
            //table of pages: 4b x pages count
            for (int i = 0; i < roughPagesCount; i++)
            {
                fs.Write(BitConverter.GetBytes(0));
            }

            long pagesArrayHeaderOffset = fs.Position;

            //pages array
            AppendPagesSection(ctx, pp, sp, ep, eparams, action);

            //update pages qty in all sections
            fs.Seek(9, SeekOrigin.Begin);
            fs.Write(BitConverter.GetBytes(ctx.Pages));

            fs.Seek(pagesTableHeaderOffset + 5, SeekOrigin.Begin);
            fs.Write(BitConverter.GetBytes(ctx.Pages));

            //update pages offsets
            fs.Seek(pagesTableHeaderOffset + 9, SeekOrigin.Begin);
            foreach (var item in ctx.PagesOffsets)
            {
                fs.Write(BitConverter.GetBytes((int)(item)));
            }

            //ctx.UpdatePages();          
        }

        public void ExportToInternalFormat(BookExportParams eparams,
       IPagesProvider pp, Stream stream,
       Func<PageInfo, ExportResult> action = null)
        {
            var fs = stream;
            BookExportContext ctx = new BookExportContext();
            ctx.Stream = stream;

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

            fs.Write(BitConverter.GetBytes(pp.Pages));
            fs.Write(BitConverter.GetBytes((ushort)eparams.Width));//width
            fs.Write(BitConverter.GetBytes((ushort)eparams.Height));//heigth

            if (eparams.TOC != null)
                AppendTOC(eparams.TOC, fs);

            //var bounds = pdoc.GetTextBounds(new PdfTextSpan(0, 0, 0));
            int sp = 0;
            int ep = pp.Pages;
            if (eparams.UsePagesLimit)
            {
                ep = Math.Min(eparams.EndPage, pp.Pages);
                sp = Math.Max(0, eparams.StartPage);
            }

            if (eparams.TiledMode)
            {
                //WriteTiledSection();
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

                List<Rect> rects = new List<Rect>();
                var aspect = ((decimal)tmat.Width / tmat.Height) * 100;
                rects.Add(new Rect(0, 0, tmat.Width, tmat.Height));

                if (eparams.SplitWhenAspectGreater && aspect > eparams.AspectSplitLimit)
                {
                    rects.Clear();
                    rects.AddRange(
                         [
                                           new Rect (0, 0, tmat.Width/2,tmat.Height),
                                                new Rect (tmat.Width/2, 0, tmat.Width/2,tmat.Height ),
                              ]);
                }


                foreach (var rect in rects)
                {
                    var mat = new Mat(tmat, rect);

                    MatProcess(mat, i, rect, ctx, eparams, pp, action);
                    mat.Dispose();
                }
            }

            ctx.UpdatePages();

            /* if (eparams.Finish != null)
                 eparams.Finish();*/
        }


        private void AppendPagesSection(BookExportContext ctx, IPagesProvider pp, int sp, int ep, BookExportParams eparams, Func<PageInfo, ExportResult> action)
        {
            var fs = ctx.Stream;
            var startPos = fs.Position;
            ctx.PagesArraySectionOffset = startPos;

            fs.Write(BitConverter.GetBytes((int)(0)));//stub
            fs.WriteByte(0xC0);

            for (int i = sp; i < ep; i++)
            {
                if (eparams.Progress != null)
                    eparams.Progress(i - sp, ep - sp);

                using var img = pp.GetPage(i);

                if (img == null)
                    continue;

                if (img.Height == 0 || img.Width == 0)
                    continue;

                using var tmat = img.ToMat();
                List<Mat> mats = new List<Mat>();


                List<Rect> rects = new List<Rect>();
                var aspect = ((decimal)tmat.Width / tmat.Height) * 100;
                rects.Add(new Rect(0, 0, tmat.Width, tmat.Height));

                if (eparams.SplitWhenAspectGreater && aspect > eparams.AspectSplitLimit)
                {
                    rects.Clear();
                    rects.AddRange(
                         [
                                           new Rect (0, 0, tmat.Width/2,tmat.Height),
                                                new Rect (tmat.Width/2, 0, tmat.Width/2,tmat.Height ),
                              ]);
                }


                foreach (var rect in rects)
                {
                    var mat = new Mat(tmat, rect);

                    MatProcess(mat, i, rect, ctx, eparams, pp, action);
                    mat.Dispose();
                }
            }

            var len = fs.Position - startPos;
            var endPos = fs.Position;
            fs.Seek(startPos, SeekOrigin.Begin);
            fs.Write(BitConverter.GetBytes((int)len));
            fs.Seek(endPos, SeekOrigin.End);
        }

        private void MatProcess(Mat mat, int pageNo, Rect sourceRect, BookExportContext ctx, BookExportParams eparams, IPagesProvider pp, Func<PageInfo, ExportResult> action)
        {
            var ttmat = mat;

            Mat thr = null;
            if (eparams.AutoDithering)
            {
                Dithering d = new Dithering();
                using var tt = mat.ToBitmap();
                using var res = d.Process(tt);
                ttmat = res.ToMat();
            }

            using var mat2 = ttmat.CvtColor(ColorConversionCodes.BGR2GRAY);
            if (eparams.AutoDithering)
            {
                ttmat.Dispose();
                thr = mat2;
            }
            else
            {
                using Mat inv = new Mat();
                Cv2.BitwiseNot(mat2, inv);
                thr = Threshold(inv, eparams);
            }
            using var coords = thr.FindNonZero();
            if (!eparams.AutoDithering)
            {
                thr.Dispose();
            }
            var bound = Cv2.BoundingRect(coords);
            if (bound.Width == 0 || bound.Height == 0)
                return;




            using var mat3 = mat2.Clone(bound);
            LetterInfo[] letters = null;
            if (eparams.RectifyLetters && pp is IPagesProviderWithLetters ppwl3)
            {
                letters = ppwl3.GetPageLetters(pageNo);

                foreach (var item in letters)
                {
                    item.Bound = new RectangleF(item.Bound.Left - bound.X,
                        item.Bound.Top - bound.Y, item.Bound.Width, item.Bound.Height);

                }
            }

            using var rmat = mat3.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height * 2));

            if (eparams.RectifyLetters && pp is IPagesProviderWithLetters ppwl)
            {
                //rmat.SaveImage("before1.png");
                var kx = rmat.Width / (float)mat3.Width;
                var ky = rmat.Height / (float)mat3.Height;
                foreach (var item in letters)
                {
                    item.Bound = new RectangleF(item.Bound.Left * kx,
                        item.Bound.Top * ky, item.Bound.Width * kx, item.Bound.Height * ky);

                }
                //var groups = letters.GroupBy(z => z.Letter + "_" + z.Font).ToArray();

                //foreach (var item in groups)
                //{
                //    var bnd = item.First().Bound;
                //    Mat letter = rmat.Clone(new Rect((int)(bnd.X * kx), (int)(bnd.Y * ky),
                //        (int)(bnd.Width * kx), (int)(bnd.Height * ky)));

                //    foreach (var gg in item.Skip(1))
                //    {
                //        var bnd2 = gg.Bound;
                //        var roi = new Rect((int)(bnd2.X * kx),
                //            (int)(bnd2.Y * ky),
                //            (int)(bnd2.Width * kx),
                //            (int)(bnd2.Height * ky)
                //            );

                //        // Create a sub-Mat representing the ROI in the destination image
                //        Mat destination_roi = rmat.SubMat(roi);

                //        // Copy the source image to the destination ROI
                //        letter.CopyTo(destination_roi);
                //    }
                //}
                //rmat.SaveImage("after1.png");

            }
            //search safe cut line
            var safeY = GetSafeY(rmat, eparams);
            //todo: use smart Y cut. don't resize if no needed just move futher if there is some free space
            //rmat.Height / 2
            Rect[] rects =
            [
               new Rect (0, 0, rmat.Width, safeY),
               new Rect (0, safeY, rmat.Width, rmat.Height - safeY),
            ];

            foreach (var rect in rects)
            {
                bool? flowControl = ProcessPageItem(ctx, eparams, pp, action, rmat, rect, letters);
                switch (flowControl)
                {
                    case false: continue;
                    case true: return;
                }
            }
            //using (var bottom = new Mat(rmat, new Rect(0, safeY, rmat.Width, rmat.Height - safeY)))
        }

        private bool? ProcessPageItem(BookExportContext ctx,
            BookExportParams eparams,
            IPagesProvider pp,
            Func<PageInfo,
            ExportResult> action,
            Mat rmat,
            Rect sourceRect,
            LetterInfo[] letters)
        {
            var top = new Mat(rmat, sourceRect);
            if (eparams.RenderPageInfo)
            {
                using var topResized = top.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height - eparams.PageInfoHeight));
                var ky = (float)topResized.Height / eparams.Height;

                using var total = new Mat(new OpenCvSharp.Size(eparams.Width, eparams.Height), MatType.CV_8UC1);
                topResized.CopyTo(total.RowRange(0, eparams.Height - eparams.PageInfoHeight).ColRange(0, eparams.Width));
                total.Rectangle(new Rect(0, eparams.Height - eparams.PageInfoHeight, eparams.Width, eparams.PageInfoHeight), Scalar.White, -1);

                var rtotal = total;
                if (eparams.FlyRead)
                    rtotal = FlyRead(total);

                // total.Line(590, 0, 590, 448, Scalar.Black);
                //total.PutText((pages + 1) + " / " + pdoc.PageCount * 2,new OpenCvSharp.Point(591,5),HersheyFonts.HersheySimplex,1,Scalar.Black)
                using var temp1 = rtotal.CvtColor(ColorConversionCodes.GRAY2BGR);
                using var bmp1 = temp1.ToBitmap();

                if (rtotal != total)
                    rtotal.Dispose();


                BookExportContext.PrintFooter(ctx.Pages, pp.Pages * 2, bmp1, eparams.PageInfoHeight);

                using var matTotal = bmp1.ToMat();
                using var temp2 = matTotal.CvtColor(ColorConversionCodes.BGR2GRAY);
                using var top1 = Threshold(temp2, eparams);
                if (top1.Width <= 0 || top1.Height <= 0)
                    return false;

                //top1.SaveImage(i + "_page_0.bmp");

                using var bmp = top1.ToBitmap();
                if (action != null)
                {
                    var res = action(new PageInfo()
                    {
                        Page = ctx.Pages,
                        Bmp = bmp.Clone() as Bitmap
                    });
                    if (res.Terminate)
                        return true;
                }
                using var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed);

                if (eparams.RectifyLetters && letters != null)
                {
                    TileProcessor tp = new TileProcessor();
                    tp.Init(clone);
                    tp.MakeGroups();
                    tp.SimplifyMarks();

                    var tiles = tp.ExtractTiles();
                    foreach (var titem in tiles.Infos)
                    {
                        var cx = titem.X + titem.Tile.Bmp.Width / 2;
                        var cy = titem.Y + titem.Tile.Bmp.Height / 2;
                        var ord = letters.OrderBy(z => Math.Abs((z.Bound.X + z.Bound.Width / 2) - cx)
                        + Math.Abs(((z.Bound.Y - sourceRect.Y) * ky  + z.Bound.Height * ky / 2) - cy)).ToArray();
                        if (ord.Count() == 0)
                            continue;

                        var fr = ord.First();
                        var dist = Math.Abs(fr.Bound.X + fr.Bound.Width / 2 - cx) +
                            Math.Abs((fr.Bound.Y - sourceRect.Y) * ky  + fr.Bound.Height * ky / 2 - cy);
                        if (dist > 3)
                            continue;

                        var dist2 = Math.Abs(titem.Tile.Bmp.Height - fr.Bound.Height * ky);
                        if (dist2 > 3)
                            continue;

                        var dist3 = Math.Abs(titem.Tile.Bmp.Width - fr.Bound.Width);
                        if (dist3 > 4)
                            continue;


                        titem.Key = $"{fr.Letter}_{fr.Font}";
                    }

                  //  TilesViewer tv = new TilesViewer();
                  //  tv.Init([tiles]);
                    var lettersModified = letters.Select(z => z.Clone()).ToArray();
                    foreach (var item in lettersModified)
                    {
                        item.Bound = new RectangleF(item.Bound.Left,
                            item.Bound.Top - sourceRect.Y,
                            item.Bound.Width,
                            item.Bound.Height
                            );
                    }
                   // tv.InitAdditionalInfo(lettersModified);
                  //  tv.ShowDialog();
                   // var groups = letters.GroupBy(z => z.Letter + "_" + z.Font).ToArray();
                    //top1.SaveImage("before1.png");
                    using var merged = MergeKeys([tiles]);
                    using var clone2 = merged.Clone(new Rectangle(0, 0, merged.Width, merged.Height), PixelFormat.Format1bppIndexed);

                    ctx.AppendPage(clone2);

                    //foreach (var gitem in groups)
                    //{
                    //    var bnd = gitem.First().Bound;
                    //    if ((bnd.Y + bnd.Height) > top1.Height)
                    //        continue;

                    //    Mat letter = top1.Clone(new Rect((int)(bnd.X), (int)(bnd.Y),
                    //    (int)(bnd.Width), (int)(bnd.Height)));

                    //    foreach (var gg in gitem.Skip(1))
                    //    {
                    //        var bnd2 = gg.Bound;
                    //        var roi = new Rect((int)(bnd2.X),
                    //            (int)(bnd2.Y),
                    //            (int)(bnd2.Width),
                    //            (int)(bnd2.Height)
                    //            );

                    //        // Create a sub-Mat representing the ROI in the destination image
                    //        Mat destination_roi = top1.SubMat(roi);

                    //        // Copy the source image to the destination ROI
                    //        letter.CopyTo(destination_roi);
                    //    }
                    //}
                    //top1.SaveImage("after1.png");
                }
                else
                    ctx.AppendPage(clone);
            }
            else
            {
                using var topResized = top.Resize(new OpenCvSharp.Size(eparams.Width, eparams.Height));
                using var top1 = Threshold(topResized, eparams);
                if (top1.Width <= 0 || top1.Height <= 0)
                    return false;

                //top1.SaveImage(i + "_page_0.bmp");
                using var bmp = top1.ToBitmap();
                using var clone = bmp.Clone(new Rectangle(0, 0, bmp.Width, bmp.Height), PixelFormat.Format1bppIndexed);
                ctx.AppendPage(clone);
            }

            return null;
        }

        public Dictionary<string, Tile> tilesHash = new Dictionary<string, Tile>();

        private Bitmap MergeKeys(TiledPageInfo[] pages)
        {
            //find replaceable candidates
            var grp = pages.SelectMany(z => z.Infos)
                .Where(z => !tilesHash.ContainsKey($"{z.Key}_{z.Tile.Bmp.Width}_{z.Tile.Bmp.Height}"))
                .Where(z => !string.IsNullOrEmpty(z.Key)).GroupBy(z => $"{z.Key}_{z.Tile.Bmp.Width}_{z.Tile.Bmp.Height}").ToArray();

            foreach (var item in grp)
            {
                var fr = item.First();
                var hashKey = $"{fr.Key}_{fr.Tile.Bmp.Width}_{fr.Tile.Bmp.Height}";
                tilesHash.Add(hashKey, fr.Tile);

                foreach (var sitem in item.Skip(1))
                {
                    sitem.Tile = fr.Tile;
                }
            }

            var grp2 = pages.SelectMany(z => z.Infos)
               .Where(z => !string.IsNullOrEmpty(z.Key)).GroupBy(z => $"{z.Key}_{z.Tile.Bmp.Width}_{z.Tile.Bmp.Height}").ToArray();

            foreach (var item in grp2)
            {
                var fr = item.First();
                var hashKey = $"{fr.Key}_{fr.Tile.Bmp.Width}_{fr.Tile.Bmp.Height}";

                foreach (var sitem in item)
                {
                    sitem.Tile = tilesHash[hashKey];
                }
            }

            var bin = TilesViewer.ExportToBinary(pages);
            using MemoryStream ms = new MemoryStream(bin);
            TiledCBook book = new TiledCBook(ms);
            var _page = book.GetPage(0);
            return _page;
        }

        public static void AppendTOC(TOC toc, Stream fs)
        {
            if (toc == null || toc.Items.Count <= 0)
                return;

            fs.Write(BitConverter.GetBytes((int)toc.Items.Count));
            var offset = fs.Position;
            fs.Write(BitConverter.GetBytes(0));//total section len in bytes
            foreach (var item in toc.Items)
            {
                var h1 = Encoding.UTF8.GetBytes(item.Header).ToList();
                var h2 = Encoding.UTF8.GetBytes(item.Text).ToList();
                //while (h1.Count % 4 != 0)
                //     h1.Add(0);
                //     
                fs.Write(BitConverter.GetBytes(item.Page));
                fs.Write(BitConverter.GetBytes((ushort)item.Ident));
                fs.Write(BitConverter.GetBytes((ushort)h1.Count));
                fs.Write(h1.ToArray());
                fs.Write(BitConverter.GetBytes((ushort)h2.Count));
                fs.Write(h2.ToArray());
            }
            var totalLen = fs.Position - offset + 4;
            var offset2 = fs.Position;
            fs.Seek(offset, SeekOrigin.Begin);
            fs.Write(BitConverter.GetBytes((int)totalLen));
            fs.Seek(offset2, SeekOrigin.Begin);
        }

        public static void AppendTOCAsSection(TOC toc, Stream fs)
        {
            if (toc == null || toc.Items.Count <= 0)
                return;

            MemoryStream ms = new MemoryStream();
            ms.Write(BitConverter.GetBytes((int)toc.Items.Count));
            var offset = fs.Position;
            ms.Write(BitConverter.GetBytes(0));//total section len in bytes
            foreach (var item in toc.Items)
            {
                var h1 = Encoding.UTF8.GetBytes(item.Header).ToList();
                var h2 = Encoding.UTF8.GetBytes(item.Text).ToList();
                //while (h1.Count % 4 != 0)
                //     h1.Add(0);
                //     
                ms.Write(BitConverter.GetBytes(item.Page));
                ms.Write(BitConverter.GetBytes((ushort)item.Ident));
                ms.Write(BitConverter.GetBytes((ushort)h1.Count));
                ms.Write(h1.ToArray());
                ms.Write(BitConverter.GetBytes((ushort)h2.Count));
                ms.Write(h2.ToArray());
            }
            var totalLen = ms.Position - offset + 4;
            var offset2 = ms.Position;
            ms.Seek(offset, SeekOrigin.Begin);
            ms.Write(BitConverter.GetBytes((int)totalLen));
            ms.Seek(offset2, SeekOrigin.Begin);

            //section header
            fs.Write(BitConverter.GetBytes((int)(5 + ms.Length)));
            fs.WriteByte(0xA0);
            fs.Write(ms.ToArray());
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
