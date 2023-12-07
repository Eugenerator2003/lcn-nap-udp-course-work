using ImageProcessing;
using ImageProcessing.Gaussian;
using System.Diagnostics;
using System.Drawing;

namespace linear_solution
{
    internal class Program
    {
        static readonly string dirname = "images";
        static readonly string filteredDirname = "filtered";

        static void Main(string[] args)
        {
            GaussianFilter gaussianFilter = new AForgeGaussianFilter(3, 1);

            if (!Directory.Exists(dirname))
            {
                Console.WriteLine("Отсутсвует директория image с изображениями");
                return;
            }

            var files = Directory.EnumerateFiles(dirname);
            var filtereds = new List<Bitmap>();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach(var file in files)
            {
                Bitmap bitmap = (Bitmap)Bitmap.FromFile(file);
                Bitmap filtered = gaussianFilter.Apply(bitmap);
                filtereds.Add(filtered);
            }
            stopwatch.Stop();
            Console.WriteLine($"Прошло {stopwatch.ElapsedMilliseconds} миллисекунд");

            if (Directory.Exists(filteredDirname))
            {
                Directory.Delete(filteredDirname);
            }
            Directory.CreateDirectory(filteredDirname);

            int i = 0;
            foreach(var bitmap in filtereds)
            {
                bitmap.Save(filteredDirname + "/" + i + ".jpg");
                i++;
            }
            Console.WriteLine("Успешно сохранено");
            Console.WriteLine("Для продолжения нажмите клавишу Enter...");
            Console.ReadLine();
        }
    }
}