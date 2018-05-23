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

namespace Graphics
{
    public partial class Form1 : Form
    {
        string[] ports;
        string portname = "";
        SerialPort sp = null;

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
            string indata = Encoding.Default.GetString(buf, 0, buf.Length);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (sp.IsOpen)
            {
                sp.Write("1");
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
                textBox1.Enabled = false;
                textBox2.Enabled = false;
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
                button2.Enabled = true;
                textBox1.Enabled = true;
                textBox2.Enabled = true;
                button3.Enabled = false;
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
            }
        }
    }
}
