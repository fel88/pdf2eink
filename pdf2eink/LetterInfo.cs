namespace pdf2eink
{
    public class LetterInfo
    {
        public string Letter;
        public RectangleF Bound;
        public PointF Location;
        public string Font;
        public FontInfo FontInfo;
      
        public LetterInfo Clone()
        {
            LetterInfo ret = new LetterInfo();
            ret.Letter = Letter;
            ret.Bound = Bound;
            ret.Font = Font;
            return ret;
        }

        public override string ToString()
        {
            return $"{Letter} {Font}";
        }
    }
}