namespace pdf2eink
{
    public interface ITile
    {
        string Name { get; }
        Bitmap Bmp { get; }

        ITile Clone();
    }
}