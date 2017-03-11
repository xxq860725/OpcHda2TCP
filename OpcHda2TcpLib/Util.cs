using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace OpcHda2Tcp
{
	/// <summary>
	/// 静态工具类
	/// </summary>
	public static class Util
	{
		/// <summary>
		/// Gzip 压缩
		/// </summary>
		/// <param name="rawString"></param>
		/// <returns></returns>
		public  static string GZipCompressString(string rawString)
		{
			if (string.IsNullOrEmpty(rawString) || rawString.Length == 0)
			{
				return "";
			}
			else
			{
				byte[] rawData = System.Text.Encoding.UTF8.GetBytes(rawString.ToString());
				byte[] zippedData = Compress(rawData);
				return Convert.ToBase64String(zippedData);
			}
		}

		/// <summary>
		/// GZip解压缩
		/// </summary>
		/// <param name="zippedString">经GZip压缩后的二进制字符串</param>
		/// <returns>原始未压缩字符串</returns>
		public static string GZipDecompressString(string zippedString)
		{
			if (string.IsNullOrEmpty(zippedString) || zippedString.Length == 0)
			{
				return "";
			}
			else
			{
				byte[] zippedData = Convert.FromBase64String(zippedString.ToString());
				return (string)(System.Text.Encoding.UTF8.GetString(Decompress(zippedData)));
			}
		}

		/// <summary>
		/// 数据表序列化为json
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static string DataTableToJson(DataTable dt)
		{
			string strResult = "";
			JavaScriptSerializer s = new JavaScriptSerializer();
			s.MaxJsonLength = int.MaxValue;
			List<dataRow> rows = new List<dataRow> { };
			foreach (DataRow row in dt.Rows)
			{
				rows.Add(new dataRow { TimeStamp = row["Timestamp"].ToString(), TagName = row["TagName"].ToString(), Value = row["Value"].ToString(), Quality = row["Quality"].ToString() });
			}
			strResult = s.Serialize(rows);
			return strResult;
		}

		/// <summary>
		/// 反序列化json为datatable
		/// </summary>
		/// <param name="jsonString"></param>
		/// <returns></returns>
		public static DataTable JsonToDataTable(string jsonString)
		{
			DataTable dtEmpty = new DataTable();
			DataTable dt = new DataTable();
			dt.Columns.Add("Timestamp");
			dt.Columns.Add("TagName");
			dt.Columns.Add("Value");
			dt.Columns.Add("Quality");
			dtEmpty = dt.Clone();
			List<dataRow> dataRows = new List<dataRow> { };
			if (jsonString == "") return dtEmpty;
			JavaScriptSerializer s = new JavaScriptSerializer();
			s.MaxJsonLength = int.MaxValue;
			try
			{
				dataRows = s.Deserialize<List<dataRow>>(jsonString);
				foreach (dataRow dr in dataRows)
				{
					DataRow DR = dt.NewRow();
					DR["Timestamp"] = dr.TimeStamp;
					DR["TagName"] = dr.TagName;
					DR["Value"] = float.Parse(dr.Value);
					DR["Quality"] = dr.Quality;
					dt.Rows.Add(DR);
				}
			}
			catch
			{
				return dtEmpty;
			}
			return dt;
		}

		#region 私有方法
		/// <summary>
		/// ZIP压缩
		/// </summary>
		/// <param name="rawData"></param>
		/// <returns></returns>
		private static byte[] Compress(byte[] rawData)
		{
			MemoryStream ms = new MemoryStream();
			GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Compress, true);
			compressedzipStream.Write(rawData, 0, rawData.Length);
			compressedzipStream.Close();
			return ms.ToArray();
		}

		/// <summary>
		/// ZIP解压
		/// </summary>
		/// <param name="zippedData"></param>
		/// <returns></returns>
		private static byte[] Decompress(byte[] zippedData)
		{
			MemoryStream ms = new MemoryStream(zippedData);
			GZipStream compressedzipStream = new GZipStream(ms, CompressionMode.Decompress);
			MemoryStream outBuffer = new MemoryStream();
			byte[] block = new byte[1024];
			while (true)
			{
				int bytesRead = compressedzipStream.Read(block, 0, block.Length);
				if (bytesRead <= 0)
					break;
				else
					outBuffer.Write(block, 0, bytesRead);
			}
			compressedzipStream.Close();
			return outBuffer.ToArray();
		}
		#endregion
	}
}
