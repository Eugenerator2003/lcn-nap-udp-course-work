using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessing
{
    /// <summary>
    /// Задача для обработки изображения
    /// </summary>
    internal interface ITask
    {
        Bitmap Bitmap { get; }
    }
}
