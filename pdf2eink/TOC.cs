namespace pdf2eink
{
    public class TOC
    {
        public List<TOCItem> Items = new List<TOCItem>();

        internal void Parse(string str)
        {
            StringReader rdr = new StringReader(str);
            string t;
            while ((t = rdr.ReadLine()) != null)
            {
                var spl = t.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                if (spl.Length == 0 || !spl.Last().All(char.IsDigit))
                    continue;

                Items.Add(new TOCItem() { Header = string.Join(' ', spl.Take(spl.Length - 1).ToArray()), Page = int.Parse(spl.Last()), Ident = 0 });
            }
        }
    }
}
