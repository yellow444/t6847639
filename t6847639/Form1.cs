using Emgu.CV;
using Emgu.CV.Structure;

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace t6847639
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            openFileDialog1.InitialDirectory = Application.StartupPath;
            openFileDialog1.Filter = "ori files (*.ori)|*.ori|All files (*.*)|*.*";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Выбираем и читаем файл
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                byte[] bytesfile = ReadBinFile(openFileDialog1.FileName);
                // Если не пустой читаем размеры изображения 1-4 и 5-8 байты
                if (bytesfile != BitConverter.GetBytes(0))
                {
                    int width = BitConverter.ToInt32(bytesfile, 0);
                    int height = BitConverter.ToInt32(bytesfile, 4);
                    byte[] imageData = new byte[width * height * 2];
                    Array.Copy(bytesfile, 16, imageData, 0, imageData.Length);
                    // Создаем пустое изображение
                    Image<Gray, UInt16> depthImage = new Image<Gray, UInt16>(width, height);
                    // Загружаем массив данных из файла
                    depthImage.Bytes = imageData;
                    // Копируем в imageBox1
                    imageBox1.Image = depthImage.Copy().Convert<Bgra, byte>();
                    // Создаем гистограмму
                    GrayHist(depthImage.Copy());
                    // Сохраняем результаты
                    depthImage.Convert<Bgra, byte>().Save(Path.ChangeExtension(openFileDialog1.FileName, ".bmp"));
                    Bitmap histo = new Bitmap(width, height);
                    histogramBox1.DrawToBitmap(histo, new Rectangle(0, 0, histo.Width, histo.Height));
                    histo.Save(Path.ChangeExtension(openFileDialog1.FileName, "histo.bmp"));
                }
            }
        }

        private void GrayHist(Image<Gray, UInt16> image)
        {
            histogramBox1.ClearHistogram();
            histogramBox1.GenerateHistograms(image, 256);
            histogramBox1.Refresh();
        }

        private static byte[] ReadBinFile(string filename)
        {
            byte[] bytes;
            try
            {
                using (FileStream fsSource = new FileStream(filename,
                    FileMode.Open, FileAccess.Read))
                {
                    // Read the source file into a byte array.
                    bytes = new byte[fsSource.Length];
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    while (numBytesToRead > 0)
                    {
                        // Read may return anything from 0 to numBytesToRead.
                        int n = fsSource.Read(bytes, numBytesRead, numBytesToRead);

                        // Break when the end of the file is reached.
                        if (n == 0)
                            break;

                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    numBytesToRead = bytes.Length;
                    return bytes;
                }
            }
            catch (FileNotFoundException Ex)
            {
                Console.WriteLine(Ex.Message);
            }

            return BitConverter.GetBytes(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}