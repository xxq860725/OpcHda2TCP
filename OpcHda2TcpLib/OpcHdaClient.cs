using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Opc.Hda;
using System.Data;
using System.Net;

namespace OpcHda2Tcp
{
	public class OPCHDAClient
	{
		private Server _hdaServer = null;
		private string _hostName, _serverName;

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="hostName"></param>
		/// <param name="serverName"></param>
		public OPCHDAClient(string hostName, string serverName)
		{
			if (hostName == "") hostName = Dns.GetHostName();
			_hostName = hostName;
			_serverName = serverName;
		}

		/// <summary>
		/// 连接HDA服务器
		/// </summary>
		/// <param name="hostName">主机名称或者IP</param>
		/// <param name="serverName">服务器名称</param>
		public bool Connect()
		{
			Opc.URL url = new Opc.URL(String.Format("opchda://" + _hostName + @"/{0}", _serverName));
			OpcCom.Factory fact = new OpcCom.Factory();
			_hdaServer = new Opc.Hda.Server(fact, url);
			try
			{
				_hdaServer.Connect();
				//Console.WriteLine(String.Format("Connect to server {0}", serverName));
			}
			catch (Opc.ConnectFailedException opcConnExc)
			{
				Console.WriteLine(String.Format("Could not connect to server {0}", _serverName));
				Console.WriteLine(opcConnExc.ToString());
				//return false;
			}
			return _hdaServer.IsConnected ? true : false;
			//Console.WriteLine("Are we connected? " + _hdaServer.IsConnected);
		}

		/// <summary>
		/// 添加item到HDA服务器
		/// </summary>
		/// <param name="itemName">item 名称</param>
		public void AddItem(string itemName)
		{
			if (_hdaServer != null)
			{
				Opc.ItemIdentifier itemIdentifier = new Opc.ItemIdentifier(itemName);
				Opc.ItemIdentifier[] items = { itemIdentifier };
				Opc.IdentifiedResult[] addItemResults = _hdaServer.CreateItems(items);
				Opc.IdentifiedResult[] validateItemResults = _hdaServer.ValidateItems(items);
				//Console.WriteLine("Item Added: " + itemName);
			}
		}

		/// <summary>
		/// 读取原始数据
		/// </summary>
		/// <param name="startTime">开始时间</param>
		/// <param name="endTime">结束时间</param>
		/// <param name="maxValues">最多读取的点数，0表示全部</param>
		/// <param name="inclubeBounds">是否包含边界</param>
		/// <returns></returns>
		public DataTable ReadRaw(DateTime startTime, DateTime endTime, int maxValues, bool inclubeBounds)
		{
			Opc.Hda.Time hdaStartTime = new Time(startTime);
			Opc.Hda.Time hdaEndTime = new Time(endTime);
			//DataSet dsResult = new DataSet();
			DataTable dataTable = new DataTable("DATA");
			DataColumn timestamp = new DataColumn("TimeStamp");
			DataColumn TagName = new DataColumn("TagName");
			DataColumn value = new DataColumn("Value");
			DataColumn quality = new DataColumn("Quality");
			dataTable.Columns.Add(timestamp);
			dataTable.Columns.Add(TagName);
			dataTable.Columns.Add(value);
			dataTable.Columns.Add(quality);

			Opc.ItemIdentifierCollection itemIdentifierCollection = null;
			Opc.ItemIdentifier[] items = null;
			int index = 0;
			if (_hdaServer.Items.Count != 0)
			{
				itemIdentifierCollection = _hdaServer.Items;
				items = new Opc.ItemIdentifier[itemIdentifierCollection.Count];
				Console.WriteLine("{0} Tags will read", itemIdentifierCollection.Count);
			}
			Opc.Hda.Trend group = new Opc.Hda.Trend(_hdaServer);
			group.Name = String.Format("{0}-{1}", group.Server.Url.HostName, Guid.NewGuid().ToString());
			group.EndTime = new Opc.Hda.Time(endTime);
			group.StartTime = new Opc.Hda.Time(startTime);
			TimeSpan span = endTime.Subtract(startTime);
			int calcinterval = ((int)span.TotalSeconds);
			group.ResampleInterval = (decimal)calcinterval;//calcinterval
			group.AggregateID = Opc.Hda.AggregateID.NOAGGREGATE;//Opc.Hda.AggregateID.DURATIONGOOD;
			group.MaxValues = maxValues;
			//将item添加到trend
			foreach (Opc.ItemIdentifier itemIdentifier in itemIdentifierCollection)
			{
				group.Items.Clear();//清空item
				items[index] = itemIdentifier;
				Opc.IdentifiedResult[] results = group.Server.ValidateItems(new Opc.ItemIdentifier[] { itemIdentifier });
				group.AddItem(itemIdentifier);
				ItemValueCollection[] values = group.ReadRaw();
				foreach (ItemValueCollection itemValueCollection in values)
				{
					foreach (ItemValue itemValue in itemValueCollection)
					{
						DataRow dataRow = dataTable.NewRow();
						dataRow["Timestamp"] = itemValue.Timestamp;
						dataRow["TagName"] = itemIdentifier.ItemName;
						dataRow["Value"] = itemValue.Value;
						dataRow["Quality"] = itemValue.Quality;
						dataTable.Rows.Add(dataRow);
					}
				}
				index++;
			}
			Console.WriteLine("return data:{0} rows", dataTable.Rows.Count);
			return dataTable;
		}

		/// <summary>
		/// 按照时间间隔读取数据
		/// </summary>
		/// <param name="startTime">开始时间</param>
		/// <param name="endTime">结束时间</param>
		/// <param name="interVal">时间间隔（秒）</param>
		/// <param name="inclubeBounds">是否包含边界</param>
		/// <returns></returns>
		public DataTable ReadByInterval(DateTime startTime, DateTime endTime, int interVal, bool inclubeBounds)
		{
			Opc.Hda.Time hdaStartTime = new Time(startTime);
			Opc.Hda.Time hdaEndTime = new Time(endTime);
			//DataSet dsResult = new DataSet();
			DataTable dataTable = new DataTable("DATA");
			DataColumn timestamp = new DataColumn("TimeStamp");
			DataColumn TagName = new DataColumn("TagName");
			DataColumn value = new DataColumn("Value");
			DataColumn quality = new DataColumn("Quality");
			dataTable.Columns.Add(timestamp);
			dataTable.Columns.Add(TagName);
			dataTable.Columns.Add(value);
			dataTable.Columns.Add(quality);

			Opc.ItemIdentifierCollection itemIdentifierCollection = null;
			Opc.ItemIdentifier[] items = null;
			int index = 0;
			if (_hdaServer.Items.Count != 0)
			{
				itemIdentifierCollection = _hdaServer.Items;
				items = new Opc.ItemIdentifier[itemIdentifierCollection.Count];
				Console.WriteLine("{0} Tags will read", itemIdentifierCollection.Count);
			}
			Opc.Hda.Trend group = new Opc.Hda.Trend(_hdaServer);
			group.Name = String.Format("{0}-{1}", group.Server.Url.HostName, Guid.NewGuid().ToString());
			group.EndTime = new Opc.Hda.Time(endTime);
			group.StartTime = new Opc.Hda.Time(startTime);
			//TimeSpan span = endTime.Subtract(startTime);
			//int calcinterval = interVal;
			group.ResampleInterval = interVal;//calcinterval
			group.AggregateID = Opc.Hda.AggregateID.NOAGGREGATE;//Opc.Hda.AggregateID.DURATIONGOOD;
			group.MaxValues = 0;
			//将item添加到trend
			foreach (Opc.ItemIdentifier itemIdentifier in itemIdentifierCollection)
			{
				group.Items.Clear();//清空item
				items[index] = itemIdentifier;
				Opc.IdentifiedResult[] results = group.Server.ValidateItems(new Opc.ItemIdentifier[] { itemIdentifier });
				group.AddItem(itemIdentifier);
				ItemValueCollection[] values = group.ReadProcessed();
				foreach (ItemValueCollection itemValueCollection in values)
				{
					foreach (ItemValue itemValue in itemValueCollection)
					{
						DataRow dataRow = dataTable.NewRow();
						dataRow["Timestamp"] = itemValue.Timestamp;
						dataRow["TagName"] = itemIdentifier.ItemName;
						dataRow["Value"] = itemValue.Value;
						dataRow["Quality"] = itemValue.Quality;
						dataTable.Rows.Add(dataRow);
					}
				}
				index++;
			}
			Console.WriteLine("return data:{0} rows", dataTable.Rows.Count);
			return dataTable;
		}

		/// <summary>
		/// 断开连接
		/// </summary>
		public void Disconnect()
		{
			if (_hdaServer != null && _hdaServer.IsConnected)
				_hdaServer.Disconnect();
		}
	}
}
