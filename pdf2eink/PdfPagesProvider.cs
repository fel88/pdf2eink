using PdfiumViewer;

namespace pdf2eink
{
    public class PdfPagesProvider : IPagesProvider, IDisposable
    {
        public PdfPagesProvider(string fileName)
        {
            pdoc = PdfDocument.Load(fileName);
        }
        public int Dpi { get; set; } = 300;

        PdfDocument pdoc;
        public int Pages => pdoc.PageCount;

        public string SourcePath { get; set; }

        public Bitmap GetPage(int index)
        {
            return (Bitmap)pdoc.Render(index, Dpi, Dpi, PdfRenderFlags.CorrectFromDpi);
        }

        public void Dispose()
        {
            if (pdoc != null)
                pdoc.Dispose();
        }
    }    
}