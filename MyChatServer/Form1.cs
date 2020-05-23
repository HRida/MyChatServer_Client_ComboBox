using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;   //
using System.IO;            //
using System.Threading;     //

namespace MyChatServer
{
    public partial class Form1 : Form
    {
        private NetworkStream socketStream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private Socket connection;
        private Thread readThread;
        public Form1()
        {
            InitializeComponent();
            // create a new thread from the server
            readThread = new Thread(new ThreadStart(RunServer));
            readThread.Start();
        }

        private void inputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // sends the text to the client
            try
            {
                if (e.KeyCode == Keys.Enter && connection != null)
                {
                    writer.Write("SERVER>>> " + inputTextBox.Text);
                    Action updateLabel = () => displayTextBox.Text += "\r\nSERVER>>> " + inputTextBox.Text;
                    displayTextBox.Invoke(updateLabel);
 
                    // if the user at the server signaled termination sever the connection to the client
                    if (inputTextBox.Text == "TERMINATE")
                        connection.Close();

                    inputTextBox.Clear();
                }
            }
            catch (SocketException)
            {
                Action updateLabel = () => displayTextBox.Text += "\nError writing object";
                displayTextBox.Invoke(updateLabel);
            }
        }
        // allows a client to connect and displays the text it sends
        public void RunServer()
        {
            TcpListener listener;
            int counter = 1;

            // wait for a client connection and display the text
            // that the client sends
            try
            {
                // Step 1: create TcpListener

                listener = new TcpListener(5001);

                // Step 2: TcpListener waits for connection request
                listener.Start();

                // Step 3: establish connection upon client request
                while (true)
                {
                    Action updateLabel = () => displayTextBox.Text += "Waiting for connection\r\n";
                    displayTextBox.Invoke(updateLabel);
                    // accept an incoming connection
                    connection = listener.AcceptSocket();

                    // create NetworkStream object associated with socket
                    socketStream = new NetworkStream(connection);

                    // create objects for transferring data across stream
                    writer = new BinaryWriter(socketStream);
                    reader = new BinaryReader(socketStream);
                    string connect = "";
                    for (int i = 0; i < comboBox1.Items.Count; i++)
                    {
                        connect += comboBox1.Items[i].ToString() + " ";
                    }
                    writer.Write(connect);
                    writer.Write("Connection successful");
                    updateLabel = () => displayTextBox.Text += "Connection " + counter + " received.\r\n";
                    displayTextBox.Invoke(updateLabel);
                    // inform client that connection was successfull
                    writer.Write("SERVER>>> Connection successful");

                    inputTextBox.ReadOnly = false;
                    string theReply = "";

                    // Step 4: read String data sent from client
                    do
                    {
                        try
                        {
                            // read the string sent to the server
                            theReply = reader.ReadString();
                            updateLabel = () => displayTextBox.Text += "\r\n" + theReply;
                            displayTextBox.Invoke(updateLabel);
                        }
                        catch (Exception)
                        {
                            break;
                        }

                    } while (theReply != "CLIENT>>> TERMINATE" &&
                       connection.Connected);
                    SetText("\r\nUser terminated connection");

                    // Step 5: close connection
                    writer.Close();
                    reader.Close();
                    socketStream.Close();
                    connection.Close();

                    ++counter;
                }
            } // end try

            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
            }

        } // end method RunServer
        delegate void SetTextCallback(string text);
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.displayTextBox.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.displayTextBox.Text += text;
            }
        }
    }
}
