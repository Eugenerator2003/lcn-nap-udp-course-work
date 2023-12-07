using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Gaussian
{
    public abstract class GaussianFilter : IImageFilter
    {
        public int Radius { get; protected set; }

        public double Sigma { get; protected set; }

        public GaussianFilter(int radius, double sigma)
        {
            Radius = radius;
            Sigma = sigma;
        }

        public abstract Bitmap Apply(Bitmap image);

        public abstract void SetParameters(int radius, double sigma);
    }
}
