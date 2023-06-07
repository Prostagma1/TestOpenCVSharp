using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Point = OpenCvSharp.Point;
using Size = OpenCvSharp.Size;

namespace Laba4
{
    public partial class Form1 : Form
    {
        bool runVideo, secondFormStarted, findRed;
        readonly Size sizeObject = new Size(640, 480);
        Form2 form;
        string pathToFile = String.Empty;
        VideoCapture capture;
        Mat matInput;
        Thread cameraThread;
        Point clickPoint;
        bool canPrintPoint, canDoMask, canDoMaskWithTemplate, canDoMaskHSV;
        byte[,] keys = new byte[3, 2];
        byte[,] hsvKeys = new byte[3, 2];
        Mat[] templates = new Mat[5];
        Mat[] contours;
        Mat d = new Mat();
        int[] sizeCont = new int[2];
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
                Bitmap bitmapDebugg = new Bitmap(1,1);
                Mat matForSearching = new Mat(sizeObject, MatType.CV_8UC3);
                matInput = radioButton1.Checked ? capture.RetrieveMat() : new Mat(pathToFile).Resize(sizeObject);
                matInput.CopyTo(matForSearching);

                FormVideoProcessing(secondFormStarted);
                if (canPrintPoint)
                {
                    canPrintPoint = false;
                    ReadPixelValue(matInput, clickPoint);
                }
                if (canDoMask) DoMaskByKeys(keys, ref matInput);
                if (canDoMaskWithTemplate) Cv2.BitwiseAnd(templates[comboBox1.SelectedIndex], matInput, matInput);
                if (canDoMaskHSV)
                {
                    matForSearching = matForSearching.CvtColor(ColorConversionCodes.BGR2HSV);
                    DoMaskByKeys(hsvKeys, ref matForSearching,findRed);

                    matForSearching = matForSearching.GaussianBlur(new Size(7, 7), 0);
                    bitmapDebugg = BitmapConverter.ToBitmap(matForSearching.CvtColor(ColorConversionCodes.HSV2BGR).Resize(new Size(256, 256)));

                    matForSearching = matForSearching.Canny(250, 100);
                    matForSearching.FindContours(out contours, d, RetrievalModes.External, ContourApproximationModes.ApproxNone);
                    foreach (Mat countur in contours)
                    {
                        var a = Cv2.BoundingRect(countur);
                        if (a.Width < sizeCont[0] || a.Height < sizeCont[0] || a.Width > sizeCont[1] || a.Height > sizeCont[1]) continue;
                        Cv2.Rectangle(matInput, a, Scalar.Red);
                    }
                }
                pictureBox1.Image = BitmapConverter.ToBitmap(matInput);
                pictureBox2.Image = bitmapDebugg;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public void DoMaskByKeys(byte[,] colorKeys, ref Mat mat,bool findRed = false)
        {
            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    if (findRed)
                    {
                        if (((mat.At<Vec3b>(i, j)[0] >= 0 && mat.At<Vec3b>(i, j)[0] <= 10 ) || (mat.At<Vec3b>(i, j)[0] >= 165 && mat.At<Vec3b>(i, j)[0] <= 255)) &&
                            ((mat.At<Vec3b>(i, j)[1] >= 163 && mat.At<Vec3b>(i, j)[1] <= 255) || (mat.At<Vec3b>(i, j)[1] >= 126 && mat.At<Vec3b>(i, j)[1] <= 255)) &&
                            ((mat.At<Vec3b>(i, j)[2] >= 134 && mat.At<Vec3b>(i, j)[2] <= 255) || (mat.At<Vec3b>(i, j)[2] >= 165 && mat.At<Vec3b>(i, j)[2] <= 255)))
                        {

                        }
                        else
                        {
                            mat.At<Vec3b>(i, j)[0] = 0;
                            mat.At<Vec3b>(i, j)[1] = 0;
                            mat.At<Vec3b>(i, j)[2] = 0;
                        }
                    }
                    else
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
        public void ReadPixelValue(Mat inputMat, Point point)
        {
            byte[] buffer = new byte[3];
            for (int i = 0; i < 3; i++)
            {
                buffer[i] = inputMat.At<Vec3b>(point.Y, point.X)[i];
            }
            label2.Text = ($"{buffer[0]} {buffer[1]} {buffer[2]}");
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

        private void button5_Click(object sender, EventArgs e)
        {
            sizeCont[0] = int.Parse(textBox8.Text);
            sizeCont[1] = int.Parse(textBox9.Text);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            findRed = false;
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    hsvKeys[0, 0] = 50;
                    hsvKeys[0, 1] = 80;
                    hsvKeys[1, 0] = 200;
                    hsvKeys[1, 1] = 255;
                    hsvKeys[2, 0] = 30;
                    hsvKeys[2, 1] = 255;
                    break;
                case 1:
                    findRed = true;
                    hsvKeys[0, 0] = 0;
                    hsvKeys[0, 1] = 10;
                    hsvKeys[1, 0] = 163;
                    hsvKeys[1, 1] = 255;
                    hsvKeys[2, 0] = 134;
                    hsvKeys[2, 1] = 255;
                    break;
                case 2:
                    hsvKeys[0, 0] = 15; // 18 
                    hsvKeys[0, 1] = 40; // 60
                    hsvKeys[1, 0] = 15; //120
                    hsvKeys[1, 1] = 255; 
                    hsvKeys[2, 0] = 140; //175
                    hsvKeys[2, 1] = 255;
                    break;
                case 3:
                    hsvKeys[0, 0] = 88;
                    hsvKeys[0, 1] = 165;
                    hsvKeys[1, 0] = 150;
                    hsvKeys[1, 1] = 255;
                    hsvKeys[2, 0] = 100;
                    hsvKeys[2, 1] = 255;
                    break;
            }
        }
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            canDoMaskHSV = checkBox4.Checked;
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
            button5_Click(null, null);
            for (byte i = 0; i < 5; i++)
            {
                templates[i] = new Mat($@"D:\Study\4 sem\TechnicalVision\Template\{i + 1}.png").Resize(sizeObject);
            }
            for (byte i = 0; i < 3; i++)
            {
                hsvKeys[i, 0] = 0;
                hsvKeys[i, 1] = byte.MaxValue;

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
