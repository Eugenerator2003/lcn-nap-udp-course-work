using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Gaussian
{
    public class CustomGaussianFilter : GaussianFilter
    {
        public double[,] Kernel { get; protected set; }

        private int offset;
        private int kernelSize;

        public CustomGaussianFilter(int radius, double sigma) : base(radius, sigma)
        {
            SetParameters(radius, sigma);
        }

        public override Bitmap Apply(Bitmap input)
        {
            Bitmap output = new Bitmap(input.Width, input.Height);

            for (int y = 0; y < input.Height; y++)
            {
                for (int x = 0; x < input.Width; x++)
                {
                    Color newColor = ApplyKernel(input, x, y);
                    output.SetPixel(x, y, newColor);
                }
            }

            return output;
        }

        public override void SetParameters(int radius, double sigma)
        {
            if (radius != Radius && sigma != Sigma)
            {
                Radius = radius;
                Sigma = sigma;

                kernelSize = 2 * radius + 1;
                offset = kernelSize / 2;
                Kernel = new double[kernelSize, kernelSize];

                double sum = 0;

                for (int i = -radius; i <= radius; i++)
                {
                    for (int j = -radius; j <= radius; j++)
                    {
                        double value = Math.Exp(-(i * i + j * j) / (2.0 * Sigma * Sigma));
                        Kernel[i + radius, j + radius] = value;
                        sum += value;
                    }
                }

                for (int i = 0; i < kernelSize; i++)
                {
                    for (int j = 0; j < kernelSize; j++)
                    {
                        Kernel[i, j] /= sum;
                    }
                }
            }
        }

        private Color ApplyKernel(Bitmap input, int x, int y)
        {
            double red = 0, green = 0, blue = 0;

            for (int i = 0; i < kernelSize; i++)
            {
                for (int j = 0; j < kernelSize; j++)
                {
                    int pixelX = Math.Max(0, Math.Min(input.Width - 1, x + i - offset));
                    int pixelY = Math.Max(0, Math.Min(input.Height - 1, y + j - offset));

                    Color pixel = input.GetPixel(pixelX, pixelY);

                    red += pixel.R * Kernel[i, j];
                    green += pixel.G * Kernel[i, j];
                    blue += pixel.B * Kernel[i, j];
                }
            }

            return Color.FromArgb((int)red, (int)green, (int)blue);
        }
    }
}
