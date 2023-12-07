using ImageProcessing.Gaussian;
using NodeControllers.Controllers;
using NodeControllers.Controllers.Fabric;
using NodeControllers.Controllers.Fabric.UDP;
using System.Drawing;

namespace ClientApplication
{
    public partial class ClientForm : Form
    {
        private IIOController client;
        private int yLocation = 0;

        public ClientForm()
        {
            InitializeComponent();

            IFabric fabric = new UdpFabric();
            client = fabric.GetClientController();

            client.OnReceiving += (data) =>
            {
                MemoryStream stream = new MemoryStream();
                stream.Write(data);
                var filteredBitmap = (Bitmap)Bitmap.FromStream(stream);
                stream.Close();
                filteredPanel.Invoke((MethodInvoker)delegate
                {
                    PictureBox box = new PictureBox();
                    box.SizeMode = PictureBoxSizeMode.StretchImage;
                    box.Image = filteredBitmap;
                    box.Width = imagePanel.Width;
                    box.Height = 450;
                    box.Location = new Point(0, yLocation);
                    filteredPanel.Controls.Add(box);
                    yLocation += 470;
                });
            };

            client.Start();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                imagePanel.Controls.Clear();
                filteredPanel.Controls.Clear();

                yLocation = 0;
                foreach (var filename in openFileDialog.FileNames)
                {
                    Image bitmap = Bitmap.FromFile(filename);
                    PictureBox box = new PictureBox();
                    box.SizeMode = PictureBoxSizeMode.StretchImage;
                    box.Image = bitmap;
                    box.Width = imagePanel.Width;
                    box.Height = 450;
                    box.Location = new Point(0, yLocation);
                    yLocation += 470;

                    imagePanel.Controls.Add(box);
                }
                yLocation = 0;
            }
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            if (imagePanel.Controls.Count == 0)
            {
                MessageBox.Show("Не загружены изображения");
                return;
            }

            int radius;
            double sigma;

            string radiusString = radiusTextBox.Text;
            string sigmaString = sigmaTextBox.Text;

            if (!int.TryParse(radiusString, out radius) || !double.TryParse(sigmaString, out sigma))
            {
                MessageBox.Show("Введены некорректные значения сигмы и радиуса");
                return;
            }

            foreach (var control in imagePanel.Controls)
            {
                var box = (PictureBox)control;
                GaussianTask task = new GaussianTask();
                task.Radius = radius;
                task.Sigma = sigma;
                task.Bitmap = (Bitmap)box.Image;
                var bytes = task.ToBytes();
                client.Send(bytes);
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}