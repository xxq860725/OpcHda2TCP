using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace OpcHda2Tcp
{
	public class AsyncTCPClient
	{
		/// <summary>
		/// 服务器ip
		/// </summary>
		public string ServerIP { get; private set; }

		/// <summary>
		/// 服务器端口
		/// </summary>
		public int ServerPort { get; private set; }

		/// <summary>
		/// 缓冲区大小
		/// </summary>
		public int BufferSize { get;private set; }

		/// <summary>
		/// tcp client
		/// </summary>
		public TcpClient tcpClient { get; private set; }

		/// <summary>
		/// 网络流
		/// </summary>
		public NetworkStream NetworkStream { get; private set; }

		/// <summary>
		/// 缓冲区
		/// </summary>
		public byte[] Buffer;

		/// <summary>
		/// 接收到的数据
		/// </summary>
		public List<byte> RecivedData = new List<byte> { };

		/// <summary>
		///接收的数据长度（字节数） 
		/// </summary>
		private int _dataLength = 0;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="serverip"></param>
		/// <param name="serverport"></param>
		/// <param name="buffersize"></param>
		public AsyncTCPClient(string serverip,int serverport,int buffersize)
		{
			ServerIP = serverip;
			ServerPort = serverport;
			BufferSize = buffersize;
			tcpClient = new TcpClient();
			tcpClient.ReceiveBufferSize = BufferSize;
			Buffer = new byte[BufferSize];			
		}

		/// <summary>
		/// 连接服务器
		/// </summary>
		public bool Connect()
		{
			try
			{
				tcpClient.Connect(IPAddress.Parse(ServerIP), ServerPort);
				this.NetworkStream = tcpClient.GetStream();
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// 同步发送数据
		/// </summary>
		/// <param name="message">客户端发送给服务器的信息</param>
		public void SyncSend(string message)
		{
			List<byte> data = new List<byte> { };
			data.AddRange(Encoding.UTF8.GetBytes(message));
			this.NetworkStream.Write(data.ToArray(), 0, data.Count);
		}

		/// <summary>
		/// 异步读取
		/// </summary>
		public void AsyncRead()
		{
			this.NetworkStream.BeginRead(Buffer, 0, Buffer.Length, asyncread, tcpClient);
		}

		/// <summary>
		/// 关闭
		/// </summary>
		public void Close()
		{
			this.NetworkStream.Close();
			tcpClient.Close();
			RecivedData = null;
			Buffer = null;
		}

		/// <summary>
		/// 读取头包
		/// </summary>
		public void ReadHeader()
		{
			byte[] data = new byte[48];
			int bytes = this.NetworkStream.Read(data, 0, data.Length);
			byte[] byteInt = new byte[4];
			System.Buffer.BlockCopy(data, 0, byteInt, 0,4);
			_dataLength= System.BitConverter.ToInt32(byteInt,0);
		}

		/// <summary>
		/// 异步接收数据回调函数
		/// </summary>
		/// <param name="ar"></param>
		private  void asyncread(IAsyncResult ar)
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
			byte[] buff = new byte[recv];
			System.Buffer.BlockCopy(Buffer, 0, buff,0,recv);
			RecivedData.AddRange(buff);
			Console.WriteLine("RecivedData Count:{0}", RecivedData.Count);
			if (RecivedData.Count >= _dataLength)
			{
				Console.WriteLine("读取完成！");
			}
			else
			{
				stream.BeginRead(Buffer, 0, Buffer.Length, asyncread, client);
			}			
		}
	}
}
