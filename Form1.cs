using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.IO;
using System.Threading;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace Laba4
{
    public partial class Form1 : Form
    {
        bool runVideo = false, secondFormStarted;
        readonly OpenCvSharp.Size sizeObject = new OpenCvSharp.Size(640, 480);
        Form2 form;
        string pathToFile = null;
        VideoCapture capture;
        Mat matInput;
        Thread cameraThread;
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


                pictureBox1.Image = BitmapConverter.ToBitmap(matInput);

                Cv2.WaitKey(30);
                GC.Collect();
                GC.WaitForPendingFinalizers();
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

        private void Form1_Load(object sender, EventArgs e)
        {

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
