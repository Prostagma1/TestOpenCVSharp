using OpenCvSharp;
using System;
using System.Windows.Forms;

namespace Laba4
{

    public partial class Form2 : Form
    {
        public sbyte colorChange = -1;
        public sbyte inRange = -1;
        public sbyte canny = -1;
        public sbyte blur = -1;
        public sbyte gradientCanny = -1;
        public Scalar[] scalars = new Scalar[2];
        public int[] cannyValues = new int[3];
        public string selectedColorSpace;
        public string selectedBlur;
        public int valueForBlur = 1;
        public int[] valuesForGauss = new int[3] { 1, 0, 0 };
        public int valueForMedian = 3;
        public int[] valuesForBil = new int[3];
        sbyte maxScore = -1;
        public Form2()
        {
            InitializeComponent();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                colorChange = ++maxScore;
            }
            else
            {
                colorChange = -1;
                maxScore--;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedColorSpace = (string)comboBox1.SelectedItem;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            scalars[0] = new Scalar(0, 0, 0);
            scalars[1] = new Scalar(0, 0, 0);
            cannyValues[2] = 3;
            panel4.Visible = false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                inRange = ++maxScore;
            }
            else
            {
                inRange = -1;
                maxScore--;
            }
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            colorChange = -1;
            inRange = -1;
            canny = -1;
            blur = -1;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            scalars[0].Val0 = trackBar1.Value;
            label1.Text = trackBar1.Value.ToString();
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            scalars[1].Val0 = trackBar2.Value;
            label2.Text = trackBar2.Value.ToString();
        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {
            scalars[0].Val1 = trackBar3.Value;
            label3.Text = trackBar3.Value.ToString();
        }

        private void trackBar4_Scroll(object sender, EventArgs e)
        {
            scalars[1].Val1 = trackBar4.Value;
            label4.Text = trackBar4.Value.ToString();
        }

        private void trackBar5_Scroll(object sender, EventArgs e)
        {
            scalars[0].Val2 = trackBar5.Value;
            label5.Text = trackBar5.Value.ToString();
        }

        private void trackBar6_Scroll(object sender, EventArgs e)
        {
            scalars[1].Val2 = trackBar6.Value;
            label6.Text = trackBar6.Value.ToString();
        }

        private void trackBar7_Scroll(object sender, EventArgs e)
        {
            cannyValues[0] = trackBar7.Value;
            label11.Text = trackBar7.Value.ToString();
        }

        private void trackBar8_Scroll(object sender, EventArgs e)
        {
            cannyValues[1] = trackBar8.Value;
            label12.Text = trackBar8.Value.ToString();
        }
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox3.Checked)
            {
                canny = ++maxScore;
            }
            else
            {
                canny = -1;
                maxScore--;
            }
        }

        private void trackBar9_Scroll(object sender, EventArgs e)
        {
            if (trackBar9.Value == 4) trackBar9.Value = 3;
            else if (trackBar9.Value == 6) trackBar9.Value = 7;
            cannyValues[2] = trackBar9.Value;
            label13.Text = trackBar9.Value.ToString();
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox4.Checked)
            {
                blur = ++maxScore;
            }
            else
            {
                blur = -1;
                maxScore--;
            }
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedBlur = (string)comboBox2.SelectedItem;
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    panel4.Visible = true;
                    panel5.Visible = false;
                    panel6.Visible = false;
                    panel7.Visible = false;
                    break;
                case 1:
                    panel4.Visible = false;
                    panel5.Visible = true;
                    panel6.Visible = false;
                    panel7.Visible = false;
                    break;
                case 2:
                    panel4.Visible = false;
                    panel5.Visible = false;
                    panel6.Visible = true;
                    panel7.Visible = false;
                    break;
                case 3:
                    panel4.Visible = false;
                    panel5.Visible = false;
                    panel6.Visible = false;
                    panel7.Visible = true;
                    break;
            }
        }

        private void trackBar10_Scroll(object sender, EventArgs e)
        {
            label15.Text = trackBar10.Value.ToString();
            valueForBlur = trackBar10.Value;
        }

        private void trackBar11_Scroll(object sender, EventArgs e)
        {
            trackBar11.Value = trackBar11.Value % 2 == 0 ? trackBar11.Value - 1 : trackBar11.Value;
            valuesForGauss[0] = trackBar11.Value;
            label16.Text = trackBar11.Value.ToString();
        }

        private void trackBar14_Scroll(object sender, EventArgs e)
        {
            valuesForGauss[1] = trackBar14.Value;
            label21.Text = trackBar14.Value.ToString();
        }

        private void trackBar15_Scroll(object sender, EventArgs e)
        {
            valuesForGauss[2] = trackBar15.Value;
            label22.Text = trackBar15.Value.ToString();
        }

        private void trackBar13_Scroll(object sender, EventArgs e)
        {
            trackBar13.Value = trackBar13.Value % 2 == 0 ? trackBar13.Value - 1 : trackBar13.Value;
            label18.Text = trackBar13.Value.ToString();
            valueForMedian = trackBar13.Value;
        }

        private void trackBar12_Scroll(object sender, EventArgs e)
        {
            valuesForBil[0] = trackBar12.Value;
            label17.Text = trackBar12.Value.ToString();
        }

        private void trackBar16_Scroll(object sender, EventArgs e)
        {
            valuesForBil[1] = trackBar16.Value;
            label26.Text = trackBar16.Value.ToString();
        }

        private void trackBar17_Scroll(object sender, EventArgs e)
        {
            valuesForBil[2] = trackBar17.Value;
            label27.Text = trackBar17.Value.ToString();
        }
    }
}
