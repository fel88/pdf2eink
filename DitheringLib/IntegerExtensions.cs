namespace DitheringLib
{
    internal static class IntegerExtensions
    {
        #region Static Methods

        internal static byte ToByte(this int value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 255)
            {
                value = 255;
            }

            return (byte)value;
        }

        #endregion
    }
}
