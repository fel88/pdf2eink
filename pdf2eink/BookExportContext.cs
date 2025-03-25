namespace pdf2eink
{
    public class BookExportContext
    {
        public int Pages;
        public Stream Stream; 
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
