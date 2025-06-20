using OpenCvSharp.Extensions;
using PdfiumViewer;
using System.Drawing;
using System.Drawing.Printing;
using System.Security.Cryptography.Pkcs;
using UglyToad.PdfPig.Content;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace pdf2eink
{
    public class PdfPagesProvider : IPagesProviderWithLetters, IDisposable
    {
        public PdfPagesProvider(string fileName)
        {
            pdoc = PdfDocument.Load(fileName);
            SourcePath = fileName;
        }
        public int Dpi { get; set; } = 300;

        PdfDocument pdoc;
        public int Pages => pdoc.PageCount;

        public string SourcePath { get; private set; }

        public Bitmap GetPage(int index)
        {
            return (Bitmap)pdoc.Render(index, Dpi, Dpi, PdfRenderFlags.CorrectFromDpi);
        }

        public void Dispose()
        {
            if (pdoc != null)
                pdoc.Dispose();
        }

        //public string GetPageText(int index)
        //{
        //    var all =pdoc.GetPdfText(index); ;
        //    var span = new PdfTextSpan(index, 0, 10);
        //    var bounds= pdoc.GetTextBounds(span );
        //    var t1 =pdoc.GetPdfText(span);
        //    var span2 = new PdfTextSpan(index, 1, 2);
        //    var bounds2 = pdoc.GetTextBounds(span2);
        //    var t12 = pdoc.GetPdfText(span2);

        //    var size = pdoc.PageSizes[index];

        //    var pg = GetPage(index);
        //    var koefx = size.Width / pg.Width;
        //    var koefy = size.Height / pg.Height;
        //    var gr = Graphics.FromImage(pg);
        //    var zz = pdoc.PointToPdf(0, new Point(100, 100));

        //    foreach (var item in bounds2)
        //    {
        //        var zoom = 4.0f;
        //        var convert = pdoc.RectangleFromPdf(0, item.Bounds);
        //        var bnds= new RectangleF(
        //            convert.Left/koefx,
        //            (convert.Top) / koefy,
        //            convert.Width /koefx, convert.Height /koefy);

        //        gr.DrawRectangle(Pens.Red, bnds);
        //    }
        //    /*using (var pdfDoc = PdfReader.Open(SourcePath))
        //    {
        //        var content = ContentReader.ReadContent(pdfDoc.Pages[0]);
        //        var text = ExtractText(content).ToArray();
        //        var txt = string.Join(' ', text);
        //        //return text;
        //        XGraphics xgfx = XGraphics.FromPdfPage(pdfDoc.Pages[0]);

        //    }*/
        //        pg.Save("temp1.png");

        //    return pdoc.GetPdfText(index);

        //}

        public LetterInfo[] GetPageLetters(int index)
        {
            List<TiledPageInfo> pages = new List<TiledPageInfo>();
            List<Word> words = new List<Word>();
            UglyToad.PdfPig.Content.Page ppage = null;
            using (var document = UglyToad.PdfPig.PdfDocument.Open(SourcePath))
            {
                foreach (var page in document.GetPages().Skip(index).Take(1))
                {
                    string pageText = page.Text;
                    ppage = page;
                    words = page.GetWords().ToList();
                }
            }
            var bmp = GetPage(index);
            var kx = bmp.Width / (float)ppage.Width;
            var ky = bmp.Height / (float)ppage.Height;
            List<LetterInfo> ret = new List<LetterInfo>();
            foreach (var item in words)
            {
                foreach (var litem in item.Letters)
                {
                    var g = litem.GlyphRectangle;

                    var rect = new RectangleF(kx * (float)g.Left,
                        bmp.Height - ky * (float)g.Top, kx * (float)g.Width,
                         ky * (float)g.Height);
                    ret.Add(new LetterInfo()
                    {
                        Bound = rect,
                        Letter = litem.Value,
                        Font = $"{litem.FontName}_{litem.FontSize}_{litem.Font.Weight}_{litem.Font.IsItalic}_{litem.Font.IsBold}"
                    });
                }

                
            }
            return ret.ToArray();

        }
    }
}