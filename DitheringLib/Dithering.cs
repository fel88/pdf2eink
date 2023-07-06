using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DitheringLib
{

    public class Dithering
    {
        public Bitmap Process(Bitmap bmp)
        {
            WorkerData workerData;
            IPixelTransform transform;
            IErrorDiffusion ditherer;
            Bitmap image;




            transform = this.GetPixelTransform();
            ditherer = this.GetDitheringInstance();
            image = bmp.Copy();

            workerData = new WorkerData
            {
                Image = image,
                Transform = transform,
                Dither = ditherer,
                ColorCount = this.GetMaximumColorCount()
            };


            backgroundWorker_RunWorkerCompleted(new RunWorkerCompletedEventArgs(this.GetTransformedImage(workerData), null, false));

            return _transformed;
        }
        private Bitmap GetTransformedImage(WorkerData workerData)
        {
            Bitmap image;
            Bitmap result;
            ArgbColor[] pixelData;
            Size size;
            IPixelTransform transform;
            IErrorDiffusion dither;

            transform = workerData.Transform;
            dither = workerData.Dither;
            image = workerData.Image;
            size = image.Size;
            pixelData = image.GetPixelsFrom32BitArgbImage();

            if (dither != null && dither.Prescan)
            {
                // perform the dithering on the source data before
                // it is transformed
                this.ProcessPixels(pixelData, size, null, dither);
                dither = null;
            }

            // scan each pixel, apply a transform the pixel
            // and then dither it
            this.ProcessPixels(pixelData, size, transform, dither);

            // create the final bitmap
            result = pixelData.ToBitmap(size);

            return result;
        }
        private void ProcessPixels(ArgbColor[] pixelData, Size size, IPixelTransform pixelTransform, IErrorDiffusion dither)
        {
            for (int row = 0; row < size.Height; row++)
            {
                for (int col = 0; col < size.Width; col++)
                {
                    int index;
                    ArgbColor current;
                    ArgbColor transformed;

                    index = row * size.Width + col;

                    current = pixelData[index];

                    // transform the pixel
                    if (pixelTransform != null)
                    {
                        transformed = pixelTransform.Transform(pixelData, current, col, row, size.Width, size.Height);
                        pixelData[index] = transformed;
                    }
                    else
                    {
                        transformed = current;
                    }

                    // apply a dither algorithm to this pixel
                    // assuming it wasn't done before
                    dither?.Diffuse(pixelData, current, transformed, col, row, size.Width, size.Height);
                }
            }
        }
        private Bitmap _transformed;
        private void CleanUpTransformed()
        {
            //transformedImageBox.Image = null;
            // transformedColorsToolStripStatusLabel.Text = string.Empty;

            if (_transformed != null)
            {
                _transformed.Dispose();
                _transformed = null;
            }
        }

        private ArgbColor[] _transformedImage;
        private void backgroundWorker_RunWorkerCompleted(RunWorkerCompletedEventArgs e)
        {
            this.CleanUpTransformed();

            if (e.Error != null)
            {
                //MessageBox.Show("Failed to transform image. " + e.Error.GetBaseException().Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                _transformed = e.Result as Bitmap;
                _transformedImage = _transformed.GetPixelsFrom32BitArgbImage();

                //transformedImageBox.Image = _transformed;

                ThreadPool.QueueUserWorkItem(state =>
                {
                    int count;

                    count = this.GetColorCount(_transformedImage);


                });
            }


        }

        private int GetColorCount(ArgbColor[] pixels)
        {
            HashSet<int> colors;

            colors = new HashSet<int>();

            foreach (ArgbColor color in pixels)
            {
                colors.Add(color.ToArgb());
            }

            return colors.Count;
        }

        private IErrorDiffusion GetDitheringInstance()
        {
            IErrorDiffusion result;


            result = new FloydSteinbergDithering();


            return result;
        }

        private int GetMaximumColorCount()
        {
            int result;

            result = 256;


            result = 2;

            return result;
        }

        private IPixelTransform GetPixelTransform()
        {
            IPixelTransform result;

            result = null;


            result = new MonochromePixelTransform((byte)127);

            return result;
        }

    }
}
