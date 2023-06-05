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
        Mat matOutput;
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
                matOutput = new Mat();

                if (radioButton1.Checked) 
                {
                    capture = new VideoCapture(0);
                    cameraThread = new Thread(new ThreadStart(CaptureCameraCallback));
                    cameraThread.Start();
                }
                else if(radioButton3.Checked)
                {
                    matInput = new Mat(pathToFile);
                    Cv2.Resize(matInput, matOutput, new OpenCvSharp.Size(640, 480));

                    Bitmap bmpWebCam = BitmapConverter.ToBitmap(matOutput);
                    pictureBox1.Image = bmpWebCam;
                    matOutput.Dispose();
                }

            }
        }
        private void CaptureCameraCallback()
        {
            while (runVideo)
            {
                if (capture.Read(matInput))
                {
 
                    Cv2.Resize(matInput, matOutput, new OpenCvSharp.Size(640, 480));

                    Bitmap bmpWebCam = BitmapConverter.ToBitmap(matOutput);
                    pictureBox1.Image = bmpWebCam;
                }
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
    }
}
