using System.Text;

namespace pdf2eink
{
    public class Tile
    {
        public Tile(TilePoint[] points)
        {
            Points = points;
            MakeBmp();
        }

        public Bitmap Bmp;
        public TilePoint[] Points;
        private void MakeBmp()
        {
            if (Bmp != null)
                return;

            int width = Points.Max(z => z.X) + 1;
            int height = Points.Max(z => z.Y) + 1;
            Bmp = new Bitmap(width, height);
            var gr = Graphics.FromImage(Bmp);
            foreach (var zitem in Points)
            {
                Bmp.SetPixel(zitem.X, zitem.Y, Color.Black);
            }
            CalcImageHash();
        }
        public string ImageHash { get; private set; }
        public void CalcImageHash()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < Bmp.Width; i++)
            {
                for (int j = 0; j < Bmp.Height; j++)
                {
                    if (Bmp.GetPixel(i, j).R == 0)
                    {
                        sb.Append('0');
                    }
                    else sb.Append('1');
                }
            }
            ImageHash = sb.ToString();

        }
    }
}