

namespace pdf2eink
{
    public class BitStream
    {
        Stream stream;
        public BitStream(Stream stream)
        {
            this.stream = stream;
        }

        Queue<byte> qbits = new Queue<byte>();
        public ushort ReadUIn16(int bits)
        {
            while (qbits.Count < bits)
            {
                var v = stream.ReadByte();
                if (v < 0)
                    throw new EndOfStreamException();

                var bb = (byte)v;
                for (int i = 0; i < 8; i++)
                {
                    if ((bb & (1 << i)) > 0)
                        qbits.Enqueue(1);
                    else
                        qbits.Enqueue(0);
                }
            }
            ushort ret = 0;
            for (int i = 0; i < bits; i++)
            {
                var deq = qbits.Dequeue();
                if (deq > 0)
                    ret |= (ushort)(1 << i);
            }

            return ret;
        }

        internal byte[] ReadBytes(int qty)
        {
            while (qbits.Count < (qty * 8))
            {
                var v = stream.ReadByte();
                if (v < 0)
                    throw new EndOfStreamException();

                var bb = (byte)v;
                for (int i = 0; i < 8; i++)
                {
                    if ((bb & (1 << i)) > 0)
                        qbits.Enqueue(1);
                    else
                        qbits.Enqueue(0);
                }
            }

            List<byte> ret = new List<byte>();
            for (int i = 0; i < qty; i++)
            {

                byte bret = 0;
                for (int k = 0; k < 8; k++)
                {
                    var deq = qbits.Dequeue();
                    if (deq > 0)
                        bret |= (byte)(1 << k);
                }
                ret.Add(bret);
            }

            return ret.ToArray();
        }


        public void Align8()
        {
            qbits.Clear();
        }

        public bool[] ReadBits(int sz)
        {
            while (qbits.Count < sz)
            {
                var v = stream.ReadByte();
                if (v < 0)
                    throw new EndOfStreamException();

                var bb = (byte)v;
                for (int i = 0; i < 8; i++)
                {
                    if ((bb & (1 << i)) > 0)
                        qbits.Enqueue(1);
                    else
                        qbits.Enqueue(0);
                }
            }

            List<bool> ret = new List<bool>();
            for (int i = 0; i < sz; i++)
            {
                ret.Add(qbits.Dequeue() == 0);
            }
            return ret.ToArray();
        }
    }
}