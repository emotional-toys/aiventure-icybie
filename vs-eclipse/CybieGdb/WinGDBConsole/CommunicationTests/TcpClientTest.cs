using System;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CommunicationTests
{
    [TestClass]
    public class TcpClientTest
    {
        private const int PortNum = 15200;

        [TestMethod]
        public void ListenClient()
        {
            var tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect("127.0.0.1", PortNum);
                var networkStream = tcpClient.GetStream();

                if (networkStream.CanWrite && networkStream.CanRead)
                {

                    var dataToSend = "";

                    while (dataToSend != "quit")
                    {

                        //Console.WriteLine("\nType a text to be sent:");
                        dataToSend = "help breakpoint";
                        if (dataToSend.Length == 0) break;

                        Byte[] sendBytes = Encoding.ASCII.GetBytes(dataToSend);
                        networkStream.Write(sendBytes, 0, sendBytes.Length);

                        // Reads the NetworkStream into a byte buffer.
                        var bytes = new byte[tcpClient.ReceiveBufferSize];
                        var bytesRead = networkStream.Read(bytes, 0, (int) tcpClient.ReceiveBufferSize);

                        // Returns the data received from the host to the console.
                        string returndata = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                        //Console.WriteLine("This is what the host returned to you: \r\n{0}", returndata);
                    }
                    networkStream.Close();
                    tcpClient.Close();
                }
                else if (!networkStream.CanRead)
                {
                    Console.WriteLine("You can not write data to this stream");
                    tcpClient.Close();
                }
                else if (!networkStream.CanWrite)
                {
                    Console.WriteLine("You can not read data from this stream");
                    tcpClient.Close();
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Sever not available!");
            }
            catch (System.IO.IOException)
            {
                Console.WriteLine("Sever not available!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        // Main() 
    }

    // class TcpClientTest {
}
