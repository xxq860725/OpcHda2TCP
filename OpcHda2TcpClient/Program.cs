using System;
using System.Data;
using OpcHda2Tcp.Common;
using OpcHda2Tcp.Client;

namespace OpcHda2TcpClient
{
	class Program
	{
		static void Main(string[] args)
		{
			Opc2TCPClient myClient = new Opc2TCPClient("10.132.94.5", 3000, 64 * 1024);
			myClient.AsyncReadcompleted += MyClient_AsyncReadcompleted;
			bool isConnected=myClient.Connect();
			if (isConnected)
			{
				//发送命令到服务器，命令长度较短，暂时没有考虑传输错误
				string message = @"2016-09-25 08:00:00%2016-09-25 20:00:00%5%Arch\H4/PT001.Value,Arch\H4/PT002.Value,Arch\H4/PT003.Value";
				//string zipString = Util.GZipCompressString(message);
				byte[] data = Util.MakeMessage(message);
				myClient.SyncSend(data);
				//读取服务器返回的包头
				//myClient.ReadHeader();
				//读取返回的数据
				myClient.AsyncRead();
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
