using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Gaussian
{
    public class AForgeGaussianFilter : GaussianFilter
    {
        GaussianBlur filter;

        public AForgeGaussianFilter(int radius) : base(radius)
        {
            filter = new GaussianBlur(radius);
        }

        public override Bitmap Apply(Bitmap image)
        {
            return filter.Apply(image);
        }

        public override void ResizeKernel(int radius)
        {
            filter.Size = radius;
        }
    }
}
