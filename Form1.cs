using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Laba4
{
    public partial class Form1 : Form
    {
        bool runVideo = false;
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
            StartVideo();
        }
        private void StartVideo()
        {
            if (runVideo)
            {
                runVideo = false;
                pictureBox1.Image = null;
                cameraThread.Abort();
                matInput?.Dispose();
                capture?.Dispose();
            }
            else
            {
                runVideo = true;
                matInput = new Mat();

                if (radioButton1.Checked) capture = new VideoCapture(0);
                else if (radioButton2.Checked) capture = new VideoCapture(pathToFile);

                cameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
                cameraThread.Start();
            }
        }
        private void CaptureCameraCallback()
        {
            while (runVideo)
            {
                if (capture.Read(matInput))
                {
                    Mat matOutput = new Mat();
                    Cv2.Resize(matInput, matOutput, new OpenCvSharp.Size(640, 480));

                    //Cv2.Canny(matInput, matOutput, 90, 120);

                    Bitmap bmpWebCam = BitmapConverter.ToBitmap(matOutput);
                    pictureBox1.Image = bmpWebCam;
                    matOutput.Dispose();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (cameraThread != null && cameraThread.IsAlive) cameraThread.Abort();
            matInput?.Dispose();
            capture?.Dispose();
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
                    //if (ext == ".flv")
                    {
                        pathToFile = tempPath;
                        textBox1.Text = pathToFile;
                    }
                }
            }
            file.Dispose();
        }
    }
}
