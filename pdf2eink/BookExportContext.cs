namespace pdf2eink
{
    public class BookExportContext
    {
        public int Pages;
        public Stream Stream;
        public static void PrintFooter(int page, int totalPages, Bitmap bmp1, int pageInfoHeight)
        {
            using var gr = Graphics.FromImage(bmp1);
            /* gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
             gr.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
             gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;*/
            var hh = bmp1.Height - pageInfoHeight - 1;
            gr.FillRectangle(Brushes.White, 0, hh, bmp1.Width, pageInfoHeight + 1);
            
            gr.DrawLine(Pens.Black, 0, hh, bmp1.Width, hh);
            
            var str = $"{page} / {totalPages}";
            /*for (int z = 0; z < str.Length; z++)
            {
                gr.DrawString(str[z].ToString(), new Font("Courier New", 6),
             Brushes.Black, 0, 5 + z * 10);
            }*/
            var ms = gr.MeasureString("99999 / 99999", new Font("Consolas", 7));

            int xx = (page * 15) % (int)(bmp1.Width - ms.Width - 1);
            gr.DrawString(str.ToString(), new Font("Consolas", 7), Brushes.Black, xx, hh - 1);
            
        }

        public static byte[] GetBuffer(Bitmap bmp)
        {

            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
             bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
             bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes); bmp.UnlockBits(bmpData);

            return rgbValues;
        }
        public static byte[] GetCompressedInfoBuffer(Bitmap bmp)
        {
            List<byte> ret = new List<byte>();
            ret.Add(0x01);//page type, compressed , etc..  0x01 - raw rgb array

            // Lock the bitmap's bits. 
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
             bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
             bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes); bmp.UnlockBits(bmpData);
            ret.AddRange(rgbValues);
            return ret.ToArray();
        }

        public List<long> PagesOffsets = new List<long>();

        public long PagesArraySectionOffset;

        public void AppendPage(Bitmap clone)
        {
            var bts = GetBuffer(clone);
            PagesOffsets.Add(Stream.Position - PagesArraySectionOffset);
            Stream.Write(bts);
            Pages++;
        }

        internal void UpdatePages()
        {
            Stream.Seek(4, SeekOrigin.Begin);
            Stream.Write(BitConverter.GetBytes(Pages));
        }
    }
}
