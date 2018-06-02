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
        public ArrayList receivebuf = null;

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
                    receivebuf.Add((buf[i] | buf[i + 1] << 8));
                }
            }
            if (receivebuf.Count >= 127)
                received = true;

            //string str = "";
            //for (int i = 0; i < receivebuf.Count; i++)
            //    str += receivebuf[i].ToString() + " ";
            //MessageBox.Show(receivebuf.Count.ToString() + " bytes " + str);
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
            //try
           // {
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
                
                if (sp.IsOpen)
                {
                    timer1.Enabled = true;
                    timer1.Start();
                //do
                //{

                //if (indata==null)
                //    MessageBox.Show("null");
                
                //Thread.Sleep(timer1.Interval);
                //}
                //while (!stop);
            }
            //}
            //catch (Exception ex)
           // {
               // MessageBox.Show(ex.Message);
           // }
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
            indata = new int[length];
            receivebuf = new ArrayList();
            sp.Write("MEAS\r");
            Application.DoEvents();
            if (received)
            {
                int j = 0;
                for (int i = 0; i <= receivebuf.Count; i++, j += 2)
                {
                    if (i != receivebuf.Count && j < 256)
                    {
                        indata[j] = (int)receivebuf[i];
                        indata[j + 1] = (int)receivebuf[i];
                    }
                    else
                    {
                        for (int q = receivebuf.Count * 2; q < 256; q++)
                        { indata[q] = 0; }
                    }
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
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            
            if (!((bmp == null) || (myBitmap == null) || stop))
            {
                //Stopwatch sw = new Stopwatch();
                //sw.Start();
                //TestDraw(bmp);
                received = false;
                ProcessUsingLockbitsAndUnsafe(myBitmap);
                e.Graphics.DrawImage(myBitmap, panel1.Location.X, panel1.Location.Y, width, height);
                //sw.Stop();
                //MessageBox.Show(sw.Elapsed.TotalMilliseconds.ToString());
            }
        }
    }
}
