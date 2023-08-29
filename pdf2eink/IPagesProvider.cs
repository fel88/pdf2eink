namespace pdf2eink
{
    public interface IPagesProvider : IDisposable
    {
        int Pages { get; }
        Bitmap GetPage(int index);
        int Dpi { get; set; }
    }

}