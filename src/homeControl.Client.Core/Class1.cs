using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace homeControl.Client.Core
{
    public class Class1
    {
        public Class1()
        {
            // Create one SocketPermission for socket access restrictions 
            SocketPermission permission = new SocketPermission(
                NetworkAccess.Connect,    // Connection permission 
                TransportType.Tcp,        // Defines transport types 
                "",                       // Gets the IP addresses 
                SocketPermission.AllPorts // All ports 
                );

            // Ensures the code to have permission to access a Socket 
            permission.Demand();

            // Resolves a host name to an IPHostEntry instance            
            IPHostEntry ipHost = Dns.GetHostEntry("");

            // Gets first IP address associated with a localhost 
            IPAddress ipAddr = ipHost.AddressList[0];

            // Creates a network endpoint 
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4510);

            // Create one Socket object to setup Tcp connection 
            senderSock = new Socket(
                ipAddr.AddressFamily,// Specifies the addressing scheme 
                SocketType.Stream,   // The type of socket  
                ProtocolType.Tcp     // Specifies the protocols  
                );

            senderSock.NoDelay = false;   // Using the Nagle algorithm 

            // Establishes a connection to a remote host 
            senderSock.Connect(ipEndPoint);
            tbStatus.Text = "Socket connected to " + senderSock.RemoteEndPoint.ToString();

            Connect_Button.IsEnabled = false;
            Send_Button.IsEnabled = true;
        }
    }
}
