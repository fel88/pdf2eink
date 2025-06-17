using System.Collections;
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

        public Tile(Stream stream)
        {
            byte[] temp = new byte[3];
            stream.Read(temp, 0, temp.Length);
            BitArray bb = new BitArray(temp);
            ushort tileW = 0;
            ushort tileH = 0;

            for (int j = 0; j < 10; j++)
            {
                tileW |= (ushort)((bb[j] ? 1 : 0) << j);
            }
            for (int j = 0; j < 10; j++)
            {
                tileH |= (ushort)((bb[j + 10] ? 1 : 0) << j);
            }

            List<TilePoint> tpoints = new List<TilePoint>();
            var sz = tileW * tileH - 4;
            while (sz % 8 != 0)
                sz++;

            var bytes = sz / 8;

            byte[] buffer = new byte[bytes];
            stream.Read(buffer, 0, bytes);
            List<byte> bits = new List<byte>();
            bits.Insert(0, (byte)(bb[23] ? 1 : 0));
            bits.Insert(0, (byte)(bb[22] ? 1 : 0));
            bits.Insert(0, (byte)(bb[21] ? 1 : 0));
            bits.Insert(0, (byte)(bb[20] ? 1 : 0));

            foreach (var item in buffer)
            {
                for (int i = 0; i < 8; i++)
                {
                    bits.Add((byte)((item & (1 << i)) != 0 ? 1 : 0));
                }
            }

            var index = 0;
            for (int i = 0; i < tileW; i++)
            {
                for (int j = 0; j < tileH; j++)
                {
                    if (bits[index++] == 0)
                        tpoints.Add(new TilePoint() { X = i, Y = j });
                }
            }

            Points = tpoints.ToArray();

            MakeBmp(tileW, tileH);
        }

        public string Name { get; set; }

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
            gr.Clear(Color.White);
            foreach (var zitem in Points)
            {
                Bmp.SetPixel(zitem.X, zitem.Y, Color.Black);
            }
            CalcImageHash();
        }
        private void MakeBmp(int w, int h)
        {
            if (Bmp != null)
                return;


            Bmp = new Bitmap(w, h);
            var gr = Graphics.FromImage(Bmp);
            gr.Clear(Color.White);
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