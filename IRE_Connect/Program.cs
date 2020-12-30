using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace IRE_Connect
{
    class Program
    {
        static int retry = 0;
        TcpClient client;
        Int32 port = 5060;
       static Socket clientSocket;
        static void Main(string[] args)
        {
            // Connect("10.206.100.230", "Action:Login\r\nUsername: asterisk\r\nSecret: asterisk\r\nActionID: 1\r\n\r\n");
            authenticate("192.168.67.215");
        }

       static void authenticate(String server)
        {
            utility _utility = new utility();
            bool socketException = false;
            Connect:
            Console.WriteLine("Connecting to AMI session: " + retry +"\n");
            _utility.notify("Connecting to AMI host - 192.168.67.215", "Attempting to re-establish connection to AMI host \n\nTime:" + DateTime.Now.ToString());

            // Connect to the asterisk server.
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint myEndPoint = new IPEndPoint(IPAddress.Any, 9901);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Parse(server), 5038);
           
            try {

                if (!socketException)
                {
                    serverSocket.Bind(myEndPoint);
                    serverSocket.Listen(4);
                }
             
                clientSocket.Connect(serverEndPoint);

                // Login to the server; manager.conf needs to be setup with matching credentials.
                clientSocket.Send(Encoding.ASCII.GetBytes("Action:Login\r\nUsername: atladmin\r\nSecret: 7mmT@XAy\r\nActionID: 5\r\n\r\n"));

                if (clientSocket.Poll(100000, SelectMode.SelectError))
                        {
                    Console.WriteLine("poll interval");
                    if (!clientSocket.Connected) {
                        // Something bad has happened, shut down
                        Console.WriteLine("disconnected");
                    }
                    else {
                        // There is data waiting to be read"
                    }
                        }

                int bytesRead,bytes = 0;

                do
                {
                    byte[] buffer = new byte[10024];
                    byte[] buffer2 = new byte[10024];
                    bytesRead = clientSocket.Receive(buffer);

                    string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                    if (!socketException)
                    serverSocket.BeginAccept(new AsyncCallback(AcceptCallBack), serverSocket);
                  //  bytes=socketAccept.Receive(buffer2);

                    string responseData = Encoding.ASCII.GetString(buffer2, 0, bytes);
                    Console.WriteLine(responseData);

                      String[] pars = response.Split(new string[] {"\r\n\r\n"},StringSplitOptions.None);
                    if (response.IndexOf("\r\n\r\n") > -1)
                    {
                        Console.WriteLine(response);
                        Task task = new Task(() => _utility.consumeResponse(pars));
                        task.Start();
                    }
                    if (Regex.Match(response, "Message: Authentication accepted", RegexOptions.IgnoreCase).Success)
                    {
                        Console.Write("Login Successfull");
                        _utility.notify("Connection to AMI host sucessfull - 192.168.67.215", "Alert: Successfully connected to AMI host \n\nTime:" + DateTime.Now.ToString());
                    }

                    //Let's get pretty parsing and checking events

                

                } while (bytesRead != 0);

                Console.WriteLine("Connection to server lost.");
                _utility.notify("AMI Connection Lost - 192.168.67.215", "Alert: Connection to server lost \n\nTime:" + DateTime.Now.ToString());
              //  _utility._sqlcon.Dispose();
                // serverSocket.Shutdown(SocketShutdown.Both);
                // serverSocket.Disconnect(true);
                socketException = true;
             //   retry=retry+1;
                goto Connect;
                //Console.ReadLine();

            }
            catch (SocketException ex)
            {
                // _utility._sqlcon.Dispose();
                _utility.notify("AMI Connection Lost - 192.168.67.215", "Alert: " + ex.Message.ToString() + "\n\nTime:" + DateTime.Now.ToString());
               goto Connect;
            }
        }
    
    private static void AcceptCallBack(IAsyncResult ar)
        {
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            byte[] buffer = new byte[10024];
            byte[] buffer2 = new byte[10024];

            int bytesRead = handler.Receive(buffer);

            string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);
            Console.WriteLine("SAID: " + response);
            clientSocket.Send(Encoding.ASCII.GetBytes(response));
        }


       static void Connect(String server, String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer 
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = 5038;
                TcpClient client = new TcpClient(server, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer. 
                stream.Write(data, 0, data.Length);

                Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

            Console.WriteLine("\n Press Enter to continue...");
            Console.Read();
        }
    }
}
