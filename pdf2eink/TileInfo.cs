namespace pdf2eink
{
    public class TileInfo
    {
        public TileInfo(TiledPageInfo page)
        {
            Page = page;
        }

        public Tile Tile;
        public int X;
        public int Y;
        public TiledPageInfo Page;
        public string Key;
    }
}