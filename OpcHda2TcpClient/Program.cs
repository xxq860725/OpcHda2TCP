using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;
using System.Threading;
using OpcHda2Tcp;
namespace OpcHda2TcpClient
{
	class Program
	{
		static int ll = 0;
		static List<byte> reciveData = new List<byte> { };
		static void Main(string[] args)
		{
			AsyncTCPClient myClient = new AsyncTCPClient("127.0.0.1", 3000, 64 * 1024);
			bool isConnected=myClient.Connect();
			if (isConnected)
			{
				myClient.SyncSend("111222333444555666777888999000");
				myClient.ReadHeader();
				myClient.AsyncRead();
			}
			Console.ReadLine();
			myClient.Close();
			//TcpClient myClient = new TcpClient();
			//myClient.ReceiveBufferSize = 16 * 1024;
			//myClient.Connect(IPAddress.Parse("127.0.0.1"), 3000);
			////发送数据
			//NetworkStream ns = myClient.GetStream();
			//string message = "12345678901234567890" + "\r\n";
			//List<byte> data = new List<byte> { };
			//data.AddRange(Encoding.UTF8.GetBytes(message));
			//ns.Write(data.ToArray(), 0, data.Count);
			////接收数据
			//byte[] buffer = new byte[myClient.ReceiveBufferSize];
			//ns.BeginRead(buffer, 0, buffer.Length, asyncread, myClient);
			//Console.WriteLine("按回车退出.....");
			//Console.ReadLine();		
			//ns.Close();
			//myClient.Close();

		}
		private static void asyncread(IAsyncResult ar)
		{
			TcpClient client = (TcpClient)ar.AsyncState;
			if (!client.Connected) return;
			NetworkStream stream = client.GetStream();
			int recv = 0;
			try
			{
				recv = stream.EndRead(ar);
			}
			catch
			{
				recv = 0;
			}
			ll += recv;
			Console.WriteLine("ll={0}", ll);
			byte[] buff = new byte[recv];
			stream.Read(buff, 0, recv);
			reciveData.AddRange(buff);
			//Buffer.BlockCopy(state.Buffer, 0, buff, 0, recv);
			//触发数据收到事件
			//RaiseDataReceived(state);
			// continue listening for tcp datagram packets
			buff = new byte[client.ReceiveBufferSize];
			stream.BeginRead(buff, 0, buff.Length, asyncread, client);
		}
	}
}
