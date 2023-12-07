using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing.Gaussian
{
    public class GaussianTask : ITask
    {
        public static GaussianTask FromBytes(byte[] bytes)
        {
            GaussianTask task = new GaussianTask();

            var radInt = new int[1];
            var sigmaDouble = new double[1];

            Buffer.BlockCopy(bytes, 0, radInt, 0, 4);
            Buffer.BlockCopy(bytes, 4, sigmaDouble, 0, 8);

            MemoryStream stream = new MemoryStream();
            stream.Write(bytes, 12, bytes.Length - 12);
            Bitmap bitmap = (Bitmap)Bitmap.FromStream(stream);

            task.Radius = radInt[0];
            task.Sigma = sigmaDouble[0];
            task.Bitmap = bitmap;

            return task;
        }

        public int Radius { get; set; }

        public double Sigma { get; set; }

        public Bitmap Bitmap { get; set; }

        public byte[] ToBytes()
        {
            MemoryStream stream = new MemoryStream();
            MemoryStream imageStream = new MemoryStream();

            var radBytes = new byte[4];
            Buffer.BlockCopy(new int[] { Radius }, 0, radBytes, 0, 4);
            stream.Write(radBytes);

            var sigmaBytes = new byte[8];
            Buffer.BlockCopy(new double[] { Sigma }, 0, sigmaBytes, 0, 8);
            stream.Write(sigmaBytes);

            Bitmap.Save(imageStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            stream.Write(imageStream.ToArray());

            return stream.ToArray();
        }
    }
}
