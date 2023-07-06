using System.Runtime.InteropServices;

namespace DitheringLib
{
    [StructLayout(LayoutKind.Explicit)]
    public struct ArgbColor
    {
        [FieldOffset(0)]
        private readonly int _value;

        /// <summary>
        /// Gets the blue component value of this <see cref="ArgbColor"/> structure.
        /// </summary>
        [FieldOffset(0)]
        public byte B;

        /// <summary>
        /// Gets the green component value of this <see cref="ArgbColor"/> structure.
        /// </summary>
        [FieldOffset(1)]
        public byte G;

        /// <summary>
        /// Gets the red component value of this <see cref="ArgbColor"/> structure.
        /// </summary>
        [FieldOffset(2)]
        public byte R;

        /// <summary>
        /// Gets the alpha component value of this <see cref="ArgbColor"/> structure.
        /// </summary>
        [FieldOffset(3)]
        public byte A;

        public ArgbColor(int red, int green, int blue)
          : this(255, red, green, blue)
        { }

        public ArgbColor(int alpha, int red, int green, int blue)
          : this()
        {
            A = (byte)alpha;
            R = (byte)red;
            G = (byte)green;
            B = (byte)blue;
        }

        internal static ArgbColor FromArgb(byte a, byte r, byte g, byte b)
        {
            return new ArgbColor(a, r, g, b);
        }

        internal static ArgbColor FromArgb(byte r, byte g, byte b)
        {
            return new ArgbColor(r, g, b);
        }

        public int ToArgb()
        {
            return _value;
        }
    }
}
