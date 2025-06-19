using System.Collections;
using System.IO.MemoryMappedFiles;

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
            var pagesSectionOffset = ms.ReadInt();
            var tilesQty = ms.ReadInt();

            List<Tile> tiles = new List<Tile>();
            var bs = new BitStream(ms);
            for (int i = 0; i < tilesQty; i++)
            {
                var type = bs.ReadUIn16(1);
                if (type == 0)
                {
                    Tile tile = new Tile(bs);
                    tiles.Add(tile);
                }
                else if (type == 1)
                {
                    DiffTile tile = new DiffTile(tiles, bs);
                    tiles.Add(tile.ToFullTile());
                }
            }

            ms.Seek(pagesSectionOffset * 1024, SeekOrigin.Begin);
            var pagesQty = ms.ReadInt();
            var bitsOfTileIdx = (int)Math.Ceiling(Math.Log2(tiles.Count));
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
                    int bitsToRead = 20 + bitsOfTileIdx;
                    while (bitsToRead % 8 != 0)
                        bitsToRead++;

                    byte[] read = new byte[bitsToRead / 8];
                    ms.Read(read, 0, read.Length);
                    BitArray ba = new BitArray(read);
                    t.X = 0;
                    t.Y = 0;
                    for (int j = 0; j < 10; j++)
                    {
                        t.X |= (ushort)((ba[j] ? 1 : 0) << j);
                    }
                    for (int j = 0; j < 10; j++)
                    {
                        t.Y |= (ushort)((ba[j + 10] ? 1 : 0) << j);
                    }
                    int tileIdx = 0;
                    for (int j = 0; j < bitsOfTileIdx; j++)
                    {
                        tileIdx |= (ushort)((ba[j + 20] ? 1 : 0) << j);
                    }
                                        
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
