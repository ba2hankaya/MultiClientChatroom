using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace ServerSideForms
{
    public partial class Form1 : Form
    {
        static List<Client> clients;
        static List<Thread> threads;
        static int port = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void hostButton_Click(object sender, EventArgs e)
        {
            IPAddress iPAddress = IPAddress.Parse(ipTextBox.Text.ToString());
            port = Convert.ToInt32(portTextBox.Text);
            Thread t = new Thread(() => listenForClient(iPAddress, port));
            t.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            clients = new List<Client>();
            threads = new List<Thread>();
        }

        public void listenForClient(IPAddress iPAddress, int port)
        {
            while (true)
            {


                //starts a listener
                TcpListener listener = new TcpListener(iPAddress, port);
                listener.Start();
                this.Invoke(new MethodInvoker(delegate ()
                {
                    listBox1.Items.Add("Listening...");
                }));
                
                //when a client connects, adds it to a list
                TcpClient tcp_cl = listener.AcceptTcpClient();
                Client temp = new Client(tcp_cl, new StreamReader(tcp_cl.GetStream()), new StreamWriter(tcp_cl.GetStream()),false);
                temp.p_stwr.AutoFlush = true;
                this.Invoke(new MethodInvoker(delegate ()
                {
                    listBox1.Items.Add("Connection from " + tcp_cl.Client.RemoteEndPoint.ToString());
                }));
                
                clients.Add(temp);

                //opens a thread for listening that client, adds it to a list and starts it
                Thread t = new Thread(() => Listen(clients[clients.Count - 1]));
                threads.Add(t);
                threads[threads.Count - 1].Start();
                port++;
            }
        }
        void Broadcast(string Message)
        {
            //sends every client the message entered
            foreach (Client client in clients)
            {
                if (client.loggedIn)
                {
                    client.p_stwr.WriteLine("Server: " + Message);
                }
            }
        }

        void Listen(Client listenTo)
        {
            //prints the data received from a client
            while (true)
            {
                
                if (listenTo.loggedIn)
                {
                    string datain = listenTo.p_strd.ReadLine();
                    foreach (Client client in clients)
                    {
                        if (client != listenTo && client.loggedIn)
                        {
                            client.p_stwr.WriteLine(datain);
                        }

                    }
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        listBox1.Items.Add(datain);
                    }));
                    
                }
                else
                {
                    string datain = listenTo.p_strd.ReadLine();
                    
                    if (userInfo(datain)[0] == "user1" && userInfo(datain)[1] == "123456")
                    {
                        listenTo.loggedIn = true;
                        listenTo.p_stwr.WriteLine("Login Successful!");
                    }
                    else if (userInfo(datain)[0] == "user2" && userInfo(datain)[1] == "abcdefg")
                    {
                        listenTo.loggedIn = true;
                        listenTo.p_stwr.WriteLine("Login Successful!");
                    }
                    else
                    {
                        listenTo.p_stwr.WriteLine("Login Failed.");
                    }
                }
                
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public string[] userInfo(string input)
        {
            return input.Split(':');
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            Broadcast(messageTextBox.Text.ToString());
            this.Invoke(new MethodInvoker(delegate ()
            {
                listBox1.Items.Add("Server: " + messageTextBox.Text.ToString());
                messageTextBox.Clear();
            }));
        }
    }
}

class Client
{
    public TcpClient p_cl;
    public StreamReader p_strd;
    public StreamWriter p_stwr;
    public bool loggedIn;
    public Client(TcpClient cl, StreamReader strd, StreamWriter stwr, bool loggedIn)
    {
        p_cl = cl;
        p_strd = strd;
        p_stwr = stwr;
        this.loggedIn = loggedIn;
    }
}
