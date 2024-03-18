using OpenCvSharp;
using System.Text;

namespace pdf2eink
{
    public class CbBook
    {
        public CbBook()
        {

        }
        public CbBook(string path)
        {
            Open(path);
        }
        int tocRawSize = 0;
        byte[] bts;
        byte[] header;
        byte[] body;
        public void ParseTOC()
        {
            //toolStripDropDownButton1.Enabled = true;
            //parse toc here
            Toc = new TOC();
            var tocItems = BitConverter.ToUInt32(bts, 12);
            tocRawSize = BitConverter.ToInt32(bts, 16);
            int accum = 20;
            for (int i = 0; i < tocItems; i++)
            {
                var page = BitConverter.ToInt32(bts, (int)accum);
                accum += 4;
                var ident = BitConverter.ToUInt16(bts, (int)accum);
                accum += 2;
                var len = BitConverter.ToUInt16(bts, (int)accum);
                accum += 2;
                var str = Encoding.UTF8.GetString(bts, accum, len);
                accum += len;
                Toc.Items.Add(new TOCItem() { Header = str, Page = page, Ident = ident });
            }
        }

        public void DeletePage(int pageNo)
        {
            var size = 76 * 448;

            List<byte> part0 = new List<byte>();
            part0.AddRange(Encoding.UTF8.GetBytes("CB"));
            part0.AddRange(BitConverter.GetBytes((byte)0));//format . 0 -simple without meta info
            part0.AddRange(BitConverter.GetBytes(pages - 1));

            var part1 = bts.Skip(8).Take(4 + pageNo * size).ToArray();
            var part2 = bts.Skip(12 + tocRawSize + pageNo * size).Skip(size).ToArray();

            bts = part0.Concat(part1).Concat(part2).ToArray();
            pages = BitConverter.ToInt32(bts, 4);
            //trackBar1.Maximum = pages - 1;
            //showPage();
        }
        public Bitmap GetPage(int pageNo)
        {
            var width = BitConverter.ToUInt16(bts, 8);
            var height = BitConverter.ToUInt16(bts, 10);
            int stride = 4 * (int)Math.Ceiling(width / 8 / 4f);//aligned 4
            var size = stride * height;
            var page1 = bts.Skip(12 + tocRawSize).Skip(pageNo * size).Take(size).ToArray();
            Bitmap bmp = new Bitmap(width, height);

            for (int j = 0; j < bmp.Height; j++)
            {

                var line = page1.Skip(j * stride).Take(stride).ToArray();
                int counter = 0;
                for (int i = 0; i < bmp.Width; i++)
                {
                    int byteNo = counter / 8;
                    int bitNo = counter % 8;
                    counter++;
                    var b = (byte)(line[byteNo] & (1 << (8 - 1 - bitNo))) > 0;
                    if (b)
                    {
                        bmp.SetPixel(i, j, Color.White);
                    }
                    else { bmp.SetPixel(i, j, Color.Black); }
                }
            }
            return bmp;
        }

        public byte Format => bts[3];
        public void AppendTOC(TOC toc)
        {
            header[3] = 1;
            Toc = toc;
           
        }
        internal void Open(string path)
        {
            bts = File.ReadAllBytes(path);
            header = bts.Take(12).ToArray();
            body = bts.Skip(12).ToArray();
            var format = bts[3];
            if (format == 1)
            {
                ParseTOC();
                body = body.Skip(tocRawSize).ToArray();
            }
            pages = BitConverter.ToInt32(bts, 4);
        }

        internal void SaveAs(string fileName)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(header, 0, header.Length);
            
            BookExporter.AppendTOC(Toc, ms);
            ms.Write(body, 0, body.Length);

            File.WriteAllBytes(fileName, ms.ToArray());            
        }

        public int pages;

        public TOC Toc { get; private set; } 

        public bool HasTOC => Toc != null && Toc.Items.Any();
    }
}