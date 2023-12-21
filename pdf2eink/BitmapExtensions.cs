using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace pdf2eink
{
    public static class BitmapExtensions
    {
        public static Bitmap DeepClone(this Bitmap source)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (Bitmap)formatter.Deserialize(stream);
            }
        }
    }
}