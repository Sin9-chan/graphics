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

namespace Graphics_test
{
    public partial class Form1 : Form
    {
        string[] ports;
        string portname = "";
        SerialPort sp = null;
        public static int[] indata = null;
        public static bool stop = false;
        public Bitmap myBitmap;
        public Graphics graphicsObj;
        public int delay = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp1 = (SerialPort)sender;
            int length = sp1.BytesToRead;
            byte[] buf = new byte[length];
            sp1.Read(buf, 0, length);
            indata = new int[length];
            for (int i = 0; i < length; i++)
            {
                indata[i] = (int)buf[i];
            }
        }
        private void TestDraw(int[] b, int x)
        {
            int y = panel1.Location.Y;
            for (int i = 0; i < b.Length; i++)
            {
                if (!(b[i] <= -1))
                    myBitmap.SetPixel(x, y + i, Color.FromArgb(255, b[i], b[i], b[i]));
                else
                    myBitmap.SetPixel(x, y + i, Color.FromArgb(0, 0, 0, 0));
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int x = panel1.Location.X, y = panel1.Location.Y;
            button1.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            button6.Enabled = false;
            button7.Enabled = false;
            textBox1.Enabled = false;
            textBox2.Enabled = false;
            button2.Enabled = true;
            try
            {
                if (sp.IsOpen)
                {
                    do
                    {
                        sp.Write("MEAS\r");
                        if (!((indata == null) || (indata.Length == 0)))
                        {
                            for (int i = 0; i < indata.Length; i++)
                            {
                                myBitmap.SetPixel(x, y, Color.FromArgb(255, indata[i], indata[i], indata[i]));
                                y++;
                            }
                            x++;
                        }

                    }
                    while (!stop);
                }
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
            }
            button2.Enabled = true;
            Random r = new Random();
            int[][] bmp = new int[panel1.Width][];
            for (int i = 0; i < panel1.Width; i++)
            {
                bmp[i] = new int[128];
                for (int j = 0; j < 128; j++)
                {
                    bmp[i][j] = -1;
                }
            }
            int asd = 0;
            myBitmap = new Bitmap(panel1.Width, panel1.Height);
            graphicsObj = Graphics.FromImage(myBitmap);
            while (!stop)
            {
                int[] mass = new int[128];
                string str = "";
                int[][] demo = new int[bmp.Length][];
                for (int i = 0; i < 128; i++)
                {
                    mass[i] = asd;// r.Next(255);
                }

                Array.Copy(bmp, 0, demo, 1, bmp.Length - 1);
                demo[0] = mass;
                bmp = demo;
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < bmp[i].Length; j++)
                    {
                        str += bmp[i][j].ToString()+" ";
                    }
                    str += "\n";
                }
                Thread.Sleep(delay);
                for (int a = 0; a < 10; a++)
                {
                    TestDraw(bmp[a], a + panel1.Location.X);
                    Invalidate();
                }
                asd++;
                /* graphicsObj = this.CreateGraphics();
                Brush brush = new SolidBrush(Color.FromArgb(255,r.Next(255), r.Next(255), r.Next(255)));
                graphicsObj.FillRectangle(brush, r.Next(panel1.Location.X+anel), y, 1, 1);*/
                //MessageBox.Show(str);
            }
            graphicsObj.Dispose();
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
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.DrawImage(myBitmap, 0, 0, myBitmap.Width, myBitmap.Height);
            graphics.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stop = true;
            button1.Enabled = true;
            button3.Enabled = false;
            button4.Enabled = true;
            button5.Enabled = false;
            button6.Enabled = true;
            button7.Enabled = true;
            textBox1.Enabled = true;
            textBox2.Enabled = true;
            button2.Enabled = false;
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
                    if (!((g == -1) || (g > 1000 || g < 0)))
                    {
                        delay = g;
                    }
                    else { MessageBox.Show("Expects decimal number from 0 to 1000."); }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
