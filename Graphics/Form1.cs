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
using System.Collections;

namespace Graphics_test
{
    public partial class Form1 : Form
    {
        string[] ports;
        string portname = "";
        SerialPort sp = null;
        public volatile int[] indata = null;
        public volatile bool stop = false;
        public Bitmap myBitmap = null;
        public Graphics graphicsObj = null;
        public int length = 256;
        public volatile int[][] bmp = null;
        public int width, height;
        public bool received = false;
        public List<int> receivebuf = null;
        public int cnt = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp1 = (SerialPort)sender;
            int len = sp1.BytesToRead;
            byte[] buf = new byte[len];
            sp1.Read(buf, 0, len);
            if (!(receivebuf == null))
            {
                for (int i = 0; i < len - 1; i += 2)
                {
                    //receivebuf.Add((buf[i] | ((buf[i + 1] & 15) << 8))/16);
                    //receivebuf.Add((buf[i] | ((buf[i + 1] & 15) << 8))/16);
                    receivebuf.Add(Math.Abs(256 - (buf[i] | ((buf[i + 1] & 15) << 8)) / 16));
                    receivebuf.Add(Math.Abs(256 - (buf[i] | ((buf[i + 1] & 15) << 8)) / 16));
                }
            }
            if (receivebuf.Count >= 256)
            {
                received = true;
                indata = new int[length];
                receivebuf.CopyTo(0, indata, 0, length);
                //string str = "";
                //for (int i = 0; i < length; i++)
                //    str += indata[i].ToString() + " ";
                //MessageBox.Show(str);
                receivebuf = new List<int>();
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
            stop = false;
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
                myBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                graphicsObj = Graphics.FromImage(myBitmap);
                receivebuf = new List<int>();
                if (sp.IsOpen)
                {
                    timer1.Enabled = true;
                    timer1.Start();
                }
            }
            catch (Exception ex)
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
            width = panel1.Width;
            height = panel1.Height;
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.DoubleBuffer |
              ControlStyles.UserPaint |
              ControlStyles.AllPaintingInWmPaint,
              true);
            this.UpdateStyles();
        }
        private void ProcessUsingLockbitsAndUnsafe(Bitmap processedBitmap)
        {
            unsafe
            {
                BitmapData bitmapData = processedBitmap.LockBits(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), ImageLockMode.ReadWrite, processedBitmap.PixelFormat);
                int bytesPerPixel = Bitmap.GetPixelFormatSize(processedBitmap.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int alpha = bmp[x/4][y] == -1 ? 0 : 255;
                        int color = bmp[x/4][y] == -1 ? 0 : bmp[x/4][y];
                        currentLine[x] = (byte)color; //blue
                        currentLine[x + 1] = (byte)color; //green
                        currentLine[x + 2] = (byte)color; //red
                        currentLine[x + 3] = (byte)alpha; //alpha
                    }
                }
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
                receivebuf = new List<int>();
                sp = new SerialPort(portname, 2000000);
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
            timer1.Stop();
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
            receivebuf = new List<int>();
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
                        g = Convert.ToInt32(textBox2.Text);
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
                        g = Convert.ToInt32(textBox3.Text);
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
            sp.Write("MEAS\r");
            Application.DoEvents();
            if (received)
            {
                received = false;
                
                //MessageBox.Show(str);

                int[][] demo = new int[bmp.Length][];
                if (!((indata == null) || (indata.Length == 0)))
                {
                    Array.Copy(bmp, 0, demo, 1, bmp.Length - 1);
                    demo[0] = indata;
                    bmp = demo;
                }
                //if (bmp[128][length - 1] != -1)
                //{
                //    string[] lines = new string[128];
                //    string str = "";
                //    for (int j = 0; j < 128; j++)
                //    {
                //        for (int i = 0; i < length; i++)
                //            str += indata[i].ToString() + " ";
                //        str += "\n";
                //        lines[j] = str;
                //    }
                //    System.IO.File.WriteAllLines(@"D:\test.txt", lines);
                //    MessageBox.Show("i wrote file");
                //    timer1.Stop();
                //}
                this.Invalidate();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
            if (!((bmp == null) || (myBitmap == null) || stop))
            {
                ProcessUsingLockbitsAndUnsafe(myBitmap);
                e.Graphics.DrawImage(myBitmap, panel1.Location.X, panel1.Location.Y, width, height);
                
            }
        }
    }
}
