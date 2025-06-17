namespace pdf2eink
{
    public interface ICBook
    {
        int pages { get; }
        Bitmap GetPage(int pageNo);

        TOC Toc { get; }
        int Width { get; }
        int Height { get; }
        bool HasTOC { get; }
        string Name { get; set; }
    }
}