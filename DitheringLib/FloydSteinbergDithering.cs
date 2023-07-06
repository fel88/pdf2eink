using System.ComponentModel;

namespace DitheringLib
{
    [Description("Floyd-Steinberg")]
    public sealed class FloydSteinbergDithering : ErrorDiffusionDithering
    {
        #region Constructors

        public FloydSteinbergDithering()
          : base(new byte[,]
                 {
               {
                 0, 0, 7
               },
               {
                 3, 5, 1
               }
                 }, 4, true)
        { }

        #endregion
    }
}
