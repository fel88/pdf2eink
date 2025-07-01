namespace pdf2eink
{
    public interface IPagesProviderWithImages : IPagesProvider
    {
        PageImageInfo[] GetPageImages(int index);

    }
}