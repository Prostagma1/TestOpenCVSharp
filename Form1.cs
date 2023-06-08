using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
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
        bool runVideo, secondFormStarted, findRed, canPrintPoint, canDoMask, canDoMaskWithTemplate, canDoMaskHSV;
        readonly Size sizeObject = new Size(640, 480);
        Form2 form;
        string pathToFile = String.Empty;
        List<Rect> rects;
        VideoCapture capture;
        Mat matInput;
        Thread cameraThread;
        Point clickPoint;
        byte[,] keys = new byte[3, 2];
        byte[,] hsvKeys = new byte[3, 2];
        Mat[,] templates = new Mat[5, 2];
        int[] sizeCont = new int[2];
        string[] names = {
            "Stop",
            "Road up",
            "Reversal",
            "Left",
            "Directly"
        };
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
                Bitmap bitmapDebugg = new Bitmap(1, 1);
                matInput = radioButton1.Checked ? capture.RetrieveMat() : new Mat(pathToFile).Resize(sizeObject);

                FormVideoProcessing(secondFormStarted);
                if (canPrintPoint)
                {
                    canPrintPoint = false;
                    ReadPixelValue(matInput, clickPoint);
                }
                if (canDoMask) DoMaskByKeys(keys, ref matInput, false, false);
                if (canDoMaskWithTemplate) Cv2.BitwiseAnd(templates[comboBox1.SelectedIndex, 0].Resize(sizeObject).CvtColor(ColorConversionCodes.GRAY2BGR), matInput, matInput);
                if (canDoMaskHSV)
                {
                    rects = new List<Rect>();
                    SearchingContours(ref matInput, sizeCont, out rects, out bitmapDebugg, checkBox5.Checked);

                    label4.Text = rects.Count.ToString();
                    SearchSigns(ref matInput);
                }
                pictureBox1.Image = BitmapConverter.ToBitmap(matInput);
                pictureBox2.Image = bitmapDebugg;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        public void SearchSigns(ref Mat mat)
        {
            for (int g = 0; g < rects.Count; g++)
            {
                byte sign;
                Mat[] matRYB =
                {
                     new Mat(matInput, rects[g]).Resize(new Size(128, 128)),
                     new Mat(matInput, rects[g]).Resize(new Size(128, 128)),
                     new Mat(matInput, rects[g]).Resize(new Size(128, 128))
                };

                for (byte i = 0; i < 3; i++)
                {
                    ChangeKeys(i);
                    DoMaskByKeys(hsvKeys, ref matRYB[i], findRed);
                    matRYB[i] = matRYB[i].Threshold(20, 255, ThresholdTypes.Binary);
                }
                Сomparison(matRYB, out sign);
                if (sign != 255)
                {
                    mat.Rectangle(rects[g], Scalar.Green,2);
                    mat.PutText(names[sign], new Point(rects[g].X-5, rects[g].Y-5), HersheyFonts.HersheySimplex, 0.7, Scalar.Green,2);
                }
            }

        }
        public void SearchingContours(ref Mat inputMat, int[] sizeCountur, out List<Rect> filteredCounturs, out Bitmap bitmapDebugg, bool drawRectangle = false)
        {
            filteredCounturs = new List<Rect>();
            Mat matForSearching = new Mat(sizeObject, MatType.CV_8UC3);
            Mat mat = new Mat();
            Mat[] contours;
            inputMat.CopyTo(matForSearching);

            matForSearching = matForSearching.MedianBlur(3).Canny(70, 150);
            bitmapDebugg = BitmapConverter.ToBitmap(matForSearching.Resize(new Size(256, 256)));
            matForSearching.FindContours(out contours, mat, RetrievalModes.Tree, ContourApproximationModes.ApproxSimple);
            foreach (Mat countur in contours)
            {
                var a = Cv2.BoundingRect(countur);
                if (a.Width < sizeCountur[0] || a.Height < sizeCountur[0] || a.Width > sizeCountur[1] || a.Height > sizeCountur[1]) continue;
                if (drawRectangle)
                {
                    Cv2.Rectangle(matInput, a, Scalar.Red);
                    matInput.PutText(a.ToString(), new Point(a.X - 5, a.Y - 15), HersheyFonts.HersheySimplex, 0.5, Scalar.Red);
                }
                filteredCounturs.Add(a);
            }
        }
        public void DoMaskByKeys(byte[,] colorKeys, ref Mat mat, bool red = false, bool hsv = true)
        {
            if (hsv) mat = mat.CvtColor(ColorConversionCodes.BGR2HSV);
            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    if (red)
                    {
                        if ((mat.At<Vec3b>(i, j)[0] >= 0 && mat.At<Vec3b>(i, j)[0] <= 10) || (mat.At<Vec3b>(i, j)[0] >= 165 && mat.At<Vec3b>(i, j)[0] <= 180) &&
                            mat.At<Vec3b>(i, j)[1] >= 135 && mat.At<Vec3b>(i, j)[1] <= 255 && mat.At<Vec3b>(i, j)[2] >= 60 && mat.At<Vec3b>(i, j)[2] <= 255)

                        {

                        }
                        else
                        {
                            mat.At<Vec3b>(i, j)[0] = 0;
                            mat.At<Vec3b>(i, j)[1] = 0;
                            mat.At<Vec3b>(i, j)[2] = 0;
                        }
                    }
                    else if (mat.At<Vec3b>(i, j)[0] < colorKeys[0, 0] || mat.At<Vec3b>(i, j)[0] > colorKeys[0, 1] ||
                        mat.At<Vec3b>(i, j)[1] < colorKeys[1, 0] || mat.At<Vec3b>(i, j)[1] > colorKeys[1, 1] ||
                        mat.At<Vec3b>(i, j)[2] < colorKeys[2, 0] || mat.At<Vec3b>(i, j)[2] > colorKeys[2, 1])
                    {
                        mat.At<Vec3b>(i, j)[0] = 0;
                        mat.At<Vec3b>(i, j)[1] = 0;
                        mat.At<Vec3b>(i, j)[2] = 0;
                    }

                }
            }
            if (hsv) mat = mat.CvtColor(ColorConversionCodes.HSV2BGR).CvtColor(ColorConversionCodes.BGR2GRAY);
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
        private void Сomparison(Mat[] mats, out byte index)
        {
            var tempMat = new Mat();
            index = 255;
            for (byte i = 0; i < 3; i++)
            {
                double maxProc = -1;
                if (i == 0)
                {
                    for (byte j = 0; j < 2; j++)
                    {
                        mats[i].CopyTo(tempMat);
                        int count = 0;
                        double maxCount = templates[j, 0].CountNonZero();

                        Cv2.BitwiseAnd(mats[i], templates[j, 0], tempMat);
                        count += tempMat.CountNonZero();

                        Cv2.BitwiseAnd(mats[i], templates[j, 1], tempMat);
                        count -= tempMat.CountNonZero();

                        if (count / maxCount > maxProc)
                        {
                            maxProc = count / maxCount;
                            if (maxProc > 0.54)
                            {
                                index = j;
                            }
                        }
                    }
                }
                else if (i == 2)
                {
                    for (byte j = 2; j < 5; j++)
                    {
                        mats[i].CopyTo(tempMat);
                        int count = 0;
                        double maxCount = templates[j, 0].CountNonZero();

                        Cv2.BitwiseAnd(mats[i], templates[j, 0], tempMat);
                        count += tempMat.CountNonZero();

                        Cv2.BitwiseAnd(mats[i], templates[j, 1], tempMat);
                        count -= tempMat.CountNonZero();

                        if (count / maxCount > maxProc)
                        {
                            maxProc = count / maxCount;
                            if (maxProc > 0.54)
                            {
                                index = j;
                            }
                        }
                    }
                }
            }
        }
        private void ChangeKeys(byte id)
        {
            findRed = false;
            switch (id)
            {
                case 0://R
                    findRed = true;
                    hsvKeys[0, 0] = 0;
                    hsvKeys[0, 1] = 15;
                    hsvKeys[1, 0] = 87;
                    hsvKeys[1, 1] = 255;
                    hsvKeys[2, 0] = 96;
                    hsvKeys[2, 1] = 255;
                    break;
                case 1: //Y
                    hsvKeys[0, 0] = 15; // 18 
                    hsvKeys[0, 1] = 40; // 60
                    hsvKeys[1, 0] = 15; //120
                    hsvKeys[1, 1] = 255;
                    hsvKeys[2, 0] = 140; //175
                    hsvKeys[2, 1] = 255;
                    break;
                case 2: //B
                    hsvKeys[0, 0] = 88;
                    hsvKeys[0, 1] = 117;
                    hsvKeys[1, 0] = 27;
                    hsvKeys[1, 1] = 255;
                    hsvKeys[2, 0] = 33;
                    hsvKeys[2, 1] = 255;
                    break;
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            sizeCont[0] = int.Parse(textBox8.Text);
            sizeCont[1] = int.Parse(textBox9.Text);
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
                templates[i, 0] = new Mat($@"D:\Study\4 sem\TechnicalVision\Template\{i + 1}.png", ImreadModes.Grayscale).Resize(new Size(128, 128));
                templates[i, 1] = new Mat($@"D:\Study\4 sem\TechnicalVision\Template\{i + 1}_N.jpg", ImreadModes.Grayscale).Resize(new Size(128, 128));
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
