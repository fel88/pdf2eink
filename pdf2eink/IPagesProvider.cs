namespace pdf2eink
{
    public interface IPagesProvider : IDisposable
    {
        string SourcePath { get; set; }
        int Pages { get; }
        Bitmap GetPage(int index);
        int Dpi { get; set; }
    }

}