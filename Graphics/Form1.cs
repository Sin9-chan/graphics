using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace Graphics_test
{
    public partial class Form1 : Form
    {
        string[] ports;
        string portname = "";
        SerialPort sp = null;
        public static int[] indata = null;
        public static bool stop = false;
        public Bitmap myBitmap = null;
        public Graphics graphicsObj = null;
        public int length = 128;
        public int[][] bmp = null;
        public int width, height;

        public Form1()
        {
            InitializeComponent();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp1 = (SerialPort)sender;
            length = sp1.BytesToRead;
            byte[] buf = new byte[length];
            sp1.Read(buf, 0, length);
            indata = new int[length];
            for (int i = 0; i < length; i++)
            {
                indata[i] = (int)buf[i]/16;
            }
        }
        private void TestDraw(int[][] b)
        {
            int x = 0, y = 0;
            for (int i = 0; i < b.Length; i++)
            {
                for (int j = 0; j < b[i].Length; j++)
                {

                    if (!(b[i][j] <= -1))
                        myBitmap.SetPixel(x + i, y + j, Color.FromArgb(255, b[i][j], b[i][j], b[i][j]));
                    else
                        myBitmap.SetPixel(x + i, y + j, Color.FromArgb(0, 0, 0, 0));
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button2.Enabled = true;
            textBox3.Enabled = false;
            button8.Enabled = false;
            try
            {
                bmp = new int[width][];
                for (int i = 0; i < width; i++)
                {
                    bmp[i] = new int[length];
                    for (int j = 0; j < length; j++)
                    {
                        bmp[i][j] = -1;
                    }
                }
                myBitmap = new Bitmap(width, height);
                graphicsObj = Graphics.FromImage(myBitmap);
                timer1.Enabled = true;
                /*if (sp.IsOpen)
                {
                    do
                    {
                        sp.Write("MEAS\r");
                        Thread.Sleep(timer1.Interval);
                    }
                    while (!stop);
                }*/
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ports = SerialPort.GetPortNames();
            if (ports.Length != 0)
            {
                comboBox1.DataSource = ports;
                comboBox1.SelectedItem = ports[0];
                button3.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                textBox3.Enabled = false;
                button8.Enabled = false;
            }
            button1.Enabled = true;
            width = 300;
            height = panel1.Height;
        }
        private void ProcessUsingLockbitsAndUnsafeAndParallel(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);

                int bytesPerPixel = Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* PtrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    byte* currentLine = PtrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];

                        currentLine[x] = (byte)oldBlue;
                        currentLine[x + 1] = (byte)oldGreen;
                        currentLine[x + 2] = (byte)oldRed;
                    }
                });
                processedBitmap.UnlockBits(bitmapData);
            }
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            portname = comboBox1.SelectedItem.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                sp = new SerialPort(portname);
                sp.Open();
                sp.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                button4.Enabled = true;
                button1.Enabled = true;
                button2.Enabled = false;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                button3.Enabled = false;
                button5.Enabled = false;
                button6.Enabled = true;
                button7.Enabled = true;
                textBox3.Enabled = true;
                button8.Enabled = true;
            }
            catch
            {
                MessageBox.Show("Serial port is already open.");
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                button5.Enabled = true;
                textBox3.Enabled = false;
                button8.Enabled = false;
                if (!(sp == null))
                {
                    if (sp.IsOpen) sp.Close();
                    sp.Dispose();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ports = SerialPort.GetPortNames();
            if (ports.Length != 0)
            {
                comboBox1.DataSource = ports;
                comboBox1.SelectedItem = ports[0];
                button3.Enabled = true;
            }
            else
            {
                MessageBox.Show("No active serial ports.");
                comboBox1.Enabled = false;
                button1.Enabled = false;
                button2.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
                textBox1.Enabled = false;
                textBox2.Enabled = false;
                button6.Enabled = false;
                button7.Enabled = false;
                textBox3.Enabled = false;
                button8.Enabled = false;
            }
        }
        

        private void button2_Click(object sender, EventArgs e)
        {
            stop = true;
            timer1.Enabled = false;
            button1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = true;
            button5.Enabled = false;
            button6.Enabled = true;
            button7.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button2.Enabled = false;
            textBox3.Enabled = true;
            button8.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!(sp == null))
            {
                if (sp.IsOpen) sp.Close();
                sp.Dispose();
            }
            button3.Enabled = true;
            button5.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            textBox3.Enabled = false;
            button8.Enabled = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (sp.IsOpen)
                {
                    int g = -1;
                    try
                    {
                        g = Convert.ToInt32(textBox1.Text);
                    }
                    catch { }
                    if (!((g == -1) || (g > 1000)))
                    {
                        sp.Write("GAIN " + g.ToString() + "\r");
                    }
                    else { MessageBox.Show("Expects decimal number from 0 to 1000."); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (sp.IsOpen)
                {
                    int g = -1;
                    try
                    {
                        g = Convert.ToInt32(textBox1.Text);
                    }
                    catch { }
                    if (!((g == -1) || (g > 1000 || g < 90)))
                    {
                        sp.Write("TACT " + g.ToString() + "\r");
                    }
                    else { MessageBox.Show("Expects decimal number from 90 to 1000."); }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                int g = -1;
                    try
                    {
                        g = Convert.ToInt32(textBox1.Text);
                    }
                    catch { }
                    if (!((g == -1) || (g > 1000 || g < 10)))
                    {
                        timer1.Interval = g;
                    }
                    else { MessageBox.Show("Expects decimal number from 10 to 1000."); }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Random r = new Random();
            indata = new int[bmp[0].Length];
            for (int i = 0; i < length; i++)
            {
                indata[i] = r.Next(255);
            }

            int[][] demo = new int[bmp.Length][];
            if (!((indata == null) || (indata.Length == 0)))
            {
                Array.Copy(bmp, 0, demo, 1, bmp.Length - 1);
                demo[0] = indata;
                bmp = demo;
            }
            this.Invalidate();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
            if (!((bmp == null) || (myBitmap == null) || stop))
            {
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                e.Graphics.CompositingMode = CompositingMode.SourceOver;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.SmoothingMode = SmoothingMode.None;
                TestDraw(bmp);
                e.Graphics.DrawImage(myBitmap, panel1.Location.X, panel1.Location.Y, width, height);
                //sw.Stop();
                //MessageBox.Show(sw.Elapsed.TotalMilliseconds.ToString());
            }
        }
    }
}
