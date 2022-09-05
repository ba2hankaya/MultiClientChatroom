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
        //client list(client class is created at the very end of the codes),thread list and a port is defined.
        static List<Client> clients;
        static List<Thread> threads;
        static int port = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void hostButton_Click(object sender, EventArgs e)
        {
            //ip address and port are saved
            IPAddress iPAddress = IPAddress.Parse(ipTextBox.Text.ToString());
            port = Convert.ToInt32(portTextBox.Text);
            
            //thread for listening to clients is started with the given IP and Port
            Thread t = new Thread(() => listenForClient(iPAddress, port));
            t.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //clients list and threads list is initiated
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
                
                //lets the server know. (since the listBox1 object was created in the main thread and not this one,
                // it's methods cannot be called from this thread directly so the invoke(methodinvoker(delgate)) is 
                //used)
                
                this.Invoke(new MethodInvoker(delegate ()
                {
                    listBox1.Items.Add("Listening...");
                }));
                
                //when a client connects, adds it to the clients list with its variables
                TcpClient tcp_cl = listener.AcceptTcpClient();
                Client temp = new Client(tcp_cl, new StreamReader(tcp_cl.GetStream()), new StreamWriter(tcp_cl.GetStream()),false);
                temp.p_stwr.AutoFlush = true;
                clients.Add(temp);
                
                //adds the ip address of the client to the listbox
                this.Invoke(new MethodInvoker(delegate ()
                {
                    listBox1.Items.Add("Connection from " + tcp_cl.Client.RemoteEndPoint.ToString());
                }));
                
                

                //opens a thread for listening that client, adds it to a list and starts it
                Thread t = new Thread(() => Listen(clients[clients.Count - 1]));
                threads.Add(t);
                threads[threads.Count - 1].Start();
                
                //adds 1 to the port for the next client. I don't know if multiple clients can be communicated via a
                //single port so I have used this primitive solution to avoid redoing the method all over
                port++;
            }
        }
        void Broadcast(string Message)
        {
            //sends every client the message entered if they are logged in
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
            
            while (true)
            {
                
                if (listenTo.loggedIn)
                {
                    //if the user is logged in, the message sent to the server by that client is sent to every client
                    //connected to the server that are logged in besides the client that is listened to, because the code
                    //from the clientside exe already adds the sender's message to their listbox
                    string datain = listenTo.p_strd.ReadLine();
                    foreach (Client client in clients)
                    {
                        if (client != listenTo && client.loggedIn)
                        {
                            client.p_stwr.WriteLine(datain);
                        }

                    }                    
                    this.Invoke(new MethodInvoker(delegate () //adds the message to the server's listbox as well
                    {
                        listBox1.Items.Add(datain);
                    }));
                    
                }
                else
                {
                    //if they are not logged in, the received data is treated as login information
                    string datain = listenTo.p_strd.ReadLine();
                    
                    if (userInfo(datain)[0] == "user1" && userInfo(datain)[1] == "123456")
                    {
                        //if the client enters with a correct username and password, the said Client object's loggedIn
                        //value is changed so they can receive messages and their input is not treated as login info anymore
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
                        //this is needed to close the loginVer thread in the client side, because there is a
                        //while(listbox count == count(stored listbox.count before awaiting response))
                        listenTo.p_stwr.WriteLine("Login Failed.");
                    }
                }
                
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //accidentally clicked on the listbox lol
            
        }

        public string[] userInfo(string input)
        {
            return input.Split(':'); //totally unnecessary function because it is a one liner and can be done just before
            //the userinfo part but really felt like doing it
        }

        private void sendButton_Click(object sender, EventArgs e)
        {
            //broadcasts the message sent
            Broadcast(messageTextBox.Text.ToString());
            
            //adds the message to the server's listbox as well(unnecessary invoke method since this is the main thread)
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
    //client class for creating a database(array) for clients that are connected for easier and neater code
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
