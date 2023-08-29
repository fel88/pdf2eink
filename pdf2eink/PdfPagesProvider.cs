﻿using PdfiumViewer;

namespace pdf2eink
{
    public class PdfPagesProvider : IPagesProvider, IDisposable
    {
        public PdfPagesProvider(string fileName)
        {
            pdoc = PdfDocument.Load(fileName);
        }

        PdfDocument pdoc;
        public int Pages => pdoc.PageCount;

        public Bitmap GetPage(int index)
        {
            return (Bitmap)pdoc.Render(index, 300, 300, PdfRenderFlags.CorrectFromDpi);
        }

        public void Dispose()
        {
            if (pdoc != null)
                pdoc.Dispose();
        }
    }

}