﻿using NodeControllers.Controllers;
using NodeControllers.Controllers.Fabric.UDP;
using NodeControllers.Controllers.Fabric;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using ImageProcessing.Gaussian;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ClientWpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IIOController client;
        private ConcurrentQueue<System.Drawing.Bitmap> filtered;
        private Rotation[] rotations = new Rotation[] { Rotation.Rotate0, Rotation.Rotate90, Rotation.Rotate180, Rotation.Rotate270 };
        int counter = 0;

        public MainWindow()
        {
            InitializeComponent();
            filtered = new();

            IFabric fabric = new UdpFabric();
            client = fabric.GetClientController();

            client.OnReceiving += (data) =>
            {
                MemoryStream stream = new MemoryStream();
                stream.Write(data);

                try
                {
                    var bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(stream);
                    filtered.Enqueue(bitmap);
                }
                catch
                {
                    counter--;
                    MessageBox.Show("!!!", "Ошибка!");
                }
            };

            client.OnFailedMessaging += () =>
            {
                MessageBox.Show("Сервер недоступен!", "Ошибка соединения");
                client.Dispose();
            };

            client.Start();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                images.Children.Clear();
                filteredImages.Children.Clear();

                foreach(var filename in dialog.FileNames)
                {
                    ///for (int i = 0; i < 4; i++)
                    //{
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(filename);
                        //bitmap.Rotation = rotations[i];
                        bitmap.EndInit();
                        AddToStackPanel(images, bitmap);
                    //}
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (images.Children.Count == 0)
            {
                MessageBox.Show("Не загружены картинки", "Ошибка!");
                return;
            }

            string radiusText = radiusTextBox.Text;
            string sigmaText = sigmaTextBox.Text;

            int radius;
            double sigma;

            if (!int.TryParse(radiusText, out radius) || !double.TryParse(sigmaText, out sigma))
            {
                MessageBox.Show("Некорректные значения сигмы или радиуса", "Ошибка!");
                return;
            }

            filteredImages.Children.Clear();
            counter = 0;
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var imageBox in images.Children)
            {
                BitmapImage bitmapImage = (BitmapImage)(imageBox as Image).Source;
                GaussianTask gaussianTask = new GaussianTask();
                gaussianTask.Radius = radius;
                gaussianTask.Sigma = sigma;
                gaussianTask.Bitmap = BitmapCast.GetBitmapFromBitmapImage(bitmapImage);
                byte[] bytes = gaussianTask.ToBytes();
                client.Send(bytes);
                counter++;
            }
            
            Dispatcher.BeginInvoke(() =>
            {
                double all = counter;

                while (counter > 0)
                {
                    if (filtered.TryDequeue(out var bitmap))
                    {
                        var image = BitmapCast.GetBitmapImageFromBitmap(bitmap);
                        AddToStackPanel(filteredImages, image);
                        counter--;
                        //MessageBox.Show($"{(all - counter) / all * 100}%");
                    }
                }

                stopwatch.Stop();
                MessageBox.Show($"На обработку изображений было затрачено {stopwatch.ElapsedMilliseconds} миллисекунд", "Время");
            });
        }

        private void AddToStackPanel(StackPanel panel, BitmapImage bitmapImage)
        {
            Image image = new Image();
            image.Stretch = Stretch.Fill;
            image.Width = 350;
            image.Height = 350;
            image.Source = bitmapImage;
            panel.Children.Add(image);
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            if (filteredImages.Children.Count == 0)
            {
                MessageBox.Show("Нет обработанных изображений", "Ошибка!");
                return;
            }

            string dirname = "Filtered images";
            if (Directory.Exists(dirname))
            {
                Directory.Delete(dirname, true);
            }
            Directory.CreateDirectory(dirname);
            var imageBoxes = filteredImages.Children;
            int count = imageBoxes.Count;
            for (int i = 0; i < count; i++)
            {
                BitmapEncoder encoder = new JpegBitmapEncoder();
                var box = (Image)imageBoxes[i];
                encoder.Frames.Add(BitmapFrame.Create((BitmapImage)box.Source));
                using (var fileStream = new FileStream(dirname + '/' + (i + 1) + ".jpg", FileMode.CreateNew))
                {
                    encoder.Save(fileStream);
                }
            }

            MessageBox.Show("Данные успешно сохранены", "Успех");
        }
    }
}
