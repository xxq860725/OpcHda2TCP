using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpcHda2TcpLib;
using System.Net;

namespace OpcHda2TcpServer
{
	class Program
	{
		private static AsyncTCPServer myTcpServer = new AsyncTCPServer(IPAddress.Parse("127.0.0.1"), 3000);//监听本机3000端口
		private static OPCHDAClient myOpcClient = new OPCHDAClient("", "OPCServerHDA.WinCC.1");
		static void Main(string[] args)
		{			
			#region 事件注册

			myTcpServer.ClientConnected += MyTcpServer_ClientConnected;

			myTcpServer.DataReceived += MyTcpServer_DataReceived;

			myTcpServer.CompletedSend += MyTcpServer_CompletedSend;
			#endregion

			myTcpServer.Start();
			Console.WriteLine("服务器已经启动，正在监听中.....");
			while (true)//程序停在这里
			{
				string arg = Console.ReadLine();
				if (arg == "exit") break;
			}
			myTcpServer.Stop();
			Console.WriteLine("服务器已经停止，回车后本程序即可退出.....");
			Console.ReadLine();
		}
		/// <summary>
		/// 接收到数据事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void MyTcpServer_DataReceived(object sender, AsyncEventArgs e)
		{
			TCPClientState state = e._state;
			string a = Encoding.UTF8.GetString(state.Buffer);
			a = a.Trim(new char[] { '\0' });
			if (a.Substring(a.Length - 2, 2) == "\r\n")
			{
				a = a.Substring(0, a.Length - 2);
				Console.WriteLine("接收到:" + state.TcpClient.Client.RemoteEndPoint.ToString() + "\t" + a);
			}
			//分析命令

			//获取数据

			//发送数据
			
		}

		/// <summary>
		/// client 连接建立事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void MyTcpServer_ClientConnected(object sender, AsyncEventArgs e)
		{
			
		}

		/// <summary>
		/// 数据发送完毕事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void MyTcpServer_CompletedSend(object sender, AsyncEventArgs e)
		{
			//发送完毕后关闭连接
			//sever不主动断开连接，让客户端判断后断开连接
			//myTcpServer.Close(e._state);
		}

		#region 方法
		/// <summary>
		/// 发送数据
		/// </summary>
		/// <param name="state"></param>
		/// <param name="data"></param>
		private static void sendDatetoClient(TCPClientState state,byte[] data )
		{
			myTcpServer.Send(state, data);
		}

		private static byte[] readDataFromOpc()
		{
			return new byte[10];
		}
		#endregion
	}
}
