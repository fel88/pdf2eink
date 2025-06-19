using System.Collections;
using System.Text;

namespace pdf2eink
{
    public class Tile : ITile
    {
        public Tile(TilePoint[] points)
        {
            Points = points;
            MakeBmp();
        }

        public Tile(BitStream stream)
        {            
            
            ushort tileW = stream.ReadUIn16(10); 
            ushort tileH = stream.ReadUIn16(10); 
                    

            List<TilePoint> tpoints = new List<TilePoint>();
            var sz = tileW * tileH ;
            bool[] bits = stream.ReadBits(sz);

            var index = 0;
            for (int i = 0; i < tileW; i++)
            {
                for (int j = 0; j < tileH; j++)
                {
                    if (bits[index++] )
                        tpoints.Add(new TilePoint() { X = i, Y = j });
                }
            }

            Points = tpoints.ToArray();

            MakeBmp(tileW, tileH);
            stream.Align8();
        }

        public string Name { get; set; }

        public Bitmap Bmp { get; private set; }

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

        internal void WriteTo(MemoryStream ms)
        {
            var bits1 = new BitArray(BitConverter.GetBytes((ushort)Bmp.Width));
            var bits2 = new BitArray(BitConverter.GetBytes((ushort)Bmp.Height));

            List<byte> bits = new List<byte>();
            bits.Add(0);//full tile type
            for (int i = 0; i < 10; i++)
            {
                bits.Add((byte)(bits1[i] ? 1 : 0));
            }
            for (int i = 0; i < 10; i++)
            {
                bits.Add((byte)(bits2[i] ? 1 : 0));
            }

            for (int i = 0; i < Bmp.Width; i++)
            {
                for (int j = 0; j < Bmp.Height; j++)
                {
                    var px = Bmp.GetPixel(i, j);
                    bits.Add((byte)(px.R == 0 ? 0 : 1));
                }
            }

            while (bits.Count % 8 != 0)
                bits.Add(0);

            for (int i = 0; i < bits.Count; i += 8)
            {
                byte b = 0;
                for (int j = 0; j < 8; j++)
                {
                    b ^= (byte)(bits[i + j] << j);
                }

                ms.WriteByte(b);
            }
        }

        public ITile Clone()
        {
            Tile ret = new Tile(Points.ToArray());
            ret.Name = Name + "_cloned";
            return ret;
        }
    }
}