using System.Linq;
using System.Text;

namespace pdf2eink
{
    public class ZCBProcessor
    {
        public byte[] Compress(byte[] bts)
        {
            build(bts);
            var output = encode(bts);

            var preamble = "ZCB";
            List<byte> vtable = new List<byte>();
            for (int i = 0; i < 256; i++)
            {
                var b = (byte)i;
                vtable.Add(b);
                if (!hnodes.ContainsKey(b))
                {
                    vtable.Add((byte)0);
                }
                else
                {
                    var r = hnodes[b];
                    var ee = getBits(b);
                    vtable.Add((byte)ee.Length);
                    vtable.AddRange(ee);
                }
            }
            var totalBytes = Encoding.UTF8.GetBytes(preamble).Concat(BitConverter.GetBytes(bts.LongLength)).Concat(vtable).Concat(output).ToArray();

            return totalBytes;
        }
        private byte[] decode(byte[] bts)
        {
            List<byte> output = new List<byte>();
            HuffNode p = top;
            foreach (var item in bts)
            {
                List<byte> bits = new List<byte>();
                for (int i = 0; i < 8; i++)
                {
                    if ((item & (1 << i)) != 0) bits.Add(1);
                    else bits.Add(0);
                }
                foreach (var bit in bits)
                {
                    if (p.Byte != null)
                    {
                        output.Add(p.Byte.Value);
                        p = top;
                    }
                    if (bit == 1)
                    {
                        p = p.Right;
                    }
                    else
                    {
                        p = p.Left;
                    }
                }
            }
            if (p.Byte != null)
            {
                output.Add(p.Byte.Value);
            }
            return output.ToArray();
        }
        public byte[] Decompress(byte[] bts)
        {
            Dictionary<byte, byte[]> dic = new Dictionary<byte, byte[]>();
            int i = 0;
            for (i = 8 + 3; i < bts.Length;)
            {
                var val = bts[i];
                var len = bts[i + 1];
                byte[] dd = new byte[len];
                for (int j = 0; j < len; j++)
                {
                    dd[j] = bts[2 + i + j];
                }
                dic.Add(val, dd);
                i += 2;
                i += len;
                if (val == 255)
                    break;
            }
            //build tree
            ulong flen = BitConverter.ToUInt64(bts, 3);
            top = new HuffNode();
            var p = top;
            foreach (var item in dic)
            {
                if (item.Value.Length == 0)
                    continue; 

                foreach (var b in item.Value)
                {
                    if (b == 1)
                    {
                        if (p.Right == null)
                        {
                            p.Right = new HuffNode();
                            p.Right.Parent = p;
                        }
                        p = p.Right;
                    }
                    else
                    {
                        if (p.Left == null)
                        {
                            p.Left = new HuffNode();
                            p.Left.Parent = p;
                        }
                        p = p.Left;
                    }
                }
                p.Byte = item.Key;
                p = top;
            }
            //reverse
            return decode(bts.Skip(i).ToArray());
        }
        byte[] getBits(byte b)
        {

            /*if (b in bits_cache) {
                return bits_cache[b];
            }
            */
            var lbits = new List<byte>();
            var node = hnodes[b];
            // console.log('node', node);
            while (node != null && node.Parent != null)
            {
                if (node.Parent.Left == node)
                {
                    lbits.Add(0);
                }
                else
                {
                    lbits.Add(1);
                }
                node = node.Parent;
            }
            lbits.Reverse();

            //bits_cache[b] = lbits;
            return lbits.ToArray();
        }
        private byte[] encode(byte[] bts)
        {
            List<byte> bits = new List<byte>();
            List<byte> output = new List<byte>();

            foreach (var b in bts)
            {
                List<byte> lbits = new List<byte>();
                var node = hnodes[b];
                while (node != null && node.Parent != null)
                {
                    if (node.Parent.Left == node)
                    {
                        lbits.Add(0);
                    }
                    else
                    {
                        lbits.Add(1);
                    }
                    node = node.Parent;
                }
                lbits.Reverse();
                bits.AddRange(lbits);
                while (bits.Count >= 8)
                {
                    byte bb = 0;
                    for (int j = 0; j < 8; j++)
                    {
                        bb |= (byte)((bits[j]) << j);
                    }
                    output.Add(bb);
                    bits.RemoveRange(0, 8);
                }
            }
            while (bits.Count % 8 != 0)
            {
                bits.Add(0);
            }
            for (int i = 0; i < bits.Count; i += 8)
            {
                byte b = 0;
                for (int j = 0; j < 8; j++)
                {
                    b |= (byte)((bits[i + j]) << j);
                }
                output.Add(b);
            }

            return output.ToArray();
        }

        void build(byte[] b)
        {
            Dictionary<byte, int> dd = new Dictionary<byte, int>();
            for (int i = 0; i < b.Count(); i++)
            {
                if (!dd.ContainsKey(b[i]))
                {
                    dd.Add(b[i], 0);
                }
                dd[b[i]]++;
            }
            foreach (var d in dd.Keys.OrderByDescending(z => dd[z]))
            {
                
            }
            BuildHuffman(dd);

        }
        HuffNode top = null;
        Dictionary<byte, HuffNode> hnodes;
        private void BuildHuffman(Dictionary<byte, int> dd)
        {
            hnodes = new Dictionary<byte, HuffNode>();
            List<HuffNode> nodes = new List<HuffNode>();
            foreach (var key in dd.Keys)
            {
                nodes.Add(new HuffNode() { Byte = key, Prob = dd[key] });
                hnodes.Add(key, nodes.Last());
            }



            while (nodes.Count > 1)
            {
                var min2 = nodes.OrderBy(z => z.Prob).Take(2).ToArray();
                var p = new HuffNode() { Left = min2[0], Right = min2[1], Prob = min2.Sum(z => z.Prob) };
                min2[0].Parent = p;
                min2[1].Parent = p;
                nodes.Add(p);
                nodes.Remove(min2[0]);
                nodes.Remove(min2[1]);
            }
            top = nodes[0];
        }

    }
}