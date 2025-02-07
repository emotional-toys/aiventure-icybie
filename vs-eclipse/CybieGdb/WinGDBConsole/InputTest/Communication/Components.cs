using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AccessGdb
{
    /// <summary>
    /// Software components for the transmissions.
    /// </summary>
    public class Components
    {
        private const int PortNum = 15200;

        public static string OpenUdpConnection(string message) // What is the sequence to reach the IP?
        {
            var tcp = new TcpClient();
            var udp = new UdpClient();
            const int groupPort = 15200;
            var groupEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), groupPort);

            //string str4 = "Is anyone out there?";
            //var openTCP = OpenTcpPort(tcp); // Going to here will place a connection in the SimServer!
            var openGdb = OpenGdbPort(tcp);

            // What kind of dialogue does SimServer need?
            // gdb IP is 127.0.0.1:64270

            byte[] sendBytes4 = Encoding.ASCII.GetBytes(message);

            // What is next to communicate?
            udp.Send(sendBytes4, sendBytes4.Length, groupEp);

            byte[] receiveBytes = udp.Receive(ref groupEp);

            string returnData = Encoding.ASCII.GetString(receiveBytes);

            udp.Close();
            var closeTcp = CloseTcpPort(tcp);

            return returnData;
        }

        public static string OpenGdbPort(TcpClient client)
        {
            var message = "";

            int groupPort = 64270;
            var groupEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), groupPort);

            //string str4 = "Is anyone out there?";

            //byte[] sendBytes4 = Encoding.ASCII.GetBytes(message);

            client.Connect(groupEP); // .Send(sendBytes4, sendBytes4.Length, groupEP);

            if (client.Connected)
                message = "TCP connected.";

            return message;
        }

        public static string OpenTcpPort(TcpClient client)
        {
            var message = "";

            int groupPort = 15200;
            var groupEP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), groupPort);

            //string str4 = "Is anyone out there?";

            //byte[] sendBytes4 = Encoding.ASCII.GetBytes(message);

            client.Connect(groupEP); // .Send(sendBytes4, sendBytes4.Length, groupEP);

            if (client.Connected)
                message = "TCP connected.";

            //string openUdpPort = OpenUdpPort(message);

            //string returnData = Encoding.ASCII.GetString(tcp.);

            //Log(Loglevels.Detail, "Response: " + returnData);

            return message;
        }

        public static string CloseTcpPort(TcpClient client)
        {
            var message = "";
            client.Close();

            if (!client.Connected)
                message = "TCP disconnected.";

            return message;
        }

        public static string TcpListen(string message)
        {
            //message = "help breakpoint";
            var tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect("127.0.0.1", PortNum);
                NetworkStream networkStream = tcpClient.GetStream();

                if (networkStream.CanWrite && networkStream.CanRead)
                {
                    while (message != "quit")
                    {
                        byte[] sendBytes = Encoding.ASCII.GetBytes(message);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);

                        // Reads the NetworkStream into a byte buffer.
                        byte[] bytes = new byte[tcpClient.ReceiveBufferSize];
                        var BytesRead = networkStream.Read(bytes, 0, (int) tcpClient.ReceiveBufferSize);

                        // Returns the data received from the host to the console.
                        string returndata = Encoding.ASCII.GetString(bytes, 0, BytesRead);
                    }

                    networkStream.Close();
                    tcpClient.Close();

                }
                else if (!networkStream.CanRead)
                {
                    message = "You can not write data to this stream.";
                    tcpClient.Close();
                }
                else if (!networkStream.CanWrite)
                {
                    message = "You can not read data from this stream.";
                    tcpClient.Close();
                }
            }
            catch (SocketException se)
            {
                return se.Message.ToString(CultureInfo.InvariantCulture);
            }
            catch (System.IO.IOException ioe)
            {
                return ioe.Message.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return message;
        }
    }
}
