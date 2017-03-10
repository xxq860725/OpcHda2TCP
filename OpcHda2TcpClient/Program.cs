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
		//static int ll = 0;
		static List<byte> reciveData = new List<byte> { };
		static void Main(string[] args)
		{
			AsyncTCPClient myClient = new AsyncTCPClient("127.0.0.1", 3000, 64 * 1024);
			myClient.AsyncReadcompleted += MyClient_AsyncReadcompleted;
			bool isConnected=myClient.Connect();
			if (isConnected)
			{
				//发送命令到服务器，命令长度较短，暂时没有考虑传输错误
				myClient.SyncSend("111222333444555666777888999000");
				//读取服务器返回的包头
				myClient.ReadHeader();
				//读取返回的数据
				myClient.AsyncRead();
			}
			Console.ReadLine();
			myClient.Close();
		}

		private static void MyClient_AsyncReadcompleted(object sender, AsyncEventArgs e)
		{
			Console.WriteLine("收到读取完毕事件！");
			Console.WriteLine("收到数据长度:{0}",e._msg.Length);
		}
	}
}
