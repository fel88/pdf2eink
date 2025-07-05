namespace pdf2eink
{
    public class HuffNode
    {
        static int counter;
        public HuffNode()
        {
            counter++;
        }
        public int Prob;
        public byte? Byte;
        public HuffNode Left;
        public HuffNode Right;
        public HuffNode Parent;
    }
}