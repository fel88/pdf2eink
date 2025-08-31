using OpenCvSharp;
using System.Text;

namespace pdf2eink
{
    public class CbBook : ICBook
    {
        public CbBook()
        {

        }

        public CbBook(string path)
        {
            Open(path);
        }
        public CbBook(Stream stream)
        {
            Open(stream);
        }

        public CbBook(int w, int h)
        {
            Width = w;
            Height = h;
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
                var len2 = BitConverter.ToUInt16(bts, (int)accum);
                accum += 2;
                var str2 = Encoding.UTF8.GetString(bts, accum, len2);
                accum += len2;
                Toc.Items.Add(new TOCItem()
                {
                    Header = str,
                    Text = str2,
                    Page = page,
                    Ident = ident
                });
            }
        }

        public void UpdatePage(byte[] data, int pageNo)
        {
            var width = BitConverter.ToUInt16(bts, 8);
            var height = BitConverter.ToUInt16(bts, 10);
            int stride = 4 * (int)Math.Ceiling(width / 8 / 4f);//aligned 4
            var size = stride * height;
            var pageOffset = 12 + tocRawSize + size * pageNo;

            var part1 = bts.Take(pageOffset).ToArray();
            var part2 = bts.Skip(pageOffset).Skip(size).ToArray();

            bts = part1.Concat(data).Concat(part2).ToArray();
        }

        public void InsertPage(int pageNo)
        {
            var width = BitConverter.ToUInt16(bts, 8);
            var height = BitConverter.ToUInt16(bts, 10);
            int stride = 4 * (int)Math.Ceiling(width / 8 / 4f);//aligned 4
            var size = stride * height;

            var pageOffset = 12 + tocRawSize + size * pageNo;

            var part1 = bts.Take(pageOffset).ToArray();
            byte[] blank = new byte[size];
            for (int i = 0; i < blank.Length; i++)
            {
                blank[i] = 0x0;
            }
            var part2 = bts.Skip(pageOffset).ToArray();

            bts = part1.Concat(blank).Concat(part2).ToArray();
            pages++;
            UpdatePagesCount();
        }

        public void UpdatePagesCount()
        {
            var upd1 = BitConverter.GetBytes(pages);
            for (int i = 0; i < upd1.Length; i++)
            {
                bts[4 + i] = upd1[i];
            }
        }

        public void DeletePage(int pageNo)
        {
            var width = BitConverter.ToUInt16(bts, 8);
            var height = BitConverter.ToUInt16(bts, 10);
            int stride = 4 * (int)Math.Ceiling(width / 8 / 4f);//aligned 4
            var size = stride * height;

            var part1 = bts.Take(12 + tocRawSize + pageNo * size).ToArray();
            var part2 = bts.Skip(12 + tocRawSize + pageNo * size).Skip(size).ToArray();

            bts = part1.Concat(part2).ToArray();

            pages--;
            UpdatePagesCount();
            //trackBar1.Maximum = pages - 1;
            //showPage();
        }

        public Bitmap GetPage(int pageNo)
        {
            if (Format == 2)//parse sections
            {
                return GetPagesViaSections(pageNo);
            }
            //old format
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
                    bmp.SetPixel(i, j, b ? Color.Black : Color.White);
                }
            }
            return bmp;
        }

        public int GetPageOffset(int page)
        {
            var pagesArraySectionOffset = GetSectionOffset(0xC0);
            var pagesTableSectionOffset = GetSectionOffset(0xB0) + 9;
            return pagesArraySectionOffset + BitConverter.ToInt32(bts, pagesTableSectionOffset + page * 4);
        }

        private Bitmap GetPagesViaSections(int pageNo)
        {
            //parse sections
            var bookInfoSectionOffset = GetSectionOffset(0x10) + 5;

            var width = BitConverter.ToUInt16(bts, bookInfoSectionOffset + 4);
            var height = BitConverter.ToUInt16(bts, bookInfoSectionOffset + 6);

            int stride = 4 * (int)Math.Ceiling(width / 8 / 4f);//aligned 4
            var size = stride * height;

            //get page offset
            var pOffset = GetPageOffset(pageNo);
            var pageType = bts[pOffset];
            Bitmap bmp = new Bitmap(width, height);

            if (pageType == 0x1)
            {
                //raw
                var page1 = bts.Skip(pOffset + 1).Take(size).ToArray();
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
            }
            else
            {
                throw new NotImplementedException("not implemented yet");
            }

            return bmp;
        }

        private int GetSectionOffset(byte searchType)
        {
            int pos = 4;
            while (pos < bts.Length)
            {
                var len = BitConverter.ToUInt16(bts, pos);
                var type = bts[pos + 4];

                if (type == searchType)
                    return pos;

                pos += len;
            }
            return -1;
        }

        public byte Format => bts[2];
        public byte Flags => bts[3];
        public void AppendTOC(TOC toc)
        {
            header[3] = 1;
            Toc = toc;

        }
        public static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        internal void Open(Stream stream)
        {
            bts = ReadFully(stream);
            Load(bts);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        void Load(byte[] data)
        {
            header = bts.Take(12).ToArray();
            body = bts.Skip(12).ToArray();
            var format = bts[3];
            if (format == 1)
            {
                ParseTOC();
                body = body.Skip(tocRawSize).ToArray();
            }
            if (Format == 2)
            {
                //parse sections
                var bookInfoSectionOffset = GetSectionOffset(0x10) + 5;
                pages = BitConverter.ToInt32(bts, bookInfoSectionOffset);
            }
            else
            {
                pages = BitConverter.ToInt32(bts, 4);
                Width = BitConverter.ToUInt16(bts, 8);
                Height = BitConverter.ToUInt16(bts, 10);
            }
        }

        void UpdateBodyAndHeader()
        {
            header = bts.Take(12).ToArray();
            body = bts.Skip(12).ToArray();
            var format = bts[3];
            if (format == 1)
            {
                //ParseTOC();
                body = body.Skip(tocRawSize).ToArray();
            }

        }
        internal void Open(string path)
        {
            bts = File.ReadAllBytes(path);
            Load(bts);
        }

        internal void SaveAs(string fileName)
        {
            File.WriteAllBytes(fileName, GetBytes());
        }

        internal byte[] GetBytes()
        {
            UpdateBodyAndHeader();
            MemoryStream ms = new MemoryStream();
            ms.Write(header, 0, header.Length);

            BookExporter.AppendTOC(Toc, ms);
            ms.Write(body, 0, body.Length);

            return ms.ToArray();
        }

        public int pages { get; set; }

        public TOC Toc { get; private set; }

        public bool HasTOC => Toc != null && Toc.Items.Any();

        public string Name { get; set; }
    }
}