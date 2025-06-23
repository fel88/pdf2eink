namespace pdf2eink
{
    public class TileStatisticInfo
    {
        public Tile Tile;
        public int Qty;

        internal int DistTo(TileStatisticInfo deq)
        {
            int diff = 0;
            for (int i = 0; i < Tile.ImageHash.Length; i++)
            {
                if (Tile.ImageHash[i] != deq.Tile.ImageHash[i])
                    diff++;
            }
            return diff;
        }
    }
}
