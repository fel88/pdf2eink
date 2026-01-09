namespace pdf2eink
{
    public interface IPagesProviderWithLetters : IPagesProvider
    {
        PageBoundedObject[] GetBoundedObjects(int index);
        SizeF GetPageSize(int index);
        string GetPageText(int index);

    }
}