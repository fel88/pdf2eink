using System.Collections;

namespace pdf2eink
{
    public class DiffTile : ITile
    {
        public DiffTile(IReadOnlyCollection<ITile> list, BitStream stream)
        {
            var offset = stream.ReadUIn16(8);
            Parent = list.Skip(list.Count - offset).First();
            var clone = Parent.Clone();
            Bmp = clone.Bmp;

            var qty = stream.ReadUIn16(4);
            List<(int, int)> swaps = new List<(int, int)>();
            for (int j = 0; j < qty; j++)
            {
                var x = stream.ReadUIn16(4);
                var y = stream.ReadUIn16(4);
                swaps.Add((x, y));
            }

            Points = swaps.ToArray();
            foreach (var item in swaps)
            {
                var px = Bmp.GetPixel(item.Item1, item.Item2);
                Bmp.SetPixel( item.Item1, item.Item2, px.R == 0 ? Color.White : Color.Black);
            }

            //Points = tpoints.ToArray();

            //MakeBmp(tileW, tileH);
            stream.Align8();
        }
        public string Name { get; set; }
        public Func<ITile, int> GetIndexOf;
        public Bitmap Bmp { get; private set; }
        public ITile Parent;
        internal void WriteTo(MemoryStream ms)
        {
            var ind1 = GetIndexOf(Parent);
            var ind2 = GetIndexOf(this);
            if (ind2 < ind1)
                throw new Exception();

            var offset = Math.Abs(ind2 - ind1);
            if (offset > byte.MaxValue)
                throw new Exception();

            var bits1 = new BitArray(new byte[] { (byte)offset });
            if (Bmp.Width != Parent.Bmp.Width ||
                Bmp.Height != Parent.Bmp.Height
                )
                throw new Exception();

            if (Bmp.Width > 16 || Bmp.Height > 16)
                throw new Exception();

            List<byte> bits =
            [
                1, //diff tile type
            ];

            for (int i = 0; i < bits1.Length; i++)
                bits.Add((byte)(bits1[i] ? 1 : 0));
            int diffs = 0;
            for (int i = 0; i < Bmp.Width; i++)
            {
                for (int j = 0; j < Bmp.Height; j++)
                {
                    var px = Bmp.GetPixel(i, j);
                    var px2 = Parent.Bmp.GetPixel(i, j);
                    if (px.R != px2.R)
                    {
                        diffs++;                       

                    }
                }
            }
            if (diffs > 16 )
                throw new Exception();

            var bits4 = new BitArray(new byte[] { (byte)diffs });
            

            for (int k = 0; k < 4; k++)
                bits.Add((byte)(bits4[k] ? 1 : 0));

            for (int i = 0; i < Bmp.Width; i++)
            {
                for (int j = 0; j < Bmp.Height; j++)
                {
                    var px = Bmp.GetPixel(i, j);
                    var px2 = Parent.Bmp.GetPixel(i, j);
                    if (px.R != px2.R)
                    {
                        //X,Y of swap point
                        var bits2 = new BitArray(new byte[] { (byte)i });
                        var bits3 = new BitArray(new byte[] { (byte)j });

                        for (int k = 0; k < 4; k++)
                            bits.Add((byte)(bits2[k] ? 1 : 0));

                        for (int k = 0; k < 4; k++)
                            bits.Add((byte)(bits3[k] ? 1 : 0));

                    }
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

        (int, int)[] Points;

        internal Tile ToFullTile()
        {
            ITile ret = Parent.Clone();
            foreach (var item in Points)
            {
                var px = ret.Bmp.GetPixel(item.Item1, item.Item2);
                ret.Bmp.SetPixel(item.Item1, item.Item2, px.R == 0 ? Color.White : Color.Black);
            }
            return ret as Tile;
        }

        public ITile Clone()
        {
            throw new NotImplementedException();
        }
    }
}