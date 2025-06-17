namespace pdf2eink
{
    public class TiledCBook : ICBook
    {
        public TiledCBook(string path)
        {
            Name = Path.GetFileName(path);
            var bts = File.ReadAllBytes(path);
            CreateFromStream(new MemoryStream(bts));
        }

        public TiledCBook(Stream ms)
        {
            CreateFromStream(ms);
        }

        private void CreateFromStream(Stream ms)
        {

            var tilesQty = ms.ReadInt();

            List<Tile> tiles = new List<Tile>();
            for (int i = 0; i < tilesQty; i++)
            {
                Tile tile = new Tile(ms);
                tiles.Add(tile);
            }

            var pagesQty = ms.ReadInt();

            for (int k = 0; k < pagesQty; k++)
            {
                var pageWidth = ms.ReadInt();
                var pageHeigth = ms.ReadInt();
                TiledPageInfo page = new TiledPageInfo() { Width = pageWidth, Heigth = pageHeigth };
                Pages.Add(page);
                var tileInfosQty = ms.ReadInt();
                List<TileInfo> tileInfos = new List<TileInfo>();
                for (int i = 0; i < tileInfosQty; i++)
                {
                    TileInfo t = new TileInfo(page);
                    t.X = ms.ReadInt();
                    t.Y = ms.ReadInt();
                    var tileIdx = ms.ReadInt();
                    t.Tile = tiles[tileIdx];
                    tileInfos.Add(t);
                }
                page.Infos = tileInfos.ToArray();
            }
        }
        List<TiledPageInfo> Pages = new List<TiledPageInfo>();
        public int pages => Pages.Count;

        public TOC Toc { get; set; }

        public bool HasTOC => false;

        public int Width { get => Pages[0].Width; set { } }

        public int Height { get => Pages[0].Heigth; set { } }

        public string Name { get; set; }

        public Bitmap GetPage(int pageNo)
        {

            var page = Pages[pageNo];
            Bitmap bmp = new Bitmap(page.Width, page.Heigth);
            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            foreach (var item in page.Infos)
            {
                gr.DrawImage(item.Tile.Bmp, item.X, item.Y);
            }
            return bmp;
        }
    }

}
