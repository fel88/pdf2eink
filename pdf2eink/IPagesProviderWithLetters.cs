namespace pdf2eink
{
    public interface IPagesProviderWithLetters : IPagesProvider
    {
        LetterInfo[] GetPageLetters(int index);

    }

}