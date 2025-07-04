﻿namespace pdf2eink
{
    public static class StreamExtensions
    {
        public static int ReadInt(this Stream stream)
        {            
            byte[] bb = new byte[4];
            stream.Read(bb, 0, 4); 
            return BitConverter.ToInt32(bb);
        }
        public static ushort ReadUInt16(this Stream stream)
        {
            byte[] bb = new byte[4];
            stream.Read(bb, 0, 2);
            return BitConverter.ToUInt16(bb);
        }
    }
}
