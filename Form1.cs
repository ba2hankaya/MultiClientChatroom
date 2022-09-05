using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientSide
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        static StreamReader s_rdr;
        static TcpClient client;
        static bool loggedIn;
        static string username;
        static StreamWriter s_wrt;
        private void button1_Click(object sender, EventArgs e)
        {
            string targetIP = serverIPTextBox.Text;
            int port = Convert.ToInt32(serverPortTextBox.Text);
            client = new TcpClient(targetIP, port);
            s_rdr = new StreamReader(client.GetStream());
            s_wrt = new StreamWriter(client.GetStream());
            s_wrt.AutoFlush = true;
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.Items.Add("Connected!");
            }));
            Thread t = new Thread(new ThreadStart(Listen));
            t.Start();
            label2.Visible = true;
            label1.Visible = true;
            usernameTextBox.Visible = true;
            passwordTextBox.Visible = true;
            button2.Visible = true;
        }

        void Listen()
        {
            //prints whatever is received to the console
            while (client.Connected)
            {
                try
                {
                    string datain = s_rdr.ReadLine();
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        listBox1.Items.Add(datain);
                    }));
                }
                catch (Exception ex) { }

            }
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.Items.Add("Disconnected!");
            }));
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Thread a = new Thread(new ThreadStart(LoginVer));
            a.Start();
        }

        public void LoginVer()
        {
            s_wrt.WriteLine(usernameTextBox.Text + ":" + passwordTextBox.Text);
            int count = listBox1.Items.Count;
            while (count == listBox1.Items.Count)
            {

            }
            if (listBox1.Items[listBox1.Items.Count - 1].ToString() == "Login Successful!")
            {
                loggedIn = true;
                this.Invoke(new MethodInvoker(delegate ()
                {
                    messageTextBox.Visible = true;
                    sendButton.Visible = true;
                    username = usernameTextBox.Text;
                    usernameTextBox.Clear();
                    passwordTextBox.Clear();
                }));
                
            }
            count++;
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            s_wrt.WriteLine(username + " : " + messageTextBox.Text);
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.Items.Add(username + " : " + messageTextBox.Text);
                messageTextBox.Clear();
            }));

        }
    }
}
