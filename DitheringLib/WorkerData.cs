using System.Drawing;

namespace DitheringLib
{
    internal sealed class WorkerData
    {
        public Bitmap Image { get; set; }

        public IErrorDiffusion Dither { get; set; }

        public IPixelTransform Transform { get; set; }

        public int ColorCount { get; set; }
    }
}
