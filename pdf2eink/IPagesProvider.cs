using UglyToad.PdfPig.Content;

namespace pdf2eink
{
    public interface IPagesProvider : IDisposable
    {
        string SourcePath { get; }
        int Pages { get; }
        Bitmap GetPage(int index);
        int Dpi { get; set; }
    }

}