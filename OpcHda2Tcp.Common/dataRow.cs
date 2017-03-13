using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpcHda2Tcp
{
	public class dataRow
	{
		public string TimeStamp { get; set; }
		public string TagName { get; set; }
		public string Value { get; set; }
		public string Quality { get; set; }
	}
}
