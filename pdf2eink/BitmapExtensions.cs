namespace pdf2eink
{
    public static class BitmapExtensions
    {
        public static Bitmap DeepClone(this Bitmap source)
        {           
            var stream = new MemoryStream();
            source.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            using (stream)
            {                
                return (Bitmap)Bitmap.FromStream(stream);
            }
        }
    }
}