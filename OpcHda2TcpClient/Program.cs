using System;
using System.Data;
using OpcHda2Tcp.Common;
using OpcHda2Tcp.Client;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace OpcHda2TcpClient
{
	class Program
	{
		static void Main(string[] args)
		{
			Opc2TCPClient myClient = new Opc2TCPClient("10.132.94.5", 3000, 64 * 1024);
			//myClient.AsyncReadcompleted += MyClient_AsyncReadcompleted;
			bool isConnected=myClient.Connect();
			if (isConnected)
			{
				FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "\\tags.tl", FileMode.Open);
				StreamReader sr = new StreamReader(fs, Encoding.Default);
				List<string> tags = new List<string> { };
				string line = sr.ReadLine();
				string message = "";
				while (line != null)
				{
					message = message + line + ",";
				}
				//发送命令到服务器，命令长度较短，暂时没有考虑传输错误
				message = @"2016-09-25 08:00:00%2016-09-25 20:00:00%3600%Arch\H4/PT001.Value,Arch\H4/PT002.Value,Arch\H4/PT003.Value";
				//string zipString = Util.GZipCompressString(message);
				byte[] data = Util.MakeMessage(message);
				myClient.SyncSend(data);
				//读取服务器返回的包头
				//myClient.ReadHeader();
				//读取返回的数据
				myClient.AsyncRead();
				while (true)
				{
					if (myClient.RecivedAll)
					{
						Console.WriteLine("收到读取完毕事件！");
						string msg = "";
						bool b = Util.VeryfyMessage(myClient.RecivedData.ToArray(), out msg);
						if (!b)
						{
							Console.WriteLine("验证消息失败");
						}
						else
						{
							Console.WriteLine("解压缩前的数据长度{0}", myClient.RecivedData.Count);
							Console.WriteLine("解压缩后数据长度：{0}", msg.Length);
							DataTable dt = Util.JsonToDataTable(msg);
							Console.WriteLine("反序列化输出行数：{0}", dt.Rows.Count);
						}
						break;					
					}
					Thread.Sleep(100);
				}
			}
			Console.ReadLine();
			myClient.Close();
		}

		private static void MyClient_AsyncReadcompleted(object sender, AsyncEventArgs e)
		{
			Console.WriteLine("收到读取完毕事件！");
			string msg = "";
			bool b=Util.VeryfyMessage(e._data, out msg);
			if (!b)
			{
				Console.WriteLine("验证消息失败");
			}
			else
			{
				Console.WriteLine("解压缩前的数据长度{0}",e._data.Length);
				Console.WriteLine("解压缩后数据长度：{0}", msg.Length);
				DataTable dt = Util.JsonToDataTable(msg);
				Console.WriteLine("反序列化输出行数：{0}", dt.Rows.Count);
			}
		}
	}
}
