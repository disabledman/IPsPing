using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IPsPing
{
    public partial class Form1 : Form
    {
        private ManualResetEvent m_WaitObj = new ManualResetEvent(true);
        private Queue<string> m_Logs = new Queue<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count == 0) return;
            if (numericUpDown1.Value <= 0) return;

            timer1.Interval = Int32.Parse(numericUpDown1.Value.ToString()) * 1000;
            timer1.Enabled = true;
            Log("Start!");
        }

        //
        private void Log(string strString)
        {
            lock(m_Logs)
            {
                string str = $"{DateTime.Now.ToString()}, {strString}";

                m_Logs.Enqueue(str);

                File.AppendAllText("log.txt", $"{str}\r\n");
            }
        }

        //
        private void Worker()
        {
            try
            {
                Ping pingSender = new Ping();

                foreach(string ip in listBox1.Items)
                {
                    PingReply reply = pingSender.Send(ip, 120);//第一個引數為ip地址,第二個引數為ping的時間

                    if (reply.Status == IPStatus.Success)
                    {
                        Log($"{ip}, OK");
                    }
                    else
                    {
                        Log($"{ip}, Error");
                    }
                }

                pingSender = null;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                m_WaitObj.Set();
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            numericUpDown1.Value = 5;

            listBox1.Items.Clear();
            timer2.Start();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add(textBox1.Text);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;

            m_WaitObj.Reset();
            Task T = Task.Factory.StartNew(() =>
            {
                Worker();
            });
            m_WaitObj.WaitOne();

            timer1.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Log("Stop!");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timer2.Stop();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Enabled = false;

            textBox2.Invoke(new Action(() =>
            {
                lock (m_Logs)
                {
                    if (m_Logs.Count > 0)
                    {
                        textBox2.Text += $"{m_Logs.Dequeue()}\r\n";
                    }
                }
            }));

            timer2.Enabled = true;
        }
    }
}
