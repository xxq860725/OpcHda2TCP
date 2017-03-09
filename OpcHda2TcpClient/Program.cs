using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.IO;

namespace OpcHda2TcpClient
{
	class Program
	{
		static void Main(string[] args)
		{
			TcpClient myClient = new TcpClient();
			myClient.ReceiveBufferSize = 1024 * 1024;
			myClient.Connect(IPAddress.Parse("127.0.0.1"), 3000);
			//发送数据
			NetworkStream ns = myClient.GetStream();
			//FileStream fs = File.Open(".\\a.XML", FileMode.Open);
			//int data = fs.ReadByte();
			//byte[] data = Encoding.UTF8.GetBytes(message);
			string message = "12345678901234567890"+"\r\n";
			List<byte> data = new List<byte> { };
			data.AddRange(Encoding.UTF8.GetBytes(message));
			ns.Write(data.ToArray(), 0, data.Count);
			//fs.Close();
			ns.Close();
			myClient.Close();

			Console.ReadLine();
		}
	}
}
