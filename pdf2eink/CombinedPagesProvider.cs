namespace pdf2eink
{
    public class CombinedPagesProvider : IPagesProvider
    {
        IPagesProvider[] Childs;
        public CombinedPagesProvider(IPagesProvider[] childs)
        {
            Childs = childs;
        }
        public string SourcePath { get => Childs[0].SourcePath; set => throw new NotImplementedException(); }

        public int Pages => Childs.Sum(z => z.Pages);

        public int Dpi
        {
            get => Childs[0].Dpi; set
            {

                foreach (var item in Childs)
                {
                    item.Dpi = value;
                }
            }
        }


        public void Dispose()
        {
            foreach (var item in Childs)
            {
                item.Dispose();
            }
        }

        public Bitmap GetPage(int index)
        {
            int sum = 0;
            foreach (var item in Childs)
            {
                sum += item.Pages;
                if (index < sum)
                {
                    return item.GetPage(index - sum + item.Pages);

                }
            }
            throw new NotImplementedException();
        }

        
    }
}