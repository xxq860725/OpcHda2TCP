using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpcHda2Tcp;
using System.Net;
using System.IO;
using System.Data;
using System.Web.Script.Serialization;
using System.IO.Compression;
using System.Threading;
using OpcHda2Tcp.Common;
using OpcHda2Tcp.Server;

namespace OpcHda2TcpServer
{
	struct QueryAndSendPara
	{
		public string cmd;
		public TCPClientState state;
	}
	class Program
	{
		private static AsyncTCPServer myTcpServer = new AsyncTCPServer(IPAddress.Parse("127.0.0.1"), 3000);//监听本机3000端口
		private static OPCHDAClient myOpcClient = new OPCHDAClient("", "OPCServerHDA.WinCC.1");
		static void Main(string[] args)
		{
			Console.WriteLine("输入监听ip：");
			string ip = Console.ReadLine();
			myTcpServer= new AsyncTCPServer(IPAddress.Parse(ip), 3000);
			//禁止关闭按钮
			Util.DisableCloseButton(Console.Title);
			#region 事件注册
			myTcpServer.ClientConnected += MyTcpServer_ClientConnected;
			myTcpServer.DataReceived += MyTcpServer_DataReceived;
			myTcpServer.CompletedSend += MyTcpServer_CompletedSend;
			myTcpServer.ClientDisconnected += MyTcpServer_ClientDisconnected;
			#endregion
		
			myTcpServer.Start();
			Console.WriteLine("服务器已经启动，正在监听中.....");
			while (true)//程序停在这里
			{
				string arg = Console.ReadLine();
				if (arg == "exit") break;
			}
			myTcpServer.CloseAllClient();
			myTcpServer.Stop();
			Console.WriteLine("任务已经结束,按回车退出程序...");
			Console.ReadLine();

		}
		/// <summary>
		/// 客户端断开连接事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void MyTcpServer_ClientDisconnected(object sender, AsyncEventArgs e)
		{
			Console.WriteLine(  "客户端断开连接，当前连接数："+myTcpServer.ClientCount);
		}

		/// <summary>
		/// 接收到数据事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void MyTcpServer_DataReceived(object sender, AsyncEventArgs e)
		{
			TCPClientState state = e._state;

			string clientCmdString = ""; //= Encoding.UTF8.GetString(state.Buffer);
			byte[] data = new byte[state.Buffer.Length];
			Buffer.BlockCopy(state.Buffer, 0, data, 0, state.Buffer.Length);
			bool bsucceed= Util.VeryfyMessage(data, out clientCmdString);
			if (!bsucceed) return;
			//clientCmdString = clientCmdString.Trim(new char[] { '\0' });
			Console.WriteLine("接收到:" + state.TcpClient.Client.RemoteEndPoint.ToString() + "\t" + clientCmdString);
			QueryAndSendPara pra = new QueryAndSendPara();
			pra.cmd = clientCmdString;
			pra.state = state;
			Thread thread = new Thread(new ParameterizedThreadStart(QueryAndSend));
			thread.Start(pra);
		}

		/// <summary>
		/// client 连接建立事件
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private static void MyTcpServer_ClientConnected(object sender, AsyncEventArgs e)
		{
			Console.WriteLine("新的客户端加入，当前连接数：{0}", myTcpServer.ClientCount);
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
		private static void sendDatatoClient(TCPClientState state,byte[] data )
		{
			myTcpServer.Send(state, data);
		}

		/// <summary>
		/// 分析命令
		/// </summary>
		/// <param name="cmd"></param>
		/// <param name="starttime"></param>
		/// <param name="endtime"></param>
		/// <param name="step"></param>
		/// <param name="tags"></param>
		private static void analisisCmd(string cmd,out string starttime, out string endtime, out string step, out List<string> tags)
		{
			starttime = endtime=step=string.Empty;
			tags = new List<string> { };
			if (cmd.Split('%').Length != 4)
			{
				Console.WriteLine("命令错误！");
				return;
			}
			starttime = cmd.Split('%')[0];
			endtime= cmd.Split('%')[1];
			step= cmd.Split('%')[2];
			tags.AddRange(cmd.Split('%')[3].Split(','));
		}

		/// <summary>
		/// 读取opc 数据
		/// </summary>
		/// <param name="starttime"></param>
		/// <param name="endtime"></param>
		/// <param name="step"></param>
		/// <param name="tags"></param>
		/// <returns></returns>
		private static string readDataFromOpc(string starttime,string endtime,string step,List<string>tags)
		{
			string serverName = "OPCServerHDA.WinCC.1";
			//HDA主机名
			string hostName = ""; //"WIN-NVSKV8UAC6U";
			hostName = Dns.GetHostName();
			//创建一个客户端
			var hdac = new OPCHDAClient(hostName,serverName);//OPCHDA.Client();
			try
			{
				bool _connected = hdac.Connect();
				//添加点
				foreach (string tag in tags)
				{
					hdac.AddItem(@tag);
				}
				DateTime start = DateTime.Parse(starttime);
				DateTime end = DateTime.Parse(endtime);
				//读取数据到datatable    
				DataTable dt = hdac.ReadByInterval(start, end, int.Parse(step), true);
				//序列化为json			
				string strJson = Util.DataTableToJson(dt);
				hdac.Disconnect();
				//strJson =Util.GZipCompressString(strJson);
				return strJson;//Encoding.UTF8.GetBytes(strJson);
			}
			catch (Exception ex)
			{				
				Console.WriteLine(ex.Message);
				return"";
			}
		}

		/// <summary>
		/// 查询并返回数据到客户端
		/// </summary>
		/// <param name="clientCmdString"></param>
		/// <param name="state"></param>
		private static void QueryAndSend(object para)
		{

			QueryAndSendPara qPara;
			if (para is QueryAndSendPara)
			{
				qPara = (QueryAndSendPara)para;
			}
			else
			{
				return;//传入的参数不正确时直接返回，不回应。
			}
			//分析命令
			string starttime, endtime, step;
			List<string> tags;
			analisisCmd(qPara.cmd, out starttime, out endtime, out step, out tags);
			//获取数据(已经序列化并压缩)
			string  message = readDataFromOpc(starttime, endtime, step, tags);
			//发送数据
			byte[] sendData = Util.MakeMessage(message);
			sendDatatoClient(qPara.state, sendData);
			Console.WriteLine("发送ZIP压缩数据：{0}", sendData.Length);
		}
		#endregion
	}
}
