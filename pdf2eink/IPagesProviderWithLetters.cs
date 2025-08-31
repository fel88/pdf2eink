namespace pdf2eink
{
    public interface IPagesProviderWithLetters : IPagesProvider
    {
        LetterInfo[] GetPageLetters(int index);
        SizeF GetPageSize(int index);
        string GetPageText(int index);

    }
}