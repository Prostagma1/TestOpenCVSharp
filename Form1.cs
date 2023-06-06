using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Point = OpenCvSharp.Point;

namespace Laba4
{
    public partial class Form1 : Form
    {
        bool runVideo = false, secondFormStarted;
        readonly Size sizeObject = new Size(640, 480);
        Form2 form;
        string pathToFile = String.Empty;
        VideoCapture capture;
        Mat matInput;
        Thread cameraThread;
        Point clickPoint;
        bool canPrintPoint, canDoMask, canDoMaskWithTemplate;
        byte[,] keys = new byte[3, 2];
        Mat[] templates = new Mat[5];
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Enabled = !panel1.Enabled;
            panel2.Enabled = panel1.Enabled && radioButton3.Checked;
            StartVideo();
            button1.Text = panel1.Enabled ? "Старт" : "Стоп";
        }
        private void DisposeVideo()
        {
            pictureBox1.Image = null;
            if (cameraThread != null && cameraThread.IsAlive) cameraThread.Abort();
            matInput?.Dispose();
            capture?.Dispose();
        }
        private void StartVideo()
        {
            if (runVideo)
            {
                runVideo = false;
                DisposeVideo();
            }
            else
            {
                runVideo = true;
                matInput = new Mat();

                if (radioButton1.Checked)
                {
                    capture = new VideoCapture(0)
                    {
                        FrameHeight = sizeObject.Height,
                        FrameWidth = sizeObject.Width,
                        AutoFocus = true
                    };
                }
                cameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
                cameraThread.Start();
            }
        }
        private void CaptureCameraCallback()
        {
            while (runVideo)
            {
                matInput = radioButton1.Checked ? capture.RetrieveMat() : new Mat(pathToFile).Resize(sizeObject);

                FormVideoProcessing(secondFormStarted);
                ReadPixelValue(matInput, clickPoint, ref canPrintPoint);
                DoMaskByKeys(canDoMask, keys, ref matInput);
                DoMaskWithTemplate(canDoMaskWithTemplate, ref matInput);

                pictureBox1.Image = BitmapConverter.ToBitmap(matInput);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public void DoMaskWithTemplate(bool enable, ref Mat mat)
        {
            if (enable)
            {
                Cv2.BitwiseAnd(templates[comboBox1.SelectedIndex], mat,mat);
            }
        }
        public void DoMaskByKeys(bool enable, byte[,] colorKeys, ref Mat mat)
        {
            if (enable)
            {
                for (int i = 0; i < mat.Rows; i++)
                {
                    for (int j = 0; j < mat.Cols; j++)
                    {
                        if (mat.At<Vec3b>(i, j)[0] < colorKeys[0, 0] || mat.At<Vec3b>(i, j)[0] > colorKeys[0, 1] ||
                            mat.At<Vec3b>(i, j)[1] < colorKeys[1, 0] || mat.At<Vec3b>(i, j)[1] > colorKeys[1, 1] ||
                            mat.At<Vec3b>(i, j)[2] < colorKeys[2, 0] || mat.At<Vec3b>(i, j)[2] > colorKeys[2, 1])
                        {
                            mat.At<Vec3b>(i, j)[0] = 0;
                            mat.At<Vec3b>(i, j)[1] = 0;
                            mat.At<Vec3b>(i, j)[2] = 0;
                        }
                    }
                }
            }
        }
        public void ReadPixelValue(Mat inputMat, Point point, ref bool enable)
        {
            if (enable)
            {
                enable = false;
                byte[] buffer = new byte[3];
                for (int i = 0; i < 3; i++)
                {
                    buffer[i] = inputMat.At<Vec3b>(point.Y, point.X)[i];
                }
                label2.Text = ($"{buffer[0]} {buffer[1]} {buffer[2]}");
            }
        }
        private void FormVideoProcessing(bool enable)
        {
            if (enable)
            {
                for (sbyte i = 0; i < 4; i++)
                {
                    if (form.colorChange == i) ChangeColorSpace(form.selectedColorSpace, ref matInput);
                    if (form.inRange == i) matInput = matInput.InRange(form.scalars[0], form.scalars[1]);
                    if (form.canny == i) matInput = matInput.Canny(form.cannyValues[0], form.cannyValues[1], form.cannyValues[2]);
                    if (form.blur == i) Bluring(form.selectedBlur, ref matInput);
                }
            }
        }
        private void Bluring(string nameBlur, ref Mat mat)
        {
            switch (nameBlur)
            {
                case "Blur":
                    mat = mat.Blur(new OpenCvSharp.Size(form.valueForBlur, form.valueForBlur));
                    break;
                case "Gaussian Blurring":
                    mat = mat.GaussianBlur(new OpenCvSharp.Size(form.valuesForGauss[0], form.valuesForGauss[0]), form.valuesForGauss[1], form.valuesForGauss[2]);
                    break;
                case "Median Blurring":
                    mat = mat.MedianBlur(form.valueForMedian);
                    break;
                case "Bilateral Filtering":
                    mat = mat.BilateralFilter(form.valuesForBil[0], form.valuesForBil[1], form.valuesForBil[2]);
                    break;
            }
        }
        private void ChangeColorSpace(string nameSpace, ref Mat mat)
        {
            switch (nameSpace)
            {
                case "BGR2GRAY":
                    mat = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
                    break;
                case "BGR2HLS":
                    mat = mat.CvtColor(ColorConversionCodes.BGR2HLS);
                    break;
                case "BGR2HSV":
                    mat = mat.CvtColor(ColorConversionCodes.BGR2HSV);
                    break;
                case "BGR2RGB":
                    mat = mat.CvtColor(ColorConversionCodes.BGR2RGB);
                    break;
                case "BGR2XYZ":
                    mat = mat.CvtColor(ColorConversionCodes.BGR2XYZ);
                    break;
                case "BGR2YUV":
                    mat = mat.CvtColor(ColorConversionCodes.BGR2YUV);
                    break;
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeVideo();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog()
            {
                Multiselect = false
            };
            if (file.ShowDialog() == DialogResult.OK)
            {
                var tempPath = file.FileName;
                if (File.Exists(tempPath))
                {
                    var ext = Path.GetExtension(tempPath);
                    if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
                    {
                        pathToFile = tempPath;
                        textBox1.Text = pathToFile;
                    }
                }
            }
            file.Dispose();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Enabled = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            panel2.Enabled = true;
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            clickPoint = new Point(e.X, e.Y);
            canPrintPoint = checkBox1.Checked && true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            canDoMask = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            canDoMaskWithTemplate = checkBox3.Checked;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!(byte.TryParse(textBox2.Text, out keys[0, 0]) && byte.TryParse(textBox3.Text, out keys[0, 1]) &&
                byte.TryParse(textBox4.Text, out keys[1, 0]) && byte.TryParse(textBox5.Text, out keys[1, 1]) &&
                byte.TryParse(textBox6.Text, out keys[2, 0]) && byte.TryParse(textBox7.Text, out keys[2, 1])))
            {
                MessageBox.Show("Данные не верно введены!\nДанные должны быть в диапазоне [0;255]", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            for (byte i = 0; i < 5; i++)
            {
                templates[i] = new Mat($@"D:\Study\4 sem\TechnicalVision\Template\{i + 1}.png").Resize(sizeObject);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            form?.Dispose();
            form = new Form2();
            form.Show();
            secondFormStarted = true;
        }
    }
}
