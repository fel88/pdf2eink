namespace pdf2eink
{
    public class WordInfo : PageBoundedObject
    {
        public string Word;

        public string Font;
        public FontInfo FontInfo;
        public List<LetterInfo> Letters = new List<LetterInfo>();

        public WordInfo Clone()
        {
            WordInfo ret = new WordInfo();
            ret.Word = Word;
            ret.Bound = Bound;
            ret.Font = Font;
            return ret;
        }

        public override string ToString()
        {
            return $"{Word} {Font}";
        }
    }
}