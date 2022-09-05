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
        //defines stream reader,tcpclient, a bool for logging in, string for username, and streamwriter
        static StreamReader s_rdr;
        static TcpClient client;
        static bool loggedIn;
        static string username;
        static StreamWriter s_wrt;
        private void button1_Click(object sender, EventArgs e)
        {
        
            //gets the ip and the port of the server to connect to
            string targetIP = serverIPTextBox.Text;
            int port = Convert.ToInt32(serverPortTextBox.Text);
            
            //connects to it and sets the stream values
            client = new TcpClient(targetIP, port);
            s_rdr = new StreamReader(client.GetStream());
            s_wrt = new StreamWriter(client.GetStream());
            s_wrt.AutoFlush = true;
            //listbox update(the invoke, method invoker, delegate is needed when trying to add an item to a form object created in another thread, since this is the main thread it is not needed)
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.Items.Add("Connected!");
            }));
            //will be needed in the thread below (Listen) because it is not the main thread
            //listen thread is created to listen to the data sent from the server without interruptions
            Thread t = new Thread(new ThreadStart(Listen));
            t.Start();
            
            //the form tools that are needed to login are made visible to the user once the connection is successful
            label2.Visible = true;
            label1.Visible = true;
            usernameTextBox.Visible = true;
            passwordTextBox.Visible = true;
            button2.Visible = true;
        }

        void Listen()
        {
            //adds all the data received to the listbox
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
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
        //I made a seperate thread for verifying users since the verification needs to be on a loop while waiting for
        //server response to the login attempt, this cannot be done on the main thread because it stops the Listen 
        //thread and therefore makes it impossible to receive the server response resulting in an infinite loop
            Thread a = new Thread(new ThreadStart(LoginVer));
            a.Start();
            
        }

        public void LoginVer()
        {
            //the format username and password are sent for the server to process it correctly. This is an obvious vuln
            //because textboxes don't have char filter restricting the user from entering :, it doesn't necessarily
            //break the program but it is there(btw even if the textboxes had char filter, users can always edit the 
            //package before it is sent, getting around the filter)
            s_wrt.WriteLine(usernameTextBox.Text + ":" + passwordTextBox.Text);
            //server response updates the listbox count, so the app goes into a loop waiting for it to change
            int count = listBox1.Items.Count;
            while (count == listBox1.Items.Count)
            {

            }
            //if the last message from the server received is login successful, the user's send message button and
            //message input textbox appears
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
        }
        
        private void sendButton_Click(object sender, EventArgs e)
        {
            //the username saved in the loginver is added in front of every message sent by the client
            s_wrt.WriteLine(username + " : " + messageTextBox.Text);
            
            //every message sent is seenable by the user themselves as well.
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.Items.Add(username + " : " + messageTextBox.Text);
                messageTextBox.Clear();
            }));

        }
    }
}
